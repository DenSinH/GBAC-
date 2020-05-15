using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void ALUOperations(ushort Instruction)
        {
            byte Opcode, Rs, Rd;
            uint Result;

            Opcode = (byte)((Instruction & 0x03c) >> 6);
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
                    this.C = (byte)((Op1 >> (32 - (int)Op2)) & 0x01);  // Bit (32 - ShiftAmount) of contents of Rd
                    Result = Op1 << (int)Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0011:  // LSR
                    this.C = (byte)((Op1 >> ((int)Op2 - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rd
                    Result = Op1 >> (int)Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0100:  // ASR
                    this.C = (byte)((Op1 >> ((int)Op2 - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                    bool Bit31 = (Op1 & 0x8000_0000) > 0;
                    Result = Op1 >> (int)Op2;
                    if (Bit31)
                    {
                        Result |= (uint)(((1 << (int)Op2) - 1) << (32 - (int)Op2));
                    }
                    this.Registers[Rd] = Result;
                    break;
                case 0b0101:  // ADC
                    Result = Op1 + Op2 + this.C;
                    this.C = (byte)(Op1 + Op2 + this.C > 0xffff_ffff ? 1 : 0);
                    this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ (Op2 + C))) >> 31);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0110:  // SBC
                    uint temp = Op2 - C + 1;
                    Result = (uint)(Op1 - temp);
                    this.C = (byte)(temp <= Op1 ? 1 : 0);
                    this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0111:  // ROR
                    this.C = (byte)((Op1 >> ((int)Op2 - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                    Op2 &= 0x1f;  // mod 32 gives same result
                    Result = (uint)((Op1 >> (int)Op2) | ((Op1 & ((1 << (int)Op2) - 1)) << (32 - (int)(Op2))));
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
                    this.C = (byte)(Op2 <= Op1 ? 1 : 0);
                    this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
                    break;
                case 0b1011:  // CMN
                    Result = Op1 + Op2;
                    this.C = (byte)(Op1 + Op2 > 0xffff_ffff ? 1 : 0);
                    this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ Op2)) >> 31);
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
