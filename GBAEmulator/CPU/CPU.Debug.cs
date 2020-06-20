using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using GBAEmulator.IO;

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
            this.Counter = timer.Data.Get().ToString("x4");
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
            this.Enabled = dmacnt_h.Enabled ? "1" : "0";
        }
    }

    partial class ARM7TDMI
    {
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
                this.IO.KEYCNT.Mask.ToString("x4"), this.IO.IME.Enabled ? "1" : "0", this.IO.IE.raw,
                this.IO.IF.raw, this.IO.HALTCNT.Halt ? "1" : "0"
            );
        }

        public TimerInfo GetTimerInfo(int index)
        {
            return new TimerInfo(this.Timers[index]);
        }

        public DMAInfo GetDMAInfo(int index)
        {
            return new DMAInfo(this.IO.DMADAD[index].Address, this.IO.DMASAD[index].Address,
                this.IO.DMACNT_L[index].UnitCount, this.IO.DMACNT_H[index]);
        }

        public void ShowInfo()
        {
            Console.WriteLine(string.Join(" ", this.Registers.Select(x => x.ToString("X8")).ToArray()) + $" cpsr: {this.CPSR.ToString("X8")}");
        }

        public void InterruptInfo()
        {
            Console.WriteLine($"HALTCNT: {this.IO.HALTCNT.Halt}, CPSR-I: {this.I}");
            Console.WriteLine($"IME enabled: {this.IO.IME.Enabled}, IE: {this.IO.IE.raw.ToString("x8")}, IF: {this.IO.IF.raw.ToString("x8")}");
        }
    }
}
