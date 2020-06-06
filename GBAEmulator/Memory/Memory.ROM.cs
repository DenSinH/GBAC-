using System;
using System.IO;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        const string SAVE_EXTENSION = ".gbac";

        private enum Backup
        {
            EEPROM, 
            SRAM,
            FLASH,
            FLASH512,
            FLASH1M
        }

        public string ROMName { get; private set; }
        private string ROMPath;
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
            // Initialize save data
            this.ROMBackupType = this.GetBackupType(FileName);
            this.InitBackup();
            
            if (File.Exists(Path.ChangeExtension(FileName, SAVE_EXTENSION)))
            {
                this.LoadBackup(Path.ChangeExtension(FileName, SAVE_EXTENSION));
            }

            // Load actual ROM
            FileStream fs = File.OpenRead(FileName);
            int current = fs.ReadByte();
            uint i = 0;

            while (current != -1)
            {
                this.GamePak[i++] = (byte)current;
                current = fs.ReadByte();
            }
            ROMSize = i;
            this.Log(string.Format("{0:x8} Bytes loaded (hex)", ROMSize));

            this.ROMPath = FileName;
            this.ROMName = Path.GetFileName(FileName);
        }
    }
}
