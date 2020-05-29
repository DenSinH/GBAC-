using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public bool pause;

        [Conditional("DEBaUG")]
        private void Error(string message)
        {
            Console.Error.WriteLine("Error: " + message);
        }

        [Conditional("DEBaUG")]
        private void Log(string message)
        {
            if (this.PC != 0x0800_0194 && this.PC != 0x0800_0196 && this.PC != 0x0800_0198)
            {
                Console.WriteLine(message);
            }
        }

        public void ShowInfo()
        {
            Console.WriteLine(string.Join(" ", this.Registers.Select(x => x.ToString("X8")).ToArray()) + " " + this.VCOUNT.CurrentScanline.ToString("x2"));
        }

        public void InterruptInfo()
        {
            Console.WriteLine($"HALTCNT: {this.HALTCNT.Halt}, CPSR-I: {this.I}");
            Console.WriteLine($"IME disable all: {this.IME.DisableAll}, IE: {this.IE.raw.ToString("x8")}, IF: {this.IF.raw.ToString("x8")}");
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

        public void DumpOAM()
        {
            for (int i = 0; i < 0x20; i++)
            {
                Console.Write(string.Format("{0:x4} :: ", 0x20 * i));
                for (int j = 0; j < 0x10; j++)
                {
                    Console.Write(string.Format("{0:x4} ", this.GetHalfWordAt((uint)(0x0700_0000 | (0x20 * i) | (2 * j)))));
                }
                Console.WriteLine();
            }
        }

        public void DumpVRAM(byte CharBlock, byte bpp)
        {
            uint StartAddress = (uint)(CharBlock * 0x4000);
            uint Address = 0;
            for (int row = 0; row < 0x10; row++)
            {
                Console.WriteLine(Address.ToString("x4"));
                for (int dy = 0; dy < 8; dy++)  // overall y
                {
                    for (int t = 0; t < 0x10; t++)
                    {
                        Address = StartAddress + (uint)(8 * bpp * 0x10 * row) + (uint)(8 * bpp * t) + (uint)(bpp * dy);  // 0x10 is row length
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
                    if ((dy + 1) % 8 == 0)
                    {
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
        }

        public void FindValueInRAM(ushort value)
        {
            Console.Write("iWRAM: ");
            for (uint i = 0; i < this.iWRAM.Length; i += 2)
            {
                if (__GetHalfWordAt__(this.iWRAM, i) == value)
                    Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.Write("eWRAM: ");
            for (uint i = 0; i < this.eWRAM.Length; i += 2)
            {
                if (__GetHalfWordAt__(this.eWRAM, i) == value)
                    Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.Write("IORAM: ");
            for (uint i = 0; i < this.IORAM.Length; i += 2)
            {
                if (this.IOGetHalfWordAt(i) == value)
                    Console.Write(i + " ");
            }
            Console.WriteLine();
        }

        public void ShowIWRAMAt(uint address)
        {
            Console.WriteLine(string.Format("iWRAM[${0:x8}] : {1:x8}", address, __GetWordAt__(this.iWRAM, address)));
        }

        public void ShowEWRAMAt(uint address)
        {
            Console.WriteLine(string.Format("eWRAM[${0:x8}] : {1:x8}", address, __GetWordAt__(this.eWRAM, address)));
        }

    }
}
