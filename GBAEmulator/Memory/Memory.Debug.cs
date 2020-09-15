using System;

using System.Diagnostics;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private void Error(string message)
        {
            Console.Error.WriteLine($"Memory Error: {message}");
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void DumpEWRAMStart()
        {
            // used for testing my FuzzARM ROMS
            for (int i = 0; i < 0xc000; i++)
            {
                Console.WriteLine($"{i} : {this.eWRAM[i]:x2} : {(char)this.eWRAM[i]}");
            }
        }
        public void DumpIWRAMStart()
        {
            // used for testing my BIOS collab
            for (int i = 0x800; i < 0x1000; i++)
            {
                Console.Write($"{this.iWRAM[i]:x2}");
            }
        }
    }
}
