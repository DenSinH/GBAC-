using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using GBAEmulator.Memory.IO;

namespace GBAEmulator.CPU
{
    public struct InterruptControlInfo
    {
        public string KEYCNT, IME, HALTCNT;
        public ushort IE, IF;

        public InterruptControlInfo(string KEYCNT, string IME, ushort IE, ushort IF, string HALTCNT)
        {
            this.KEYCNT = KEYCNT;
            this.IME = IME;
            this.IE = IE;
            this.IF = IF;
            this.HALTCNT = HALTCNT;
        }
    }

    public struct TimerInfo
    {
        public string Counter, Reload, Prescaler, IRQEnabled, Enabled, CountUp;

        public TimerInfo(ARM7TDMI.cTimer timer)
        {
            this.Counter = timer.Data.Counter.ToString("x4");
            this.Reload = timer.Data.Reload.ToString("x4");
            this.Prescaler = timer.Data.PrescalerLimit.ToString("d4");
            this.IRQEnabled = timer.Control.TimerIRQEnable ? "1" : "0";
            this.Enabled = timer.Control.Enabled ? "1" : "0";
            this.CountUp = timer.Control.CountUpTiming ? "1" : "0";
        }
    }

    public struct DMAInfo
    {
        public string DAD, SAD, UnitCount, DestAddrControl, SourceAddrControl, Repeat, UnitLength, Timing, IRQ, Enabled;

        public DMAInfo(uint DAD, uint SAD, uint UnitCount, cDMACNT_H dmacnt_h)
        {
            this.DAD = DAD.ToString("x8");
            this.SAD = SAD.ToString("x8");
            this.UnitCount = UnitCount.ToString("x8");
            this.DestAddrControl = ((ushort)dmacnt_h.DestAddrControl).ToString("d2");
            this.SourceAddrControl = ((ushort)dmacnt_h.SourceAddrControl).ToString("d2");
            this.Repeat = dmacnt_h.DMARepeat ? "1" : "0";
            this.UnitLength = dmacnt_h.DMATransferType ? "32" : "16";
            this.Timing = dmacnt_h.StartTiming.ToString();
            this.IRQ = dmacnt_h.IRQOnEnd ? "1" : "0";
            this.Enabled = dmacnt_h.DMAEnabled ? "1" : "0";
        }
    }

    partial class ARM7TDMI
    {
        public bool pause;
        
        private void Error(string message)
        {
            Console.Error.WriteLine($"CPU Error: {message}");
        }
        
        [Conditional("DEBUG")]
        private void Log(string message)
        {
            Console.WriteLine("CPU: " + message);
        }

        public InterruptControlInfo GetInterruptControl()
        {
            return new InterruptControlInfo(
                this.mem.IORAM.KEYCNT.Mask.ToString("x4"), this.mem.IORAM.IME.Enabled ? "1" : "0", this.mem.IORAM.IE.raw,
                this.mem.IORAM.IF.raw, this.mem.IORAM.HALTCNT.Halt ? "1" : "0"
            );
        }

        public TimerInfo GetTimerInfo(int index)
        {
            return new TimerInfo(this.Timers[index]);
        }

        public DMAInfo GetDMAInfo(int index)
        {
            return new DMAInfo(this.mem.IORAM.DMADAD[index].Address, this.mem.IORAM.DMASAD[index].Address,
                this.mem.IORAM.DMACNT_L[index].UnitCount, this.mem.IORAM.DMACNT_H[index]);
        }

        public void ShowInfo()
        {
            Console.WriteLine(string.Join(" ", this.Registers.Select(x => x.ToString("X8")).ToArray()) + $" cpsr: {this.CPSR.ToString("X8")}");
        }

        public void InterruptInfo()
        {
            Console.WriteLine($"HALTCNT: {this.mem.IORAM.HALTCNT.Halt}, CPSR-I: {this.I}");
            Console.WriteLine($"IME enabled: {this.mem.IORAM.IME.Enabled}, IE: {this.mem.IORAM.IE.raw.ToString("x8")}, IF: {this.mem.IORAM.IF.raw.ToString("x8")}");
        }

        public void DumpPAL()
        {
            for (int i = 0; i < 0x20; i++)
            {
                Console.Write(string.Format("{0:x4} :: ", 0x20 * i));
                for (int j = 0; j < 0xf; j++)
                {
                    Console.Write(string.Format("{0:x4} ", this.mem.GetHalfWordAt((uint)(0x0500_0000 | (0x20 * i) | (2 * j)))));
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
                    Console.Write(string.Format("{0:x4} ", this.mem.GetHalfWordAt((uint)(0x0700_0000 | (0x20 * i) | (2 * j)))));
                }
                Console.WriteLine();
            }
        }

        //public void DumpVRAM(byte CharBlock, byte bpp)
        //{
        //    uint StartAddress = (uint)(CharBlock * 0x4000);
        //    uint Address = 0;
        //    for (int row = 0; row < 0x10; row++)
        //    {
        //        Console.WriteLine(Address.ToString("x4"));
        //        for (int dy = 0; dy < 8; dy++)  // overall y
        //        {
        //            for (int t = 0; t < 0x10; t++)
        //            {
        //                Address = StartAddress + (uint)(8 * bpp * 0x10 * row) + (uint)(8 * bpp * t) + (uint)(bpp * dy);  // 0x10 is row length
        //                for (int j = 0; j < bpp; j++)
        //                {
        //                    for (int w = bpp - 1; w < 8; w += 4)  // double writing for 4bpp
        //                    {
        //                        switch ((this.mem.VRAM[Address] >> 4) / 2)
        //                        {
        //                            case 0:
        //                                Console.Write(" ");
        //                                break;
        //                            case 1:
        //                                Console.Write("/");
        //                                break;
        //                            case 2:
        //                                Console.Write("?");
        //                                break;
        //                            case 3:
        //                                Console.Write("#");
        //                                break;
        //                            default:
        //                                Console.Write(" ");
        //                                break;
        //                        }
        //                    }
        //                    Address++;
        //                }
        //                Console.Write("|");
        //            }
        //            if ((dy + 1) % 8 == 0)
        //            {
        //                Console.WriteLine();
        //            }
        //            Console.WriteLine();
        //        }
        //    }
        //}
    }
}
