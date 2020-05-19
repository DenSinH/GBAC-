using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void ALUOperations(ushort Instruction)
        {
            this.Log("ALU Operations");
            byte Opcode, Rs, Rd;
            uint Result;

            Opcode = (byte)((Instruction & 0x03c0) >> 6);
            Rs = (byte)((Instruction & 0x0038) >> 3);  // Source register 2
            Rd = (byte)(Instruction & 0x0007);  // Source / Destination register

            uint Op1 = this.Registers[Rd];
            uint Op2 = this.Registers[Rs];

            switch (Opcode)
            {
                case 0b0000:  // AND
                    Result = Op1 & Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0001:  // EOR
                    Result = Op1 ^ Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0010:  // LSL
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b00, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0011:  // LSR
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b01, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0100:  // ASR
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b10, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0101:  // ADC
                    Result = Op1 + Op2 + this.C;
                    this.SetCVAdd(Op1, (ulong)Op2 + this.C, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0110:  // SBC
                    uint temp = Op2 - C + 1;
                    Result = (uint)(Op1 - temp);
                    this.SetCVSub(Op1, temp, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0111:  // ROR
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b11, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b1000:  // TST
                    Result = Op1 & Op2;
                    break;
                case 0b1001:  // NEG
                    Result = (uint)-Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1010:  // CMP
                    Result = Op1 - Op2;
                    this.SetCVSub(Op1, Op2, Result);
                    break;
                case 0b1011:  // CMN
                    Result = Op1 + Op2;
                    this.SetCVAdd(Op1, Op2, Result);
                    break;
                case 0b1100:  // ORR
                    Result = Op1 | Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1101:  // MUL
                    Result = Op1 * Op2;
                    // Overflow and carry give garbage result
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
                    throw new Exception("Yo I programmed this wrong");
            }
            this.SetNZ(Result);
        }
    }
}
