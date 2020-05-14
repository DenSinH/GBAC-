using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {

        private uint ARMShiftRegisterBasedOperand(uint Op2, bool ImmediateShift, byte ShiftType, byte ShiftAmount, bool SetConditions)
        {
            // Special cases for 0 shift
            if (ShiftAmount == 0 && ImmediateShift)
            {
                switch (ShiftType)  // Shift type
                {
                    case 0b00:  // Logical Left
                                // No shift applied
                        break;
                    case 0b01:  // Logical Right
                                // Interpreted as LSR#32
                        ShiftAmount = 32;
                        break;
                    case 0b10:  // Arithmetic Right
                                // Interpreted as ASR#32
                        ShiftAmount = 32;
                        break;
                    case 0b11:  // Rotate Right
                                // Interpreted as RRX#1
                        byte newC = (byte)(Op2 & 0x01);
                        Op2 = (Op2 >> 1) | (uint)(this.C << 31);
                        this.C = newC;
                        // Leave ShiftAmount = 0 so that no additional shift is applied
                        break;
                }
            }

            // We have set the shift amount accordingly above if necessary
            // if the shift amount was specified by a register that was 0 in the last byte, no shift was applied, 
            //     and the flags are not affected
            if (ShiftAmount != 0)
            {
                switch (ShiftType)  // Shift type
                {
                    case 0b00:  // Logical Left
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (32 - ShiftAmount)) & 0x01);  // Bit (32 - ShiftAmount) of contents of Rm
                        }
                        Op2 <<= ShiftAmount;
                        break;
                    case 0b01:  // Logical Right
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm
                        }
                        Op2 >>= ShiftAmount;
                        break;
                    case 0b10:  // Arithmetic Right
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                        }
                        bool Bit31 = (Op2 & 0x8000_0000) > 0;
                        Op2 >>= ShiftAmount;
                        if (Bit31)
                        {
                            Op2 |= (uint)(((1 << ShiftAmount) - 1) << (32 - ShiftAmount));
                        }
                        break;
                    case 0b11:  // Rotate Right
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                        }
                        ShiftAmount &= 0x1f;  // mod 32 gives same result
                        Op2 = (uint)((Op2 >> ShiftAmount) | ((Op2 & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
                        break;
                }
            }

            return Op2;
        }

        private void DataProcessing(uint Instruction)
        {
            bool ImmediateOperand = (Instruction & 0x0200_0000) > 0;
            byte OpCode = (byte)((Instruction & 0x01e0_0000) >> 21);
            bool SetConditions = (Instruction & 0x0010_0000) > 0;

            uint Op1 = Registers[(Instruction & 0x000f_0000) >> 16];
            byte Target = (byte)((Instruction & 0x0000_f000) >> 12);
            uint Op2;

            /*
             When Rd is a register other than R15, the condition code flags in the CPSR may be
             updated from the ALU flags as described above.

             When Rd is R15 and the S flag in the instruction is not set the result of the operation
             is placed in R15 and the CPSR is unaffected.

             When Rd is R15 and the S flag is set the result of the operation is placed in R15 and
             the SPSR corresponding to the current mode is moved to the CPSR. This allows state
             changes which atomically restore both PC and CPSR. This form of instruction should
             not be used in User mode
            */
            byte Rd = (byte)((Instruction & 0x0000_f000) >> 12);  // Destination register
            if (Rd == 15)
            {
                SetConditions = false;
            }

            if (!ImmediateOperand)
            {
                Op2 = this.Registers[Instruction & 0x0f];  // Register Rm;

                bool ImmediateShift = (Instruction & 0x10) == 0;
                if ((Instruction & 0x0f) == 15)  // PC
                {
                    /*
                     The PC value will be the address of the instruction, plus 8 or 12 bytes due to instruction
                     prefetching. If the shift amount is specified in the instruction, the PC will be 8 bytes
                     ahead. If a register is used to specify the shift amount the PC will be 12 bytes ahead.
                    */

                }
                // Shift amount is either bottom byte of register or immediate value
                byte ShiftAmount = (byte)(ImmediateShift ? ((Instruction & 0xf80) >> 7) : this.Registers[(Instruction & 0xf00) >> 8] & 0xff);

                Op2 = ARMShiftRegisterBasedOperand(Op2, ImmediateShift, (byte)((Instruction & 0x60) >> 4), ShiftAmount, SetConditions);
            }
            else
            {
                // Immediate operand
                Op2 = Instruction & 0x0ff;
                byte ShiftAmount = (byte)((Instruction & 0xf00) >> 7);  // rotated right by twice the value of the operand
                // Rotate right
                if (ShiftAmount > 0)
                {
                    if (SetConditions)
                    {
                        C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                    }
                    ShiftAmount &= 0x1f;  // mod 32 gives same result
                    Op2 = (uint)((Op2 >> ShiftAmount) | ((Op2 & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
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
                    {
                        this.C = (byte)(Op2 <= Op1 ? 1 : 0);
                        this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
                    }
                    this.Registers[Rd] = Result;
                    break;
                case 0b0011:  // RSB
                    Result = Op2 - Op1;
                    if (SetConditions)
                    {
                        this.C = (byte)(Op1 <= Op2 ? 1 : 0);
                        this.V = (byte)(((Op2 ^ Op1) & (~Op1 ^ Result)) >> 31);
                    }
                    break;
                case 0b0100:  // ADD
                    Result = Op1 + Op2;
                    if (SetConditions)
                    {
                        this.C = (byte)(Op1 + Op2 > 0xffff_ffff ? 1 : 0);
                        this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ Op2)) >> 31);
                    }
                    this.Registers[Rd] = Result;
                    break;
                case 0b0101:  // ADC
                    Result = Op1 + Op2 + C;
                    if (SetConditions)
                    {
                        this.C = (byte)(Op1 + Op2 + C > 0xffff_ffff ? 1 : 0);
                        this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ (Op2 + C))) >> 31);
                    }
                    break;
                case 0b0110:  // SBC
                    temp = Op2 - C + 1;
                    Result = (uint)(Op1 - temp);
                    if (SetConditions)
                    {
                        this.C = (byte)(temp <= Op1 ? 1 : 0);
                        this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
                    }
                    this.Registers[Rd] = Result;
                    break;
                case 0b0111:  // RSC
                    temp = Op1 - C + 1;
                    Result = (uint)(Op2 - temp);
                    if (SetConditions)
                    {
                        this.C = (byte)(temp <= Op2 ? 1 : 0);
                        this.V = (byte)(((Op2 ^ Op1) & (~Op1 ^ Result)) >> 31);
                    }
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
                    {
                        this.C = (byte)(Op2 <= Op1 ? 1 : 0);
                        this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
                    }
                    break;
                case 0b1011:  // CMN
                    Result = Op1 + Op2;
                    if (SetConditions)
                    {
                        this.C = (byte)(Op1 + Op2 > 0xffff_ffff ? 1 : 0);
                        this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ Op2)) >> 31);
                    }
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
                this.Registers[Rd] = Result;
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
