using System;
using System.IO;
using GBAEmulator.CPU;
using GBAEmulator.Memory.Backup;

namespace GBAEmulator.Memory.Sections
{
    public enum BackupType
    {
        EEPROM,
        SRAM,
        FLASH,
        FLASH512,
        FLASH1M
    }

    public class BackupSection : IMemorySection
    {
        public const string SaveExtension = ".gbac";
        public string ROMPath;

        const uint AddressMask = 0xffff;

        private IBackup Backup;
        public BackupType ROMBackupType;
        private ARM7TDMI.DMAChannel DMA3;

        public BackupSection(ARM7TDMI.DMAChannel DMA3)
        {
            this.DMA3 = DMA3;
            this.ROMBackupType = BackupType.SRAM;
        }

        public void Init()
        {
            switch (this.ROMBackupType)
            {
                case BackupType.SRAM:
                    this.Backup = new BackupSRAM();
                    break;
                case BackupType.FLASH:
                case BackupType.FLASH512:
                case BackupType.FLASH1M:
                    this.Backup = new BackupFLASH();
                    break;
                case BackupType.EEPROM:
                    this.Backup = new BackupEEPROM(this.DMA3.DMACNT_H, this.DMA3.DMACNT_L);
                    break;
                default:
                    throw new Exception($"Invalid rom backup type: {this.ROMBackupType}");
            }

            this.Backup.Init();
        }

        public bool BackupChanged { get; set; } = false;

        public void DumpBackup()
        {
            string FileName = Path.ChangeExtension(ROMPath, SaveExtension);
            this.Backup.Dump(FileName);
        }

        public void LoadBackup(string FileName)
        {
            this.Backup.Load(FileName);
        }

        public byte? GetByteAt(uint address)
        {
            return this.Backup.Read(address & AddressMask);
        }

        public ushort? GetHalfWordAt(uint address)
        {
            return (ushort)(0x0101 * this.Backup.Read(address & AddressMask));
        }

        public uint? GetWordAt(uint address)
        {
            return (uint)0x0101_0101 * this.Backup.Read(address & AddressMask);
        }

        public void SetByteAt(uint address, byte value)
        {
            // detect changes in actual backup storage
            this.BackupChanged |= this.Backup.Write(address & AddressMask, value);
        }

        public void SetHalfWordAt(uint address, ushort value)
        {
            this.BackupChanged |= this.Backup.Write(address & AddressMask, (byte)value);
        }

        public void SetWordAt(uint address, uint value)
        {
            this.BackupChanged |= this.Backup.Write(address & AddressMask, (byte)value);
        }
    }

}
