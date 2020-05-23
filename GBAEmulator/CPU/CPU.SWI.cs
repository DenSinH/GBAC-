using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void SWIInstruction(uint Instruction)
        {
            this.Log(string.Format("SWI: {0:x8}", Instruction));
            this.ChangeMode(Mode.Supervisor);
            this.I = 1;
            LR = this.PC - (uint)((this.state == State.THUMB) ? 2 : 4);  // which is now LR_svc
            this.state = State.ARM;

            this.PC = SWIVector;
            this.PipelineFlush();
        }

        private void SWIInstruction(ushort Instruction)
        {
            this.SWIInstruction((uint)Instruction);
        }
    }
}