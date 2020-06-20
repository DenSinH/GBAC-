using System;

using GBAEmulator.CPU;
using GBAEmulator.Scheduler;

namespace GBAEmulator.IO
{
    #region Timer Registers
    public class cTMCNT_L : IORegister2
    {
        private readonly ARM7TDMI cpu;
        private bool IsCountUp;  // keep track of whether we are in CountUp mode on triggers
        private ushort Counter;  // only used for CountUp timers
        private int TriggerTime; // used for non-CountUp timers
        public ushort Reload { get; private set; }

        public ushort PrescalerLimit = 1;  // initial value (see TMCNT_H.PrescalerSelection[0])

        public cTMCNT_L(ARM7TDMI cpu)
        {
            this.cpu = cpu;
        }

        public void TimerReload(bool IsCountUp)
        {
            this.IsCountUp = IsCountUp;
            this.Counter = Reload;
            this.TriggerTime = cpu.GlobalCycleCount;
        }

        public bool TickUnscaled(ushort cycles)
        {
            // don't account for the prescaler
            bool Overflow = false;
            if (this.Counter + cycles > 0xffff)  // overflow
            {
                this.Counter += this.Reload;
                Overflow = true;
            }
            this.Counter += cycles;

            return Overflow;
        }

        public override ushort Get()
        {
            if (this.IsCountUp)
                return this.Counter;
            return (ushort)(cpu.GlobalCycleCount - this.TriggerTime);
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);
            this.Reload = value;
        }
    }

    public class cTMCNT_H : IORegister2
    {
        private static ushort[] PrescalerSelection = new ushort[4] { 1, 64, 256, 1024 };

        private readonly cTMCNT_L Data;
        private readonly ARM7TDMI.cTimer Master;

        public cTMCNT_H(ARM7TDMI.cTimer Master, cTMCNT_L Data) : base()
        {
            this.Data = Data;
            this.Master = Master;
        }

        public byte Prescaler
        {
            get => (byte)(this._raw & 0x0003);
        }

        public bool CountUpTiming
        {
            get => (this._raw & 0x0004) > 0;
        }

        public bool TimerIRQEnable
        {
            get => (this._raw & 0x0040) > 0;
        }

        public bool Enabled
        {
            get => (this._raw & 0x0080) > 0;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            bool WasEnabled = this.Enabled;

            base.Set(value, setlow, sethigh);

            this.Data.PrescalerLimit = PrescalerSelection[this.Prescaler];
            if (!WasEnabled && this.Enabled)
            {
                this.Data.TimerReload(this.CountUpTiming);
                this.Master.Trigger();
            }
        }
    }
    #endregion
}
