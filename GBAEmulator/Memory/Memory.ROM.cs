using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        const string SAVE_EXTENSION = ".gbac";

        public string ROMName { get; private set; }
        private string ROMPath;
        public BackupType ROMBackupType { get; private set; }
        public uint ROMSize { get; private set; }

        private BackupType GetBackupType(string FileName)
        {
            // search for the type of backup used in ROM file
            string[] RomContent = File.ReadAllLines(FileName);
            foreach (string line in RomContent)
            {
                foreach (BackupType BackupType in Enum.GetValues(typeof(BackupType)))
                {
                    if (Regex.Match(line, $"{BackupType}_V\\d\\d\\d").Success)
                    {
                        return BackupType;
                    }
                }
            }
            this.Error($"Could not find ROM backup type for ROM {FileName}");
            return BackupType.SRAM;
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

            this.GamePakSection_L.Load(this.GamePak, 0);
            this.GamePakSection_H.Load(this.GamePak, 0x0100_0000);
        }
    }
}
