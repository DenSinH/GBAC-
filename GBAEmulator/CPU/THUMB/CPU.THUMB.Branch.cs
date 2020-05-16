using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void ConditionalBranch(ushort Instruction)
        {
            this.Log("THUMB Conditional Branch");
            /*
             The instructions in this group all perform a conditional Branch depending on the state
             of the CPSR condition codes. The branch offset must take account of the prefetch
             operation, which causes the PC to be 1 word (4 bytes) ahead of the current instruction

             My PC is always 4 bytes ahead in THUMB mode, so I don't need to account for this.
            */
            byte Condition;
            uint SOffset8;

            Condition = (byte)((Instruction & 0x0f00) >> 8);
            SOffset8 = (uint)(Instruction & 0x00ff) << 1;

            int SignedOffset = (int)(((SOffset8 & 0x100) > 0) ? -(SOffset8 & 0xff) : (SOffset8 & 0xff));

            /*
             Cond = 1110 is undefined, and should not be used.
             Cond = 1111 creates the SWI instruction: 

             Other than that, the conditions are the same as the ARM conditions for an instruction
            */
            if (Condition == 0b1110)
            {
                // Unused
                this.Error("Condition 0b1110 illegal for THUMB Conditional Branch!");
            }
            else if (Condition == 0b1111)
            {
                throw new NotImplementedException("THUMB mode SWI is not implemented yet");
            }
            else
            {
                if (this.ARMCondition(Condition))
                {
                    this.PC = (uint)(this.PC + SignedOffset);
                    this.PC |= 1;  // THUMB mode bit
                }
            }
        }

        private void UnconditionalBranch(ushort Instruction)
        {
            this.Log("THUMB Unconditional Branch");
            /*
             The address specified by label is a full 12-bit two’s complement address, but must
             always be halfword aligned (ie bit 0 set to 0), since the assembler places label >> 1 in
             the Offset11 field.
            */
            ushort Offset11 = (ushort)((Instruction & 0x07ff) << 1);
            int TrueOffset = ((Offset11 & 0x0800) > 0) ? -(Offset11 & 0x07ff) : (Offset11 & 0x07ff);
            this.PC = (uint)(this.PC + TrueOffset);
            this.PC |= 1;  // THUMB mode bit
        }

        private void LongBranchWithLink(ushort Instruction)
        {
            bool H;
            uint Offset;

            H = (Instruction & 0x0800) > 0;
            Offset = (uint)(Instruction & 0x07ff);

            if (!H)
            {
                // First Instruction - LR = PC+4+(nn SHL 12)
                LR = PC + (Offset << 12) + 4;
            }
            else
            {
                // Second Instruction - PC = LR + (nn SHL 1), and LR = PC+2 OR 1
                uint temp = PC;
                PC = LR + (Offset << 1);
                LR = (temp + 2) | 1;
            }
        }
    }
}
