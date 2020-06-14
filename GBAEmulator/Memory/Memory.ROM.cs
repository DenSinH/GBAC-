using System;
using System.IO;
using System.Text.RegularExpressions;

using GBAEmulator.Memory.Sections;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        public string ROMName { get; private set; }

        private BackupType GetBackupType(string FileName)
        {
            // search for the type of backup used in ROM file
            using (StreamReader file = new StreamReader(FileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    foreach (BackupType BackupType in Enum.GetValues(typeof(BackupType)))
                    {
                        if (Regex.Match(line, $"{BackupType}_V\\d\\d\\d").Success)
                        {
                            return BackupType;
                        }
                    }
                }
                file.Close();
            }
            this.Error($"Could not find ROM backup type for ROM {FileName}");
            return BackupType.SRAM;
        }

        public void LoadRom(string FileName)
        {
            // Initialize save data
            this.Backup.ROMBackupType = this.GetBackupType(FileName);
            this.Backup.Init();
            
            if (File.Exists(Path.ChangeExtension(FileName, BackupSection.SaveExtension)))
            {
                this.Backup.LoadBackup(Path.ChangeExtension(FileName, BackupSection.SaveExtension));
            }

            // Load actual ROM
            byte[] GamePak = File.ReadAllBytes(FileName);
            this.Log(string.Format("{0:x8} Bytes loaded (hex)", GamePak.Length));

            this.Backup.ROMPath = FileName;
            this.ROMName = Path.GetFileName(FileName);

            this.GamePak_L.ROMSize = (uint)GamePak.Length;
            this.GamePak_L.Load(GamePak, 0);
            this.GamePak_H.ROMSize = (uint)GamePak.Length;
            this.GamePak_H.Load(GamePak, 0x0100_0000);
        }
    }
}
