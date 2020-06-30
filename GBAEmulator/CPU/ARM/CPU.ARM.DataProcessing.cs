using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int DataProcessingTimings(bool ImmediateOperand)
        {
            // + 1I for SHIFT(Rs)
            return ImmediateOperand ? 0 : ICycle;
        }
        
        private int DataProcessing(uint Instruction)
        {
            bool ImmediateOperand = (Instruction & 0x0200_0000) > 0;
            byte OpCode = (byte)((Instruction & 0x01e0_0000) >> 21);
            bool SetConditions = (Instruction & 0x0010_0000) > 0;

            byte Rn = (byte)((Instruction & 0x000f_0000) >> 16);
            uint Op1 = Registers[Rn];
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
            
            if (Rd == 15) SetConditions = false;

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

                this.Log(string.Format("Data Processing, Op2 = R{0} shift {1}", Rm, ShiftAmount));
            }
            else
            {
                // Immediate operand
                Op2 = Instruction & 0x0ff;
                byte ShiftAmount = (byte)((Instruction & 0xf00) >> 7);  // rotated right by twice the value of the operand

                this.Log(string.Format("Data Processing, Op2 = immediate (hex){0:x2} ROR {1}", Op2, ShiftAmount));

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
            
            switch (OpCode)
            {
                case 0b0000:  // AND
                    this.Log(string.Format("{0:x8} AND {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op1 & Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0001:  // EOR:
                    this.Log(string.Format("{0:x8} EOR {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op1 ^ Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0010:  // SUB
                    this.Log(string.Format("{0:x8} SUB {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op1 - Op2;
                    if (SetConditions) this.SetCVSub(Op1, Op2, Result);

                    this.Registers[Rd] = Result;
                    break;
                case 0b0011:  // RSB
                    this.Log(string.Format("{0:x8} RSB {1:x8} (so {1:x8} - {0:x8}) -> R{2}", Op1, Op2, Rd));
                    Result = Op2 - Op1;
                    if (SetConditions) this.SetCVSub(Op2, Op1, Result);

                    this.Registers[Rd] = Result;
                    break;
                case 0b0100:  // ADD
                    this.Log(string.Format("{0:x8} ADD {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op1 + Op2;
                    if (SetConditions)
                        this.SetCVAdd(Op1, Op2, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0101:  // ADC
                    this.Log(string.Format("{0:x8} ADC {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op1 + Op2 + OldC;
                    if (SetConditions) this.SetCVAddC(Op1, Op2, OldC, Result);
                    
                    this.Registers[Rd] = Result;
                    break;
                case 0b0110:  // SBC
                    this.Log(string.Format("{0:x8} SBC {1:x8} -> R{2}", Op1, Op2, Rd));
                    temp = Op2 - OldC + 1;
                    Result = (uint)(Op1 - temp);
                    if (SetConditions) this.SetCVSubC(Op1, Op2, OldC, Result);

                    this.Registers[Rd] = Result;
                    break;
                case 0b0111:  // RSC
                    this.Log(string.Format("{0:x8} RSC {1:x8} -> R{2}", Op1, Op2, Rd));
                    temp = Op1 - OldC + 1;
                    Result = (uint)(Op2 - temp);
                    if (SetConditions) this.SetCVSubC(Op2, Op1, OldC, Result);

                    this.Registers[Rd] = Result;
                    break;
                case 0b1000:  // TST
                    this.Log(string.Format("{0:x8} TST {1:x8} (AND, no store)", Op1, Op2));
                    Result = Op1 & Op2;
                    break;
                case 0b1001:  // TEQ
                    this.Log(string.Format("{0:x8} TEQ {1:x8} (EOR, no store)", Op1, Op2, Rd));
                    Result = Op1 ^ Op2;
                    break;
                case 0b1010:  // CMP
                    this.Log(string.Format("{0:x8} CMP {1:x8} (SUB, no store)", Op1, Op2, Rd));
                    Result = Op1 - Op2;
                    if (SetConditions) this.SetCVSub(Op1, Op2, Result);

                    break;
                case 0b1011:  // CMN
                    this.Log(string.Format("{0:x8} CMN {1:x8} (ADD, no store)", Op1, Op2, Rd));
                    Result = Op1 + Op2;
                    if (SetConditions) this.SetCVAdd(Op1, Op2, Result);

                    break;
                case 0b1100:  // ORR
                    this.Log(string.Format("{0:x8} ORR {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op1 | Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1101:  // MOV
                    this.Log(string.Format("MOV {1:x8} -> R{2}", Op1, Op2, Rd));
                    Result = Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1110:  // BIC
                    this.Log(string.Format("{0:x8} BIC {1:x8} -> R{2} (Op1 & ~Op2)", Op1, Op2, Rd));
                    Result = Op1 & ~Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1111:  // MVN
                    this.Log(string.Format("MVN {1:x8} -> R{2}", Op1, Op2, Rd));
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

                // Setconditions is always false if Rd == 15
                this.PipelineFlush();
            }
                
            /*
             Processing Type                                                Cycles
             Normal Data Processing                                         1S
             Data Processing with register specified shift                  1S + 1I
             Data Processing with PC written                                2S + 1N
             Data Processing with register specified shift and PC written   2S + 1N + 1I
             */
            return this.DataProcessingTimings(ImmediateOperand);
        }
    }
}
