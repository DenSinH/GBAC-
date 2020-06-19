using System;
using System.Collections.Generic;
using System.Text;

using GBAEmulator.CPU;
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

    public class SOUNDCNT_H : IORegister2
    {
        private readonly APU apu;
        private readonly ARM7TDMI.cTimer[] Timers = new ARM7TDMI.cTimer[2];

        public SOUNDCNT_H(APU apu, ARM7TDMI.cTimer Timer0, ARM7TDMI.cTimer Timer1)
        {
            this.apu = apu;
            this.Timers[0] = Timer0;
            this.Timers[1] = Timer1;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xff0f), setlow, sethigh);

            apu.Sound1_4Volume = this._raw & 0x0003;
            int timer;
            for (int i = 0; i < 2; i++)
            {
                apu.DMASoundVolume[i] = ((this._raw >> i) & 1) > 0;
                apu.DMAEnableRight[i] = ((this._raw >> (8 + 4 * i)) & 1) > 0;
                apu.DMAEnableLeft[i] = ((this._raw >> (9 + 4 * i)) & 1) > 0;

                timer = (this._raw >> (10 + 4 * i)) & 1;
                this.Timers[timer].FIFO[i] = apu.FIFO[i];
                this.Timers[1 - timer].FIFO[i] = null;

                if (((this._raw >> (11 + 4 * i)) & 1) > 0) apu.FIFO[i].Reset();
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
