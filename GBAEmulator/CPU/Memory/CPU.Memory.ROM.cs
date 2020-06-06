using System;
using System.IO;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private enum Backup
        {
            EEPROM, 
            SRAM,
            FLASH,
            FLASH512,
            FLASH1M
        }

        public string ROMName { get; private set; }
        private Backup ROMBackupType;
        private uint ROMSize;

        private Backup GetBackupType(string FileName)
        {
            string[] RomContent = File.ReadAllLines(FileName);
            foreach (string line in RomContent)
            {
                foreach (Backup BackupType in Enum.GetValues(typeof(Backup)))
                {
                    if (line.Contains($"{BackupType}_"))
                    {
                        return BackupType;
                    }
                }
            }
            this.Error($"Could not find ROM backup type for ROM {FileName}");
            return Backup.SRAM;
        }

        public void LoadRom(string FileName)
        {
            this.ROMBackupType = this.GetBackupType(FileName);
            this.InitBackup();

            FileStream fs = File.OpenRead(FileName);
            int current = fs.ReadByte();
            uint i = 0;

            while (current != -1)
            {
                this.GamePak[i++] = (byte)current;
                current = fs.ReadByte();
            }
            ROMSize = i;
            this.Log(string.Format("{0:x8} Bytes loaded (hex)", i));

            while (i < 0x0200_0000)  // unused bits in ROM
            {
                this.GamePak[i] = (byte)(i++ >> 1);
            }
            this.ROMName = Path.GetFileName(FileName);
        }
    }
}
