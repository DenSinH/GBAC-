using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        [Conditional("DEBUG")]
        private void Error(string message)
        {
            Console.Error.WriteLine("Error: " + message);
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            if (this.PC != 0x0800_0194 && this.PC != 0x0800_0196 && this.PC != 0x0800_0198)
            {
                Console.WriteLine(message);
            }
        }

        public void ShowInfo()
        {
            Console.WriteLine(string.Join(",", this.Registers.Select(x => "0x" + x.ToString("X8")).ToArray()));
        }

        public void DumpPAL()
        {
            for (int i = 0; i < 0x20; i++)
            {
                Console.Write(string.Format("{0:x4} :: ", 0x20 * i));
                for (int j = 0; j < 0xf; j++)
                {
                    Console.Write(string.Format("{0:x4} ", this.GetHalfWordAt((uint)(0x0500_0000 | (0x20 * i) | (2 * j)))));
                }
                Console.WriteLine();
            }
        }

        public void DumpVRAM(byte CharBlock, byte bpp)
        {
            uint StartAddress = (uint)(CharBlock * 0x4000);
            uint Address;
            for (int i = 0; i < 0x30; i++)  // overall y
            {
                for (int t = 0; t < 0x10; t++)
                {
                    Address = StartAddress + (uint)(8 * bpp * t) + (uint)(bpp * i);
                    for (int j = 0; j < bpp; j++)
                    {
                        for (int w = bpp - 1; w < 8; w += 4)  // double writing for 4bpp
                        {
                            switch ((this.VRAM[Address] >> 4) / 2)
                            {
                                case 0:
                                    Console.Write(" ");
                                    break;
                                case 1:
                                    Console.Write("/");
                                    break;
                                case 2:
                                    Console.Write("?");
                                    break;
                                case 3:
                                    Console.Write("#");
                                    break;
                                default:
                                    Console.Write(" ");
                                    break;
                            }
                        }
                        Address++;
                    }
                    Console.Write("|");
                }
                if ((i + 1) % 8 == 0)
                {
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
