using System;
using System.IO;
using System.Linq;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private enum Backup
        {
            EEPROM,
            SRAM,
            FLASH,
            FLASH512,
            FLASH1M
        }

        byte[] BackupStorage = new byte[0x10000];
        private void Erase(byte[] Storage, uint StartAddress, uint EndAddress)
        {
            // Backup storage is cleared with 0xff instead of 0x00
            for (uint i = StartAddress; i < EndAddress; i++)
            {
                Storage[i] = 0xff;
            }
        }

        public bool BackupChanged { get; set; } = false;

        public bool DumpBackup()
        {
            string FileName = Path.ChangeExtension(this.ROMPath, SAVE_EXTENSION);
            try
            {
                switch (this.ROMBackupType)
                {
                    case Backup.SRAM:
                        File.WriteAllBytes(FileName, this.BackupStorage);
                        break;
                    case Backup.FLASH:
                    case Backup.FLASH512:
                    case Backup.FLASH1M:
                        // todo: do this better
                        File.WriteAllBytes(FileName, this.FlashBanks[0].Concat<byte>(this.FlashBanks[1]).ToArray());
                        break;
                    case Backup.EEPROM:
                        File.WriteAllBytes(FileName, this.BackupStorage);
                        break;
                    default:
                        throw new Exception($"Invalid rom backup type: {this.ROMBackupType}");
                }
                return true;
            }
            catch (Exception e)
            {
                // something went wrong
                this.Error("Something went wrong while dumping the save data... " + e.Message);
                return false;
            }
        }

        public bool LoadBackup(string FileName)
        {
            try
            {
                switch (this.ROMBackupType)
                {
                    case Backup.SRAM:
                        this.BackupStorage = File.ReadAllBytes(FileName);
                        break;
                    case Backup.FLASH:
                    case Backup.FLASH512:
                    case Backup.FLASH1M:
                        // todo: do this better
                        byte[] FlashDump = File.ReadAllBytes(FileName);
                        for (int b = 0; b < 2; b++)
                        {
                            for (int i = 0; i < 0x10000; i++)
                            {
                                this.FlashBanks[b][i] = FlashDump[0x10000 * b + i];
                            }
                        }
                        break;
                    case Backup.EEPROM:
                        this.BackupStorage = File.ReadAllBytes(FileName);
                        break;
                    default:
                        throw new Exception($"Invalid rom backup type: {this.ROMBackupType}");
                }
                return true;
            }
            catch
            {
                // something went wrong
                return false;
            }
        }

        private void InitBackup()
        {
            switch (this.ROMBackupType)
            {
                case Backup.SRAM:
                    break;
                case Backup.FLASH:
                case Backup.FLASH512:
                case Backup.FLASH1M:
                    this.InitFlash();
                    break;
                case Backup.EEPROM:
                    this.InitEEPROM();
                    break;
                default:
                    throw new Exception($"Invalid rom backup type: {this.ROMBackupType}");
            }
            this.Erase(this.BackupStorage, 0, 0x10000);
        }

        private void BackupWrite(uint address, byte value)
        {
            // 0 <= address < 0x10000
            switch (this.ROMBackupType)
            {
                case Backup.SRAM:
                    this.BackupStorage[address] = value;
                    this.BackupChanged = true;
                    return;
                case Backup.FLASH:
                case Backup.FLASH512:
                case Backup.FLASH1M:
                    this.FlashWrite(address, value);
                    return;
                case Backup.EEPROM:
                    this.EEPROMWrite(value);
                    this.BackupChanged = true;
                    return;
                default:
                    throw new Exception($"Invalid rom backup type: {this.ROMBackupType}");
            }
        }

        private byte BackupRead(uint address)
        {
            // 0 <= address < 0x10000
            switch (this.ROMBackupType)
            {
                case Backup.SRAM:
                    return this.BackupStorage[address];
                case Backup.FLASH:
                case Backup.FLASH512:
                case Backup.FLASH1M:
                    this.Log($"Flash read {address.ToString("x4")}, IDMode: {FlashChipIDMode}");
                    if (!FlashChipIDMode || (address > 1))
                    {
                        return this.FlashBanks[FlashActiveBank][address];
                    }
                    else if (address == 0)  // (and we are in FlashChipIDMode)
                    {
                        return SanyoManufacturerID;
                    }
                    else       // (address == 1 and we are in FlashChipIDMode)
                    {
                        return SanyoDeviceID;
                    }
                case Backup.EEPROM:
                    return this.EEPROMRead();
                default:
                    throw new Exception($"Invalid rom backup type: {this.ROMBackupType}");
            }
        }
    }
}
