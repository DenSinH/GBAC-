using System;
using System.IO;
using System.Linq;
using GBAEmulator.Memory.IO;

namespace GBAEmulator.Memory.Backup
{
    class BackupEEPROM : IBackup
    {
        /*
         From GBATek:
            **** Using DMA ****
            Transferring a bitstream to/from the EEPROM by LDRH/STRH opcodes does not work, this might be because of timing problems,
        or because how the GBA squeezes non-sequential memory addresses through the external address/data bus.
        For this reason, a buffer in memory must be used (that buffer would be typically allocated temporarily on stack,
        one halfword for each bit, bit1-15 of the halfwords are don't care, only bit0 is of interest).
            The buffer must be transfered as a whole to/from EEPROM by using DMA3 (only DMA 3 is valid to read & write external memory),
        use 16bit transfer mode, both source and destination address incrementing (ie. DMA3CNT=80000000h+length).
        DMA channels of higher priority should be disabled during the transfer (ie. H/V-Blank or Sound FIFO DMAs).
        And, of course any interrupts that might mess with DMA registers should be disabled.

        "Only DMA3 is valid for external memory": if EEPROMSize = 0, and a bit is written, we read the unit count of DMA3 and set the 
                                                  EEPROMSize accordingly.
             */

        cDMACNT_H DMA3CNT_H;
        cDMACNT_L DMA3CNT_L;
        public BackupEEPROM(cDMACNT_H DMA3CNT_H, cDMACNT_L DMA3CNT_L)
        {
            this.DMA3CNT_H = DMA3CNT_H;
            this.DMA3CNT_L = DMA3CNT_L;
        }

        private enum AccessType : byte
        {
            Read = 0b11,
            Write = 0b10
        }

        private const int ReadBitCounterReadReset = 68;

        private uint ReadAddress;
        private uint WriteAddress;
        private uint Buffer;
        private AccessType Access;
        private int WriteBitCounter;
        private int ReadBitCounter = ReadBitCounterReadReset;

        private byte BusSize;  // either *6* bit bus (512B/0x200) or *14* bit bus (8kB/0x2000)
        private uint Size;     // either 0x200 or 0x2000

        byte[] Storage = new byte[0x8000];

        public void Init()
        {
            this.ReadAddress = 0;
            this.WriteAddress = 0;
            this.Buffer = 0;
            this.WriteBitCounter = 0;
            this.ReadBitCounter = ReadBitCounterReadReset;
            this.BusSize = 0;
            this.Size = 0;

            for (int i = 0; i < 0x8000; i++)
            {
                Storage[i] = 0xff;
            }
        }
        public void Dump(string FileName)
        {
            try
            {
                File.WriteAllBytes(FileName, this.Storage);
            }
            catch (Exception e)
            {
                // something went wrong
                Console.Error.WriteLine("Something went wrong while dumping the save data... " + e.Message);
            }
        }

        public void Load(string FileName)
        {
            try
            {
                this.Storage = File.ReadAllBytes(FileName);
            }
            catch (Exception e)
            {
                // something went wrong
                Console.Error.WriteLine("Something went wrong while dumping the save data... " + e.Message);
            }
        }

        public byte Read(uint address)
        {
            if (this.Access == AccessType.Write)
            {
                /*
                 GBATek: After the DMA, keep reading from the chip,
                         by normal LDRH [DFFFF00h], until Bit 0 of the returned data becomes "1" (Ready).    
                */
                return 1;
            }
            else if (this.Access == AccessType.Read)
            {
                /*
                    GBATek:
                Read a stream of 68 bits from EEPROM by using DMA,
                then decipher the received data as follows:
                    4 bits - ignore these
                    64 bits - data (conventionally MSB first)
                 */
                ReadBitCounter--;
                if (ReadBitCounter > 63)
                {
                    return 1;  // ignore these
                }

                // MSB first (BitCounter == 7 (mod 8) when we first get here)
                byte value = (byte)((this.Storage[ReadAddress] >> (ReadBitCounter & 7)) & 1);
                // Console.WriteLine($"Read Access {EEPROMReadBitCounter}, got {value} from {EEPROMReadAddress.ToString("x4")} ({this.Storage[EEPROMReadAddress].ToString("x2")})");

                if ((ReadBitCounter & 7) == 0)
                {
                    ReadAddress++;  // move to next byte if BitCounter == 0 mod 8

                    if (ReadBitCounter == 0)
                    {
                        // read done
                        ReadBitCounter = ReadBitCounterReadReset;
                    }
                }

                return value;
            }
            else
            {
                Console.Error.WriteLine($"Invalid EEPROM access mode: {this.Access}");
                return 1;
            }
        }

        public bool Write(uint address, byte value)
        {
            if (this.Size == 0 && WriteBitCounter == 0)
            {
                // 9 bits to set access for 512B EEPROM, 17 for 6kB
                if (this.DMA3CNT_H.Active && this.DMA3CNT_L.UnitCount > 9)
                {
                    // 8kB
                    Size = 0x2000;
                    BusSize = 14;
                }
                else
                {
                    // 512B
                    Size = 0x200;
                    BusSize = 6;
                }
            }

            Buffer <<= 1;
            Buffer |= (uint)(value & 1);
            WriteBitCounter++;

            if (WriteBitCounter == 2)
            {
                // command write complete
                Access = (AccessType)(Buffer & 3);
                // this.Log("Set EEPROM access type to " + this.EEPROMAccess);
            }
            else if (WriteBitCounter == 2 + BusSize)
            {
                // address write complete
                /*
                 writes happen in units of 8 bytes at a time, so we must account for this in the address
                 */
                if (Access == AccessType.Read)
                {
                    ReadAddress = (uint)(Buffer << 3) & (Size - 1);
                }
                else
                {
                    WriteAddress = (uint)(Buffer << 3) & (Size - 1);
                }
            }
            else if (WriteBitCounter > 2 + BusSize)
            {
                if (Access == AccessType.Write)
                {
                    // first time we get here, WriteBitCounter will be 63 (== 7 mod 8) and counting down
                    int WriteBitCounter = (64 + BusSize + 2 - this.WriteBitCounter);
                    if (WriteBitCounter < 0)
                    {
                        // last bit (expect 0)
                        // reset buffer values
                        Buffer = 0;  // this doesn't actually matter, but just to be sure
                        this.WriteBitCounter = 0;
                        return false;
                    }

                    if ((WriteBitCounter & 7) == 0)  // 0 mod 8, proceed to next byte
                    {
                        // we might as well just write one bit at a time, we have the buffer after all
                        this.Storage[WriteAddress++] = (byte)Buffer;
                        // Console.WriteLine($"Write byte {((byte)Buffer).ToString("x4")} to {(WriteAddress - 1).ToString("x4")}");
                        return true;
                    }
                }
                else if (Access == AccessType.Read)
                {
                    // expect 0 as final bit write for Read, I don't really care about this
                    // this.Log($"Set read EEPROM read address to {EEPROMReadAddress.ToString("x4")}");

                    // ready values values
                    ReadBitCounter = ReadBitCounterReadReset;  // reuse for reads
                    WriteBitCounter = 0;
                    Buffer = 0;
                }
                else
                {
                    Console.Error.WriteLine($"Something went wrong, EEPROMBitCounter overflow ({WriteBitCounter}) in invalid mode {Access}, resetting...");
                    WriteBitCounter = 0;
                }
            }

            return false;
        }

    }
}
