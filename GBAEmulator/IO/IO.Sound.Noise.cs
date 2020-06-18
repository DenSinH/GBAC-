using System;

using GBAEmulator.Audio.Channels;

namespace GBAEmulator.IO
{
    public class NoiseCNT_L : IORegister2
    {
        private readonly NoiseChannel Master;

        public NoiseCNT_L(NoiseChannel Master)
        {
            this.Master = Master;
        }

        public override ushort Get()
        {
            return (ushort)(base.Get() & 0xff00);  // bottom 8 bits write only or unused
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xff1f), setlow, sethigh);

            this.Master.LengthCounter = (64 - this._raw & 0x001f);
            this.Master.EnvelopeTime = (this._raw >> 8) & 7;
            this.Master.EnvelopeDir = (this._raw & 0x0800) > 0;
            this.Master.Volume = (this._raw & 0xf000) >> 12;
        }
    }
    public class NoiseCNT_H : IORegister2
    {
        private readonly NoiseChannel Master;

        public NoiseCNT_H(NoiseChannel Master)
        {
            this.Master = Master;
        }

        public override ushort Get()
        {
            return (ushort)(base.Get() & 0xff00);  // bottom 8 bits write only or unused
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xc0ff), setlow, sethigh);

            int r = this._raw & 0x0007;
            int s = (this._raw & 0x00f0) >> 4;
            // ARM7TDMI.Frequency / 524288 = 32
            if (r == 0)
            {
                // interpret as 0.5 instead
                this.Master.Period = 32 * 2 * (2 << s);
            }
            else
            {
                this.Master.Period = 32 * (r * (2 << s));
            }

            this.Master.CounterStepWidth = (this._raw & 0x0008) > 0;
            this.Master.LengthFlag = (this._raw & 0x4000) > 0;
            if (this._raw >= 0x8000) this.Master.Trigger();
        }
    }
}
