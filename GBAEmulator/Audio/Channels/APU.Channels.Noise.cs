using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Audio.Channels
{
    public class NoiseChannel : Channel
    {
        public bool CounterStepWidth;  // false: 15 bit, true: 7 bit
        private uint ShiftRegister;

        public NoiseChannel()
        {
            this.Period = 128 * 2048;
        }

        protected override short GetSample()
        {
            return (short)(((((~this.ShiftRegister) & 1) == 1) ? short.MaxValue : short.MinValue)  *this.Volume / 16);
        }

        public override void Trigger()
        {
            base.Trigger();
            this.ShiftRegister = (uint)(this.CounterStepWidth ? 0x4000 : 0x40);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnTick()
        {
            /*
            Noise randomly switches between HIGH and LOW levels, the output levels are calculated by a shift register (X),
            at the selected frequency, as such:
              7bit:  X=X SHR 1, IF carry THEN Out=HIGH, X=X XOR 40h ELSE Out=LOW
              15bit: X=X SHR 1, IF carry THEN Out=HIGH, X=X XOR 4000h ELSE Out=LOW
            The initial value when (re-)starting the sound is X=40h (7bit) or X=4000h (15bit).
            The data stream repeats after 7Fh (7bit) or 7FFFh (15bit) steps.
             */
            uint carry = (this.ShiftRegister ^ (this.ShiftRegister >>= 1)) & 1;
            this.ShiftRegister |= carry << 15;

            if (carry == 1)
            {
                if (this.CounterStepWidth)
                {
                    this.ShiftRegister ^= 0x6000;
                }
                else
                {
                    this.ShiftRegister ^= 0x60;
                }
            }
        }
    }
}
