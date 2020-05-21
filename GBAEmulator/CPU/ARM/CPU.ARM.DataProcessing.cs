using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void DataProcessing(uint Instruction)
        {
            this.Log("Data Processing");

            bool ImmediateOperand = (Instruction & 0x0200_0000) > 0;
            byte OpCode = (byte)((Instruction & 0x01e0_0000) >> 21);
            bool SetConditions = (Instruction & 0x0010_0000) > 0;

            byte Rn = (byte)((Instruction & 0x000f_0000) >> 16);
            uint Op1 = Registers[Rn];
            byte Target = (byte)((Instruction & 0x0000_f000) >> 12);
            uint Op2;

            byte OldC = this.C;

            /*
             When Rd is a register other than R15, the condition code flags in the CPSR may be
             updated from the ALU flags as described above.

             When Rd is R15 and the S flag in the instruction is not set the result of the operation
             is placed in R15 and the CPSR is unaffected.

             When Rd is R15 and the S flag is set the result of the operation is placed in R15 and
             the SPSR corresponding to the current mode is moved to the CPSR. This allows state
             changes which atomically restore both PC and CPSR. This form of instruction should
             not be used in User mode

             (manual)
            */
            byte Rd = (byte)((Instruction & 0x0000_f000) >> 12);  // Destination register
            
            if (Rd == 15)
            {
                SetConditions = false;
                this.PipelineFlush();
            }

            if (!ImmediateOperand)
            {
                byte Rm = (byte)(Instruction & 0x0f);
                Op2 = this.Registers[Rm];

                bool ImmediateShift = (Instruction & 0x10) == 0;
                /*
                The PC value will be the address of the instruction, plus 8 or 12 bytes due to instruction
                prefetching. If the shift amount is specified in the instruction, the PC will be 8 bytes
                ahead. If a register is used to specify the shift amount the PC will be 12 bytes ahead.
                (manual)
                */
                if (Rm == 15 && !ImmediateShift)  // PC
                    Op2 += 4;  // My PC is always 8 bytes ahead of the instruction.

                if (Rn == 15 && !ImmediateShift)
                    Op1 += 4; // Same thing

                // Shift amount is either bottom byte of register or immediate value
                byte ShiftAmount = (byte)(ImmediateShift ? ((Instruction & 0xf80) >> 7) : this.Registers[(Instruction & 0xf00) >> 8] & 0xff);

                Op2 = ShiftOperand(Op2, ImmediateShift, (byte)((Instruction & 0x60) >> 5), ShiftAmount, SetConditions);
            }
            else
            {
                // Immediate operand
                Op2 = Instruction & 0x0ff;
                byte ShiftAmount = (byte)((Instruction & 0xf00) >> 7);  // rotated right by twice the value of the operand
                // Rotate right
                if (ShiftAmount > 0)
                {
                    ShiftAmount &= 0x1f;  // mod 32 gives same result
                    Op2 = (uint)((Op2 >> ShiftAmount) | ((Op2 & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
                    if (SetConditions)
                    {
                        this.C = (byte)(Op2 >> 31);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                    }
                }
            }

            uint Result;
            ulong temp;
            
            switch ((Instruction & 0x01e0_0000) >> 21)
            {
                case 0b0000:  // AND
                    Result = Op1 & Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0001:  // EOR:
                    Result = Op1 ^ Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0010:  // SUB
                    Result = Op1 - Op2;
                    if (SetConditions)
                        this.SetCVSub(Op1, Op2, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0011:  // RSB
                    Result = Op2 - Op1;
                    if (SetConditions)
                        this.SetCVSub(Op2, Op1, Result);
                    break;
                case 0b0100:  // ADD
                    Result = Op1 + Op2;
                    if (SetConditions)
                        this.SetCVAdd(Op1, Op2, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0101:  // ADC
                    Result = Op1 + Op2 + OldC;
                    if (SetConditions)
                        this.SetCVAdd(Op1, (ulong)Op2 + OldC, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0110:  // SBC
                    temp = Op2 - OldC + 1;
                    Result = (uint)(Op1 - temp);
                    if (SetConditions)
                        this.SetCVSub(Op1, temp, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0111:  // RSC
                    temp = Op1 - OldC + 1;
                    Result = (uint)(Op2 - temp);
                    if (SetConditions)
                        this.SetCVSub(Op2, temp, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b1000:  // TST
                    Result = Op1 & Op2;
                    break;
                case 0b1001:  // TEQ
                    Result = Op1 ^ Op2;
                    break;
                case 0b1010:  // CMP
                    Result = Op1 - Op2;
                    if (SetConditions)
                        this.SetCVSub(Op1, Op2, Result);
                    break;
                case 0b1011:  // CMN
                    Result = Op1 + Op2;
                    if (SetConditions)
                        this.SetCVAdd(Op1, Op2, Result);
                    break;
                case 0b1100:  // ORR
                    Result = Op1 | Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1101:  // MOV
                    Result = Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1110:  // BIC
                    Result = Op1 & ~Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1111:  // MVN
                    Result = ~Op2;
                    this.Registers[Rd] = Result;
                    break;
                default:
                    throw new Exception("This cannot happen");
            }

            // Setting NZ flags if necessary
            if (SetConditions)
            {
                this.SetNZ(Result);
            }
            else if (Rd == 15)
            {
                // Special cases for writing to PC
                if ((Instruction & 0x0010_0000) > 0)  // S bit
                {
                    this.CPSR = this.SPSR;
                }
            }

            /*
             Processing Type                                                Cycles
             Normal Data Processing                                         1S
             Data Processing with register specified shift                  1S + 1I
             Data Processing with PC written                                2S + 1N
             Data Processing with register specified shift and PC written   2S + 1N + 1I
             */
        }

    }
}
