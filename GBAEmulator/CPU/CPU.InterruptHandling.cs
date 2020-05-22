using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        [Flags]
        public enum Interrupt : ushort
        {
            LCDVBlank = 0x0001,
            LCDHBlank = 0x0002,
            LCDVCountMatch = 0x0004,
            Timer0Overflow = 0x0008,
            Timer1Overflow = 0x0010,
            Timer2Overflow = 0x0020,
            Timer3Overflow = 0x0040,
            SerialCommunication = 0x0080,
            DMA0 = 0x0100,
            DMA1 = 0x0200,
            DMA2 = 0x0400,
            DMA3 = 0x0800,
            Keypad = 0x1000,
            GamePak = 0x2000
        }

        private bool HandleIRQs()
        {
            if (!this.IME.DisableAll && this.I == 0)
            {
                if ((this.IF.raw & this.IE.raw) != 0)
                {
                    this.DoIRQ();
                    return true;
                }
            }
            return false;
        }

    }
}
