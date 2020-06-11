using System;
using System.IO;
using System.Linq;

namespace GBAEmulator.Memory.Backup
{
    // ========================================================================================================
    //                                                 Flash
    //      Thanks Dillon for the clear explanation: https://dillonbeliveau.com/2020/06/05/GBA-FLASH.html
    // ========================================================================================================

    class BackupFLASH : IBackup
    {
        byte[][] Banks = new byte[2][] { new byte[0x8000], new byte[0x8000] };
        const byte SanyoManufacturerID = 0x62;
        const byte SanyoDeviceID = 0x13;

        private int State;
        private bool ChipIDMode;
        private bool ExpectErase;
        private bool ExpectSingleByte;
        private bool ExpectBankSwitch;
        private int ActiveBank = 0;

        public void Init()
        {
            for (int i = 0; i < 0x8000; i++)
            {
                Banks[0][i] = 0xff;
                Banks[1][i] = 0xff;
            }

            State = 0;
            ChipIDMode = false;
            ExpectErase = false;
            ExpectSingleByte = false;
            ActiveBank = 0;
        }
        public void Dump(string FileName)
        {
            try
            {
                // todo: do this better
                File.WriteAllBytes(FileName, this.Banks[0].Concat<byte>(this.Banks[1]).ToArray());
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
                byte[] FlashDump = File.ReadAllBytes(FileName);
                for (int b = 0; b < 2; b++)
                {
                    for (int i = 0; i < 0x8000; i++)
                    {
                        this.Banks[b][i] = FlashDump[0x8000 * b + i];
                    }
                }
            }
            catch (Exception e)
            {
                // something went wrong
                Console.Error.WriteLine("Something went wrong while dumping the save data... " + e.Message);
            }
        }

        private void Erase(byte[] Storage, uint StartAddress, uint EndAddress)
        {
            // Backup storage is cleared with 0xff instead of 0x00
            for (uint i = StartAddress; i < EndAddress; i++)
            {
                Storage[i] = 0xff;
            }
        }

        public byte Read(uint address)
        {
            if (!ChipIDMode || (address > 1))
            {
                return this.Banks[ActiveBank][address];
            }
            else if (address == 0)  // (and we are in FlashChipIDMode)
            {
                return SanyoManufacturerID;
            }
            else       // (address == 1 and we are in FlashChipIDMode)
            {
                return SanyoDeviceID;
            }
        }

        public bool Write(uint address, byte value)
        {
            if (ExpectSingleByte)
            {
                ExpectSingleByte = false;
                this.Banks[ActiveBank][address] = value;

                // this.Log($"Flash single byte write {value.ToString("x2")} to {address.ToString("x4")}");
                return true;
            }
            else if (ExpectBankSwitch)  // only enabled if bank switching is actually possible
            {
                ExpectBankSwitch = false;
                if (address == 0)
                {
                    ActiveBank = value;
                    // this.Log("Bank switched to " + value);
                }
                else
                {
                    Console.Error.WriteLine($"Expected Bank switch write 1/0 to 0x0e00_0000, got {value.ToString("x2")} to {address.ToString("x4")}");
                }
                return false;
            }

            switch (State)
            {
                case 0:  // expect 0xAA to 0x5555 to signify command start
                    if (address == 0x5555 && value == 0xaa) State++;
                    else Console.Error.WriteLine($"Flash: Expected 0xAA to 0x5555, got {value.ToString("x2")} to {address.ToString("x4")}");
                    return false;
                case 1:  // expect 0x55 to 0x2aaa to signify command start
                    if (address == 0x2aaa && value == 0x55) State++;
                    else Console.Error.WriteLine($"Flash: Expected 0x55 to 0x2aaa, got {value.ToString("x2")} to {address.ToString("x4")}");
                    return false;
                case 2:  // command
                    State = 0;  // reset flash state
                    switch (value)
                    {
                        case 0x90:  // Enter "Chip identification mode"
                            ChipIDMode = true;
                            return false;
                        case 0xf0:  // Exit "Chip identification mode"
                            ChipIDMode = false;
                            return false;
                        case 0x80:  // Prepare to receive erase command
                            ExpectErase = true;
                            return false;
                        case 0x10:  // Erase entire chip
                            if (ExpectErase)
                            {
                                this.Erase(this.Banks[ActiveBank], 0, 0x10000);
                                ExpectErase = false;
                                return true;
                            }
                            else
                            {
                                Console.Error.WriteLine("Flash erase command not preceded by prepare erase, command ignored");
                                return false;
                            }
                        case 0x30:  // Erase 4kB sector
                            if (ExpectErase)
                            {
                                // this.Log($"Erase 4kB flash at {address.ToString("x4")}");
                                this.Erase(this.Banks[ActiveBank], address, address + 0x1000);
                                ExpectErase = false;
                                return true;
                            }
                            else
                            {
                                Console.Error.WriteLine("Flash erase command not preceded by prepare erase, command ignored");
                                return false;
                            }
                        case 0xa0:  // Prepare to write single data byte
                            ExpectSingleByte = true;
                            return false;
                        case 0xb0:  // Bank switch
                            // only works for 128 kb flash devices
                            ExpectBankSwitch = true;
                            return false;
                        default:
                            Console.Error.WriteLine($"Invalid Flash command: {value.ToString("x2")}");
                            return false;
                    }
                default:
                    Console.Error.WriteLine("Flash state overflow, resetting FlashState to 0...");
                    State = 0;
                    return false;
            }
        }
    }
}
