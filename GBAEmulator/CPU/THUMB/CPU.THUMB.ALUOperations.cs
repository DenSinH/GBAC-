using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int ALUOperations(ushort Instruction)
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
                    this.Log(string.Format("R{0} AND R{1} -> R{1}", Rs, Rd));
                    Result = Op1 & Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0001:  // EOR
                    this.Log(string.Format("R{0} EOR R{1} -> R{1}", Rs, Rd));
                    Result = Op1 ^ Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b0010:  // LSL
                    this.Log(string.Format("R{0} LSL R{1} -> R{1}", Rs, Rd));
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b00, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0011:  // LSR
                    this.Log(string.Format("R{0} LSR R{1} -> R{1}", Rs, Rd));
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b01, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0100:  // ASR
                    this.Log(string.Format("R{0} ASR R{1} -> R{1}", Rs, Rd));
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b10, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0101:  // ADC
                    this.Log(string.Format("R{0} ADC R{1} -> R{1}", Rs, Rd));
                    Result = Op1 + Op2 + this.C;
                    this.SetCVAdd(Op1, (ulong)Op2 + this.C, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0110:  // SBC
                    this.Log(string.Format("R{0} SBC R{1} -> R{1}", Rs, Rd));
                    uint temp = Op2 - C + 1;
                    Result = (uint)(Op1 - temp);
                    this.SetCVSubC(Op1, Op2, C, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b0111:  // ROR
                    this.Log(string.Format("R{0} ROR R{1} -> R{1}", Rs, Rd));
                    if (Op2 >= 0x100)  // "Overshifting"
                        Op2 = 0xff;
                    Result = this.ShiftOperand(Op1, false, 0b11, (byte)Op2, true);
                    this.Registers[Rd] = Result;
                    break;
                case 0b1000:  // TST
                    this.Log(string.Format("R{0} TST R{1} (AND, no store)", Rs, Rd));
                    Result = Op1 & Op2;
                    break;
                case 0b1001:  // NEG
                    this.Log(string.Format("-R{0} -> R{1}", Rs, Rd));
                    Result = (uint)-Op2;
                    this.SetCVSub(0, Op2, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b1010:  // CMP
                    this.Log(string.Format("R{0} CMP R{1} (SUB, no store)", Rs, Rd));
                    Result = Op1 - Op2;
                    this.SetCVSub(Op1, Op2, Result);
                    break;
                case 0b1011:  // CMN
                    this.Log(string.Format("R{0} CMD R{1} (ADD, no store)", Rs, Rd));
                    Result = Op1 + Op2;
                    this.SetCVAdd(Op1, Op2, Result);
                    break;
                case 0b1100:  // ORR
                    this.Log(string.Format("R{0} ORR R{1} -> R{1}", Rs, Rd));
                    Result = Op1 | Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1101:  // MUL
                    this.Log(string.Format("R{0} MUL R{1} -> R{1}", Rs, Rd));
                    Result = Op1 * Op2;
                    // Overflow and carry give garbage result
                    this.Registers[Rd] = Result;
                    break;
                case 0b1110:  // BIC
                    this.Log(string.Format("R{0} BIC R{1} -> R{1} (so R & ~R')", Rs, Rd));
                    Result = Op1 & ~Op2;
                    this.Registers[Rd] = Result;
                    break;
                case 0b1111:  // MVN
                    this.Log(string.Format("~R{0} -> R{1}", Rs, Rd));
                    Result = ~Op2;
                    this.Registers[Rd] = Result;
                    break;
                default:
                    throw new Exception("Yo I programmed this wrong");
            }
            this.SetNZ(Result);

            // We always use a register as operand, Rd cannot be PC as we are in THUMB mode
            return this.DataProcessingTimings(false);
        }
    }
}
