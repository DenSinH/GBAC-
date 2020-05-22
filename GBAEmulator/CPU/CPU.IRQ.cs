using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void DoIRQ()
        {
            this.Log("Doing IRQ");
            this.ChangeMode(Mode.IRQ);
            this.I = 1;

            // store address of instruction that did not get executed + 4
            // we check for IRQ before filling the pipeline, so we are 2 (in THUMB) or 4 (ARM) ahead
            LR = this.PC + (uint)((this.state == State.THUMB)? 2 : 0);  // which is now LR_irq
            this.state = State.ARM;

            this.PC = this.IRQVector;
            this.PipelineFlush();
        }
    }
}
