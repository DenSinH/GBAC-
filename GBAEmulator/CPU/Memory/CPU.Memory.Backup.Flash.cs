using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        // ========================================================================================================
        //                                                 Flash
        //      Thanks Dillon for the clear explanation: https://dillonbeliveau.com/2020/06/05/GBA-FLASH.html
        // ========================================================================================================

        const byte SanyoManufacturerID = 0x62;
        const byte SanyoDeviceID = 0x13;

        private int FlashState;
        private bool FlashChipIDMode;
        private bool FlashExpectErase;
        private bool FlashExpectSingleByte;
        private bool FlashExpectBankSwitch;
        private int FlashActiveBank = 0;
        private byte[][] FlashBanks;

        private void InitFlash()
        {
            FlashState = 0;
            FlashChipIDMode = false;
            FlashExpectErase = false;
            FlashExpectSingleByte = false;
            FlashBanks = new byte[2][] { BackupStorage, new byte[0x10000] };
            FlashActiveBank = 0;
            this.Erase(this.FlashBanks[1], 0, 0x10000);
        }

        private void FlashWrite(uint address, byte value)
        {
            if (FlashExpectSingleByte)
            {
                FlashExpectSingleByte = false;
                this.FlashBanks[FlashActiveBank][address] = value;
                this.BackupChanged = true;

                this.Log($"Flash single byte write {value.ToString("x2")} to {address.ToString("x4")}");
                return;
            }
            else if (FlashExpectBankSwitch)  // only enabled if bank switching is actually possible
            {
                FlashExpectBankSwitch = false;
                if (address == 0)
                {
                    FlashActiveBank = value;
                    this.Log("Bank switched to " + value);
                }
                else
                {
                    this.Error($"Expected Bank switch write 1/0 to 0x0e00_0000, got {value.ToString("x2")} to {address.ToString("x4")}");
                }
                return;
            }

            switch (FlashState)
            {
                case 0:  // expect 0xAA to 0x5555 to signify command start
                    if (address == 0x5555 && value == 0xaa) FlashState++;
                    else this.Error($"Flash: Expected 0xAA to 0x5555, got {value.ToString("x2")} to {address.ToString("x4")}");
                    return;
                case 1:  // expect 0x55 to 0x2aaa to signify command start
                    if (address == 0x2aaa && value == 0x55) FlashState++;
                    else this.Error($"Flash: Expected 0x55 to 0x2aaa, got {value.ToString("x2")} to {address.ToString("x4")}");
                    return;
                case 2:  // command
                    // Console.ReadKey();
                    FlashState = 0;  // reset flash state
                    switch (value)
                    {
                        case 0x90:  // Enter "Chip identification mode"
                            FlashChipIDMode = true;
                            return;
                        case 0xf0:  // Exit "Chip identification mode"
                            FlashChipIDMode = false;
                            return;
                        case 0x80:  // Prepare to receive erase command
                            FlashExpectErase = true;
                            return;
                        case 0x10:  // Erase entire chip
                            if (FlashExpectErase)
                            {
                                this.Erase(this.FlashBanks[FlashActiveBank], 0, 0x10000);
                                this.BackupChanged = true;
                                FlashExpectErase = false;
                            }
                            else
                            {
                                this.Error("Flash erase command not preceded by prepare erase, command ignored");
                            }
                            return;
                        case 0x30:  // Erase 4kB sector
                            if (FlashExpectErase)
                            {
                                this.Log($"Erase 4kB flash at {address.ToString("x4")}");
                                this.Erase(this.FlashBanks[FlashActiveBank], address, address + 0x1000);
                                this.BackupChanged = true;
                                FlashExpectErase = false;
                            }
                            else
                            {
                                this.Error("Flash erase command not preceded by prepare erase, command ignored");
                            }
                            return;
                        case 0xa0:  // Prepare to write single data byte
                            FlashExpectSingleByte = true;
                            return;
                        case 0xb0:  // Bank switch
                            // only works for 128 kb flash devices
                            if (this.ROMBackupType == Backup.FLASH1M) FlashExpectBankSwitch = true;
                            return;
                        default:
                            this.Error($"Invalid Flash command: {value.ToString("x2")}");
                            return;
                    }
                default:
                    this.Error("Flash state overflow, resetting FlashState to 0...");
                    FlashState = 0;
                    return;
            }
        }
    }
}
