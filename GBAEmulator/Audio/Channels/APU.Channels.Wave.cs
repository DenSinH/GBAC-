using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Audio.Channels
{
    public class WaveChannel : Channel
    {
        public byte[][] WaveRAM = new byte[2][] { new byte[16], new byte[16] };
        //  Data is played back ordered as follows: MSBs of 1st byte, followed by LSBs of 1st byte, followed by MSBs of 2nd byte, and so on 
        private int PositionCounter = 0;
        private bool HighNibble = true;

        public bool Dimension;  // false: one bank/32 digits, true: 2 banks/64 digits
        public int  BankNumber;
        public bool Playback;
        public bool ForceVolume;

        public WaveChannel()
        {
            this.Period = 8 * 2048;
        }

        public void SetVolume(int value)
        {
            switch (value)
            {
                case 0: this.Volume = 0; break;   // Mute
                case 1: this.Volume = 16; break;  // 100%
                case 2: this.Volume = 8; break;   //  50%
                case 3: this.Volume = 4; break;   //  25%
            }
        }

        public override bool SoundOn()
        {
            if (!this.Enabled)
                return false;

            if (this.LengthFlag && (this.LengthCounter == 0))
                return false;

            if (!this.ForceVolume && this.Volume == 0)
                return false;

            if (!this.Playback)
                return false;

            return true;
        }

        protected override short GetSample()
        {
            int TrueVolume = this.ForceVolume ? 12 : this.Volume;

            byte Sample = this.WaveRAM[this.BankNumber][this.PositionCounter];
            if (this.HighNibble) Sample >>= 4;

            Sample &= 0xf;

            return (short)(short.MaxValue * TrueVolume * Sample / 256);  // 16 * 16, 16 for volume, 16 for sample
        }

        public override void Trigger()
        {
            base.Trigger();
            if (this.LengthCounter == 0) this.LengthCounter = 256;
            this.PositionCounter = 0;
        }

        protected override void OnTick()
        {
            this.HighNibble ^= true;
            if (this.HighNibble)
            {
                this.PositionCounter = (this.PositionCounter + 1) & 0x0f;  // mod 16
                if (this.Dimension && this.PositionCounter == 0)
                {
                    this.BankNumber = 1 - this.BankNumber;
                }
            }
        }
    }
}
