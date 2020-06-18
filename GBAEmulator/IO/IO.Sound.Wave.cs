using System;

using GBAEmulator.Audio.Channels;

namespace GBAEmulator.IO
{
    public class WaveCNT_L : IORegister2
    {
        private readonly WaveChannel Master;

        public WaveCNT_L(WaveChannel Master)
        {
            this.Master = Master;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x00e0), setlow, false);

            this.Master.Dimension  = (this._raw & 0x0020) > 0;
            this.Master.BankNumber = (this._raw & 0x0040) >> 6;
            this.Master.Playback   = (this._raw & 0x0080) > 0;
        }
    }

    public class WaveCNT_H : IORegister2
    {
        private readonly WaveChannel Master;

        public WaveCNT_H(WaveChannel Master)
        {
            this.Master = Master;
        }

        public override ushort Get()
        {
            return (ushort)(base.Get() & 0xe000);  // other bits are writeonly / unused
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xe0ff), setlow, sethigh);
            if (setlow)
                this.Master.LengthCounter = (256 - this._raw & 0x00ff);

            if (sethigh)
            {
                this.Master.SetVolume((this._raw & 0x6000) >> 13);
                this.Master.ForceVolume = this._raw >= 0x8000;
            }
        }
    }

    public class WaveCNT_X : IORegister2
    {
        private readonly WaveChannel Master;

        public WaveCNT_X(WaveChannel Master)
        {
            this.Master = Master;
        }

        public override ushort Get()
        {
            return (ushort)(base.Get() & 0x4000);  // other bits are writeonly / unused
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xc7ff), setlow, sethigh);

            // ARM7TDMI.Frequency / 2097152 = 8
            this.Master.Period = 8 * (2048 - (this._raw & 0x07ff));
            this.Master.LengthFlag = (this._raw & 0x4000) > 0;

            if (this._raw >= 0x8000) this.Master.Trigger();
        }
    }

    public class WAVE_RAM : IORegister2
    {
        private readonly WaveChannel Master;
        private int index;

        public WAVE_RAM(WaveChannel Master, int index)
        {
            this.Master = Master;
            this.index = index;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);

            // the bank that is not selected is written to
            if (setlow)
                this.Master.WaveRAM[1 - this.Master.BankNumber][index] = (byte)value;

            if (sethigh)
                this.Master.WaveRAM[1 - this.Master.BankNumber][index + 1] = (byte)(value >> 8);
        }
    }
}
