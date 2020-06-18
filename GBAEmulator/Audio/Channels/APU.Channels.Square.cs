using System.Runtime.CompilerServices;

namespace GBAEmulator.Audio.Channels
{
    public class SquareChannel : Channel
    {
        private static byte[] DutyCycles = new byte[4]
        {
            0x80,  // 12.5 %
            0xc0,  // 25   %
            0xf0,  // 50   %
            0xfc,  // 75   %
        };

        private int Index = 0;
        private byte Duty;

        public int SweepNumber;
        public bool SweepDir;
        public int SweepTime;
        public SquareChannel()
        {
            this.Duty = DutyCycles[0];
            this.Period = 128 * 2048;  // default value
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDuty(int index)
        {
            this.Duty = DutyCycles[index];
        }

        public void DoSweep()
        {
            if (SweepTime > 0)
            {
                SweepTime--;
                int dPeriod = this.Period / (1 + (1 << SweepNumber));
                if (!SweepDir) dPeriod *= -1;

                this.Period += dPeriod;
            }
        }
        
        protected override short GetSample()
        {
            return (short)(((((this.Duty >> this.Index) & 1) == 1) ? short.MaxValue: short.MinValue) * this.Volume / 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnTick()
        {
            this.Index = (this.Index + 1) & 7;
        }
    }
}
