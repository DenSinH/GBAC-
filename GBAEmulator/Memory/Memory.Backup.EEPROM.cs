using System;

namespace GBAEmulator.Memory
{
    partial class MEM
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

        private enum EEPROMAccessType : byte
        {
            Read = 0b11,
            Write = 0b10
        }

        private const int EEPROMReadBitCounterReadReset = 68;

        private uint EEPROMReadAddress;
        private uint EEPROMWriteAddress;
        private uint EEPROMBuffer;
        private EEPROMAccessType EEPROMAccess;
        private int EEPROMWriteBitCounter;
        private int EEPROMReadBitCounter = EEPROMReadBitCounterReadReset;

        private byte EEPROMBusSize;  // either *6* bit bus (512B/0x200) or *14* bit bus (8kB/0x2000)
        private uint EEPROMSize;     // either 0x200 or 0x2000

        public void InitEEPROM()
        {
            this.EEPROMReadAddress = 0;
            this.EEPROMWriteAddress = 0;
            this.EEPROMBuffer = 0;
            this.EEPROMWriteBitCounter = 0;
            this.EEPROMReadBitCounter = EEPROMReadBitCounterReadReset;
            this.EEPROMBusSize = 0;
            this.EEPROMSize = 0;
        }

        public byte EEPROMRead()
        {
            if (this.EEPROMAccess == EEPROMAccessType.Write)
            {
                /*
                 GBATek: After the DMA, keep reading from the chip,
                         by normal LDRH [DFFFF00h], until Bit 0 of the returned data becomes "1" (Ready).    
                */
                return 1;
            }
            else if (this.EEPROMAccess == EEPROMAccessType.Read)
            {
                /*
                    GBATek:
                Read a stream of 68 bits from EEPROM by using DMA,
                then decipher the received data as follows:
                    4 bits - ignore these
                    64 bits - data (conventionally MSB first)
                 */
                EEPROMReadBitCounter--;
                if (EEPROMReadBitCounter > 63)
                {
                    return 1;  // ignore these
                }

                // MSB first (BitCounter == 7 (mod 8) when we first get here)
                byte value = (byte)((this.BackupStorage[EEPROMReadAddress] >> (EEPROMReadBitCounter & 7)) & 1);
                this.Log($"Read Access {EEPROMReadBitCounter}, got {value} from {EEPROMReadAddress.ToString("x4")} ({this.BackupStorage[EEPROMReadAddress].ToString("x2")})");
                
                if ((EEPROMReadBitCounter & 7) == 0)
                {
                    EEPROMReadAddress++;  // move to next byte if BitCounter == 0 mod 8
                    
                    if (EEPROMReadBitCounter == 0)
                    {
                        // read done
                        EEPROMReadBitCounter = EEPROMReadBitCounterReadReset;
                    }
                }

                return value;
            }
            else
            {
                this.Error($"Invalid EEPROM access mode: {this.EEPROMAccess}");
                return 1;
            }
        }
        
        public void EEPROMWrite(byte value)
        {
            this.Log($"EEPROM Write {value} in EEPROM mode {this.EEPROMAccess}");

            if (this.EEPROMSize == 0 && EEPROMWriteBitCounter == 0)
            {
                // 9 bits to set access for 512B EEPROM, 17 for 6kB
                if (this.DMACNT_H[3].Active && this.DMACNT_L[3].UnitCount > 9)
                {
                    // 8kB
                    EEPROMSize = 0x2000;
                    EEPROMBusSize = 14;
                }
                else
                {
                    // 512B
                    EEPROMSize = 0x200;
                    EEPROMBusSize = 6;
                }
            }

            EEPROMBuffer <<= 1;
            EEPROMBuffer |= (uint)(value & 1);
            EEPROMWriteBitCounter++;
            
            if (EEPROMWriteBitCounter == 2)
            {
                // command write complete
                EEPROMAccess = (EEPROMAccessType)(EEPROMBuffer & 3);
                this.Log("Set EEPROM access type to " + this.EEPROMAccess);
            }
            else if (EEPROMWriteBitCounter == 2 + EEPROMBusSize)
            {
                // address write complete
                /*
                 writes happen in units of 8 bytes at a time, so we must account for this in the address
                 */
                if (EEPROMAccess == EEPROMAccessType.Read)
                {
                    EEPROMReadAddress = (uint)(EEPROMBuffer & (EEPROMSize - 1)) << 3;
                }
                else
                {
                    EEPROMWriteAddress = (uint)(EEPROMBuffer & (EEPROMSize - 1)) << 3;
                }
            }
            else if (EEPROMWriteBitCounter > 2 + EEPROMBusSize)
            {
                if (EEPROMAccess == EEPROMAccessType.Write)
                {
                    // first time we get here, WriteBitCounter will be 63 (== 7 mod 8) and counting down
                    int WriteBitCounter = (64 + EEPROMBusSize + 2 - EEPROMWriteBitCounter);
                    if (WriteBitCounter < 0)
                    {
                        // last bit (expect 0)
                        // reset buffer values
                        EEPROMBuffer = 0;  // this doesn't actually matter, but just to be sure
                        EEPROMWriteBitCounter = 0;
                        return;
                    }

                    if ((WriteBitCounter & 7) == 0)  // 0 mod 8, proceed to next byte
                    {
                        // we might as well just write one bit at a time, we have the buffer after all
                        this.BackupStorage[EEPROMWriteAddress++] = (byte)EEPROMBuffer;
                        this.BackupChanged = true;

                        this.Log($"Write byte {((byte)EEPROMBuffer).ToString("x4")} to {(EEPROMWriteAddress - 1).ToString("x4")}");
                    }
                }
                else if (EEPROMAccess == EEPROMAccessType.Read)
                {
                    // expect 0 as final bit write for Read, I don't really care about this
                    this.Log($"Set read EEPROM read address to {EEPROMReadAddress.ToString("x4")}");

                    // ready values values
                    EEPROMReadBitCounter = EEPROMReadBitCounterReadReset;  // reuse for reads
                    EEPROMWriteBitCounter = 0;
                    EEPROMBuffer = 0;
                }
                else
                {
                    this.Error($"Something went wrong, EEPROMBitCounter overflow ({EEPROMWriteBitCounter}) in invalid mode {EEPROMAccess}, resetting...");
                    EEPROMWriteBitCounter = 0;
                }
            }
            
        }
    }
}
