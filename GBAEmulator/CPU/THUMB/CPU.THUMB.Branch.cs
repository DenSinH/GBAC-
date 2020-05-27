using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private byte ConditionalBranch(ushort Instruction)
        {
            if ((Instruction & 0xff00) == 0xdf00)
            {
                // "ambiguity"
                return this.SWIInstruction(Instruction);
            }

            /*
             The instructions in this group all perform a conditional Branch depending on the state
             of the CPSR condition codes. The branch offset must take account of the prefetch
             operation, which causes the PC to be 1 word (4 bytes) ahead of the current instruction

             (manual)

             My PC is always 4 bytes ahead in THUMB mode, so I don't need to account for this.
            */
            byte Condition;
            uint SOffset8;

            Condition = (byte)((Instruction & 0x0f00) >> 8);
            SOffset8 = (uint)(Instruction & 0x00ff) << 1;

            int SignedOffset = (int)(((SOffset8 & 0x100) > 0) ? ((int)SOffset8 - 0x200) : (int)SOffset8);

            this.Log(string.Format("Conditional Branch, Offset {0}", SignedOffset));

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
                if (this.Condition(Condition))
                {
                    this.Log("Branch taken");
                    this.PC = (uint)(this.PC + SignedOffset);
                    this.PipelineFlush();
                }
            }

            // 2S + 1N cycles
            return (SCycle << 1) + NCycle;
        }

        private byte UnconditionalBranch(ushort Instruction)
        {
            /*
             The address specified by label is a full 12-bit two’s complement address, but must
             always be halfword aligned (ie bit 0 set to 0), since the assembler places label >> 1 in
             the Offset11 field.
             (manual)
            */
            ushort Offset11 = (ushort)((Instruction & 0x07ff) << 1);
            int TrueOffset = ((Offset11 & 0x0800) > 0) ? (Offset11 - 0x1000) : Offset11;
            this.PC = (uint)(this.PC + TrueOffset) & 0xffff_fffe;
            this.PipelineFlush();

            this.Log(string.Format("Unconditional Branch, Offset {0}", TrueOffset));

            // 2S + 1N cycles
            return (SCycle << 1) + NCycle;
        }

        private byte LongBranchWithLink(ushort Instruction)
        {
            this.Log("Long Branch With Link");
            bool H;
            uint Offset;

            H = (Instruction & 0x0800) > 0;
            Offset = (uint)(Instruction & 0x07ff);

            if (!H)
            {
                // First Instruction - LR = PC+4+(nn SHL 12)
                // PC + 4 is just because it's 2 instructions ahead in the above description
                int TrueOffset = (int)((Offset & 0x400) > 0 ? ((int)Offset - 0x800) : (int)Offset);
                LR = (uint)(PC + (TrueOffset << 12));
            }
            else
            {
                // Second Instruction - PC = LR + (nn SHL 1), and LR = PC+2 OR 1
                uint temp = PC - 2;  // PC is 4 instructions ahead, 
                PC = LR + (Offset << 1);
                LR = temp | 1;
                PC &= 0xffff_fffe;
                this.PipelineFlush();
            }

            // see 10.3 in Instruction cycle operations in the manual
            return 3 * SCycle + NCycle;
        }
    }
}
