using System;

using GBAEmulator.Audio;
using GBAEmulator.Audio.Channels;

namespace GBAEmulator.IO
{
    public class SquareCNT_L : IORegister2
    {
        private readonly SquareChannel Master;

        public SquareCNT_L(SquareChannel Master)
        {
            this.Master = Master;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x007f), setlow, false);  // top 9 bits unused

            this.Master.SweepNumber = this._raw & 0x7;
            this.Master.SweepDir = (this._raw & 0x8) > 0;
            this.Master.SweepPeriod = (this._raw >> 4) & 0x7;
            this.Master.SweepReload();
        }
    }

    public class SquareCNT_H : IORegister2
    {
        private readonly SquareChannel Master;

        public SquareCNT_H(SquareChannel Master)
        {
            this.Master = Master;
        }

        public override ushort Get()
        {
            return (ushort)(base.Get() & 0xffe0);  // bottom 5 bits write only
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);

            this.Master.LengthCounter = (64 - this._raw & 0x001f);
            this.Master.SetDuty((this._raw >> 6) & 0x3);
            this.Master.EnvelopeTime = (this._raw >> 8) & 7;
            this.Master.EnvelopeDir = (this._raw & 0x0800) > 0;
            this.Master.Volume = (this._raw & 0xf000) >> 12;
        }
    }

    public class SquareCNT_X : IORegister2
    {
        private readonly SquareChannel Master;

        public SquareCNT_X(SquareChannel Master)
        {
            this.Master = Master;
        }

        public override ushort Get()
        {
            return (ushort)(base.Get() & 0x4000); // rest unused/write only
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);

            // square wave channels tick 8 times as fast because of the pulse width setting,
            // so 128 / 8 = 16, 128 = ARM7TDMI.Frequency / 131072
            this.Master.Period = 16 * (2048 - (this._raw & 0x07ff));
            this.Master.LengthFlag = (this._raw & 0x4000) > 0;
            if (this._raw >= 0x8000) this.Master.Trigger();
        }
    }
}
