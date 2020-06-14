using System;

using GBAEmulator.Memory;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        // Vector pointers for handlers
        const uint ResetVector          = 0x00;
        const uint UndefVector          = 0x04;  // unused
        const uint SWIVector            = 0x08;
        const uint AbortPrefetchVector  = 0x0c;  // unused
        const uint AbortDataVector      = 0x10;  // unused
        const uint ReservedVector       = 0x14;  // unused
        const uint IRQVector            = 0x18;
        const uint FIQVector            = 0x1c;  // unused
        
        private bool HandleIRQs()
        {
            if ((this.mem.IO.IF.raw & this.mem.IO.IE.raw) != 0)
            {
                this.mem.IO.HALTCNT.Halt = false;
                
                if (this.mem.IO.IME.Enabled && (this.I == 0))
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
            // this.Log("Doing IRQ: " + (this.mem.IF.raw & this.mem.IE.raw).ToString("x8"));
            this.ChangeMode(Mode.IRQ);
            this.I = 1;

            this.mem.CurrentBIOSReadState = MEM.BIOSReadState.DuringIRQ;

            if (this.Pipeline.Count == 0) this.PC += (uint)((this.state == State.ARM) ? 4 : 2);

            // store address of instruction that did not get executed + 4
            // we check for IRQ before filling the pipeline, so we are 2 (in THUMB) or 4 (ARM) ahead
            LR = this.PC + (uint)((this.state == State.THUMB) ? 2 : 0);  // which is now LR_irq
            this.state = State.ARM;

            this.PC = IRQVector;
            this.PipelineFlush();
        }
    }
}
