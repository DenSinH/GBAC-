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
        public SquareChannel()
        {
            this.Duty = DutyCycles[0];
            this.Frequency = 64;  // default value
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDuty(int index)
        {
            this.Duty = DutyCycles[index];
        }
        
        public override short GetSample()
        {
            if (!this.Enabled)
                return 0;

            if (this.LengthFlag && (this.LengthCounter == 0))
                return 0;

            if (this.Volume == 0)
                return 0;

            return (((this.Duty >> this.Index) & 1) == 1) ? (short)(short.MaxValue * this.Volume / 16): (short)(short.MinValue * this.Volume / 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnTick()
        {
            this.Index = (this.Index + 1) & 7;
        }
    }
}
