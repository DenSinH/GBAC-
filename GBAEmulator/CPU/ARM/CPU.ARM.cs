using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        // todo: SWI, Coprocessor instructions, Undefined

        private delegate void ARMInstruction(uint Instruction);
        private ARMInstruction[] ARMInstructions = new ARMInstruction[0x1000];  // 12 bits determine the instruction

        private void InitARM()
        {
            for (uint Instruction = 0; Instruction < 0x1000; Instruction++)
            {
                // Bits 27-20 and 7-4 of an instruction
                switch ((Instruction & 0xc00) >> 10)
                {
                    case 0b00:
                        // Data Processing / Multiply / Multiply Long / Single Data Swap / Branch & Exchange / Halfword Data Transfer
                        if ((Instruction & 0xfcf) == 0x009)
                        {
                            // Multiply
                            this.ARMInstructions[Instruction] = this.Multiply;
                        }
                        else if ((Instruction & 0xf8f) == 0x089)
                        {
                            // Multiply Long
                            this.ARMInstructions[Instruction] = this.MultiplyLong;
                        }
                        else if ((Instruction & 0xfbf) == 0x109)
                        {
                            // Single Data Swap
                            this.ARMInstructions[Instruction] = this.SWP;
                        }
                        else if ((Instruction & 0xfff_fff) == 0x12f_ff1)
                        {
                            // Branch and Exchange
                            this.ARMInstructions[Instruction] = this.BX;
                        }
                        else if ((Instruction & 0xe49) == 0x009)
                        {
                            // Halfword Data Transfer: Register Offset
                            this.ARMInstructions[Instruction] = this.Halfword_SignedDataTransfer;
                        }
                        else if ((Instruction & 0xe49) == 0x049)
                        {
                            // Halfword Data Transfer: Immediate Offset
                            this.ARMInstructions[Instruction] = this.Halfword_SignedDataTransfer;
                        }
                        // SOME INSTRUCTIONS ARE A BIT AMBIGUOUS:
                        else if ((Instruction & 0xfbf) == 0x100)
                        {
                            // MRS (transfer PSR contents to a register)
                            this.ARMInstructions[Instruction] = this.PossiblePSRTransfer;
                        }
                        else if ((Instruction & 0xfbf) == 0x120)
                        {
                            // MSR (transfer register contents to PSR)
                            this.ARMInstructions[Instruction] = this.PossiblePSRTransfer;
                        }
                        else if ((Instruction & 0xfb0) == 0x320)
                        {
                            // MSR (transfer register contents or immediate value to PSR flag bits only)
                            // I is not set / I is set
                            this.ARMInstructions[Instruction] = this.PossiblePSRTransfer;
                        }
                        else
                        {
                            // Data Processing
                            this.ARMInstructions[Instruction] = this.DataProcessing;
                        }
                        break;

                    case 0b01:
                        // Single Data Transfer / Undefined
                        if ((Instruction & 0xe01) == 0x601)
                        {
                            // Undefined
                            this.ARMInstructions[Instruction] = (uint _) => throw new NotImplementedException("Undefined instruction");
                        }
                        else
                        {
                            // Single Data Transfer
                            this.ARMInstructions[Instruction] = this.SingleDataTransfer;
                        }
                        break;

                    case 0b10:
                        // Block Data Transfer / Branch
                        if ((Instruction & 0xe00) == 0x800)
                        {
                            // Block Data Transfer
                            this.ARMInstructions[Instruction] = this.BlockDataTransfer;
                        }
                        else
                        {
                            // Branch
                            this.ARMInstructions[Instruction] = this.Branch;
                        }
                        break;

                    case 0b11:
                        // Coprocessor Data (Transfer/Operation) / Coprocessor Register Transfer / SWI
                        if ((Instruction & 0xe00) == 0xc00)
                        {
                            // Coprocessor Data Transfer
                            this.ARMInstructions[Instruction] = (uint _) => throw new NotImplementedException("Coprocessor Data Transfer instruction");
                        }
                        else if ((Instruction & 0xf01) == 0xe00)
                        {
                            // Coprocessor Data Operation
                            this.ARMInstructions[Instruction] = (uint _) => throw new NotImplementedException("Coprocessor Data Operation instruction");
                        }
                        else if ((Instruction & 0xf01) == 0xe01)
                        {
                            // Coprocessor Register Transfer
                            this.ARMInstructions[Instruction] = (uint _) => throw new NotImplementedException("Coprocessor Register Transfer instruction");
                        }
                        else
                        {
                            //SWI
                            this.ARMInstructions[Instruction] = this.SWIInstruction;
                        }
                        break;
                }
            }
        }
        

        private bool ARMCondition(byte field)
        {
            switch (field)
            {
                case 0b0000:  // EQ
                    return Z == 1;
                case 0b0001:  // NE
                    return Z == 0;
                case 0b0010:  // CS
                    return C == 1;
                case 0b0011:  // CC
                    return C == 0;
                case 0b0100:  // MI
                    return N == 1;
                case 0b0101:  // PL
                    return N == 0;
                case 0b0110:  // VS
                    return V == 1;
                case 0b0111:  // VC
                    return V == 0;
                case 0b1000:  // HI
                    return (C == 1) && (Z == 0);
                case 0b1001:  // LS
                    return (C == 0) && (Z == 1);
                case 0b1010:  // GE
                    return N == V;
                case 0b1011:  // LT
                    return N != V;
                case 0b1100:  // GT
                    return (Z == 0) && (N == V);
                case 0b1101:  // LE
                    return (Z == 1) || (N != V);
                case 0b1110:  // AL
                    return true;
                default:
                    throw new Exception(string.Format("Condition field {0} reserved/invalid", field));
            }
        }

        private void ExecuteARM(uint Instruction)
        {
            this.Log(string.Format("ARM: {0:x8} :: PC: {1:x8} :: CPSR: {2:x8}", Instruction, this.PC - 8, this.CPSR));

            if (!ARMCondition((byte)((Instruction & 0xf000_0000) >> 28)))
            {
                this.Log("Condition false");
                return;
            }

            if ((Instruction & 0x0fff_fff0) == 0x012f_ff10)
            {
                this.BX(Instruction);
                return;
            }

            ushort InstructionShorthand = (ushort)(((Instruction & 0x0ff0_0000) >> 16) | ((Instruction & 0x00f0) >> 4));
            this.ARMInstructions[InstructionShorthand](Instruction);
            return;
        }
    }
}
