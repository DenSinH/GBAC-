using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int SWIInstruction(uint Instruction)
        {
            this.Log(string.Format("SWI: {0:x8}", Instruction));

            this.ChangeMode(Mode.Supervisor);
            this.I = 1;
            LR = this.PC - (uint)((this.state == State.THUMB) ? 2 : 4);  // which is now LR_svc
            this.state = State.ARM;

            this.PC = SWIVector;
            this.PipelineFlush();

            // Software interrupt instructions take 2S + 1N incremental cycles to execute
            return (SCycle << 1) + NCycle;
        }

        private int SWIInstruction(ushort Instruction)
        {
            return this.SWIInstruction((uint)Instruction);
        }
    }
}