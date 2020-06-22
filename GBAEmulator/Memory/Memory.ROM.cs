using System;
using System.IO;
using System.Text.RegularExpressions;

using GBAEmulator.Memory.Sections;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private struct ROMData
        {
            public BackupType backup;
            public GPIO.GPIO.Chip chip;
        }

        public string ROMName { get; private set; }

        private ROMData GetROMData(string FileName)
        {
            ROMData data = new ROMData()
            {
                backup = BackupType.SRAM,
                chip = GPIO.GPIO.Chip.Empty
            };

            // search for the type of backup used in ROM file
            using (StreamReader file = new StreamReader(FileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (data.backup == BackupType.SRAM)
                    {
                        foreach (BackupType BackupType in Enum.GetValues(typeof(BackupType)))
                        {
                            if (Regex.Match(line, $"{BackupType}_V\\d\\d\\d").Success)
                            {
                                data.backup = BackupType;
                            }
                        }
                    }

                    if (data.chip == GPIO.GPIO.Chip.Empty)
                    {
                        foreach (GPIO.GPIO.Chip chip in Enum.GetValues(typeof(GPIO.GPIO.Chip)))
                        {
                            if (Regex.Match(line, $"{chip}_V\\d\\d\\d").Success)
                            {
                                data.chip = chip;
                            }
                        }
                    }
                }
                file.Close();
            }
            Console.WriteLine($"Read data for ROM {FileName}:");
            Console.WriteLine($"    Using BackupType {data.backup} for ROM");
            Console.WriteLine($"    Using GPIO chip {data.chip} for ROM");

            return data;
        }

        public void LoadRom(string FileName)
        {
            // Initialize save data
            ROMData data = this.GetROMData(FileName);
            this.Backup.ROMBackupType = data.backup;
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
            this.GamePak_L.gpio = new GPIO.GPIO(data.chip);

            this.GamePak_H.ROMSize = (uint)GamePak.Length;
            this.GamePak_H.Load(GamePak, 0x0100_0000);
            // GPIO is in the lower region of the GamePak, we don't need to set it for GamePak_H
        }
    }
}
