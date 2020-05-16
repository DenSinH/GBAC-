using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private delegate void THUMBInstruction(ushort Instruction);
        private THUMBInstruction[] THUMBInstructions = new THUMBInstruction[0x40];  // top 6 bits are enough to determine what instruction

        private void InitTHUMB()
        {
            for (byte i = 0; i < 64; i++)
            {
                if ((i & 0b111110) == 0b000110)
                    this.THUMBInstructions[i] = this.AddSubtract;
                else if ((i & 0b111000) == 0b000000)
                    this.THUMBInstructions[i] = this.MoveShiftedRegister;
                else if ((i & 0b111000) == 0b001000)
                    this.THUMBInstructions[i] = this.MovCmpAddSubImmediate;
                else if ((i & 0b111111) == 0b010001)
                    this.THUMBInstructions[i] = this.HiReg_BX;
                else if ((i & 0b111110) == 0b010010)
                    this.THUMBInstructions[i] = this.PCRelativeLoad;
                else if ((i & 0b111100) == 0b010100)
                    // Some instructions were combined to fit this in (7 and 8)
                    this.THUMBInstructions[i] = this.LoadStoreRegOffset_SignExtended;
                else if ((i & 0b111000) == 0b011000)
                    this.THUMBInstructions[i] = this.LoadStoreImmediate;
                else if ((i & 0b111100) == 0b100000)
                    this.THUMBInstructions[i] = this.LoadStoreHalfword;
                else if ((i & 0b111100) == 0b100100)
                    this.THUMBInstructions[i] = this.LoadStoreSPRelative;
                else if ((i & 0b111100) == 0b101000)
                    this.THUMBInstructions[i] = this.LoadAddress;
                else if ((i & 0b111111) == 0b101100)
                    this.THUMBInstructions[i] = this.AddOffsetToSP;
                else if ((i & 0b111101) == 0b101101)
                    this.THUMBInstructions[i] = this.PushPopRegisters;
                else if ((i & 0b111100) == 0b110000)
                    this.THUMBInstructions[i] = this.MultipleLoadStore;
                else if ((i & 0b111100) == 0b110100)
                    this.THUMBInstructions[i] = this.ConditionalBranch;
                else if ((i & 0b111110) == 0b111000)
                    this.THUMBInstructions[i] = this.UnconditionalBranch;
                else
                    this.THUMBInstructions[i] = this.LongBranchWithLink;
            }
            // initialize THUMB instructions
        }

        private void ExecuteTHUMB(ushort Instruction)
        {
            this.THUMBInstructions[(Instruction & 0xfc00) >> 10](Instruction);
        }
    }
}
