using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        #region Timer Registers
        public class cTMCNT_L : IORegister2
        {
            public ushort Counter { get; private set; }
            public ushort Reload { get; private set; }

            private ushort PrescalerCounter;
            public ushort PrescalerLimit = cTMCNT_H.PrescalerSelection[0];

            public void TimerReload()
            {
                this.Counter = Reload;
            }

            public bool TickDirect(ushort cycles)
            {
                // don't account for the prescaler
                bool Overflow = false;
                if (this.Counter + cycles > 0xffff)  // overflow
                {
                    this.Counter += cycles;
                    this.Counter += this.Reload;
                    Overflow = true;
                }
                this.Counter += cycles;

                return Overflow;
            }

            public bool Tick(ushort cycles)
            {
                bool Overflow = false;

                this.PrescalerCounter += cycles;

                if (this.PrescalerCounter > this.PrescalerLimit)
                {
                    // assume cycles < 64 (pretty valid assumption)
                    Overflow |= this.TickDirect((ushort)(this.PrescalerLimit == 1 ? this.PrescalerCounter : 1));

                    this.PrescalerCounter &= (ushort)(this.PrescalerLimit - 1);  // power of 2
                }

                return Overflow;
            }

            public override ushort Get()
            {
                return this.Counter;
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                base.Set(value, setlow, sethigh);
                this.Reload = value;
            }
        }

        public class cTMCNT_H : IORegister2
        {
            public static ushort[] PrescalerSelection = new ushort[4] { 1, 64, 256, 1024 };

            cTMCNT_L Data;

            public cTMCNT_H(cTMCNT_L Data) : base()
            {
                this.Data = Data;
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
                if (!WasEnabled && this.Enabled) this.Data.TimerReload();
            }
        }
        #endregion
    }
}
