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
            TimerOverflow = 0x0008,
            // obtained by shifting:
            //Timer1Overflow = 0x0010,
            //Timer2Overflow = 0x0020,
            //Timer3Overflow = 0x0040,
            SerialCommunication = 0x0080,
            DMA = 0x0100,
            // obtained by shifting:
            //DMA1 = 0x0200,
            //DMA2 = 0x0400,
            //DMA3 = 0x0800,
            Keypad = 0x1000,
            GamePak = 0x2000
        }

        private bool HandleIRQs()
        {
            if ((this.IF.raw & this.IE.raw) != 0)  //  & 0b0011_1111_1111_1100  // mask interrupts manually for testing
            {
                this.HALTCNT.Halt = false;

                // flipping IME makes irq_demo and Endrift's suite work, so this can be (ab)used for testing
                if (this.IME.Enabled && (this.I == 0))
                {
                    this.DoIRQ();
                    return true;
                }
            }
            return false;
        }

        // public to allow for manual IRQ throwing for testing (unstable)
        public void DoIRQ()
        {
            this.Log("Doing IRQ: " + (this.IF.raw & this.IE.raw).ToString("x8"));
            this.ChangeMode(Mode.IRQ);
            this.I = 1;

            // store address of instruction that did not get executed + 4
            // we check for IRQ before filling the pipeline, so we are 2 (in THUMB) or 4 (ARM) ahead
            LR = this.PC + (uint)((this.state == State.THUMB) ? 2 : 0);  // which is now LR_irq
            this.state = State.ARM;

            this.PC = IRQVector;
            this.PipelineFlush();
        }
    }
}
