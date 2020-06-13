using System;
using System.IO;
using System.Linq;

namespace GBAEmulator.Memory.Backup
{
    class BackupSRAM : IBackup
    {
        byte[] Storage = new byte[0x8000];

        public void Init()
        {
            for (int i = 0; i < 0x8000; i++)
            {
                Storage[i] = 0xff;
            }
        }
        public void Dump(string FileName)
        {
            try
            {
                File.WriteAllBytes(FileName, this.Storage);
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
                this.Storage = File.ReadAllBytes(FileName);
            }
            catch (Exception e)
            {
                // something went wrong
                Console.Error.WriteLine("Something went wrong while dumping the save data... " + e.Message);
            }
        }

        public byte Read(uint address)
        {
            return this.Storage[address];
        }

        public bool Write(uint address, byte value)
        {
            this.Storage[address] = value;
            return true;
        }
    }
}
