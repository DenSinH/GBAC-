using System;
using System.Collections.Generic;
using System.Text;

using GBAEmulator.Audio;

namespace GBAEmulator.IO
{
    public class SOUNDCNT_L : IORegister2
    {
        private readonly APU apu;

        public SOUNDCNT_L(APU apu)
        {
            this.apu = apu;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xff77), setlow, sethigh);
            if (setlow)
            {
                this.apu.MasterVolumeRight = (uint)(this._raw & 0x0007);
                this.apu.MasterVolumeLeft = (uint)((this._raw >> 4) & 0x0007);
            }
            if (sethigh)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.apu.MasterEnableRight[i] = ((this._raw >> (8 + i)) & 1) > 0;
                    this.apu.MasterEnableLeft[i] = ((this._raw >> (12 + i)) & 1) > 0;
                }
            }
        }
    }

    public class SOUNDCNT_X : IORegister2
    {
        private readonly APU apu;

        public SOUNDCNT_X(APU apu)
        {
            this.apu = apu;
        }

        public override ushort Get()
        {
            ushort SoundOn = 0;
            for (int i = 0; i < 4; i++)
            {
                if (this.apu.Channels[i].SoundOn())
                {
                    SoundOn |= (ushort)(1 << i);
                }
            }

            return (ushort)(SoundOn | base.Get());
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x00f0), setlow, sethigh);  // other bits unused / readonly
        }
    }
}
