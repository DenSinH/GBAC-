using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void MovCmpAddSubImmediate(ushort Instruction)
        {
            this.Log("MOV/CMP/ADD/SUB Immediate");
            byte opcode, Rd, Offset8;
            uint Result;

            opcode = (byte)((Instruction & 0x1800) >> 11);
            Rd = (byte)((Instruction & 0x0700) >> 8);  // Source/Destination register
            Offset8 = (byte)(Instruction & 0x00ff);

            switch (opcode)
            {
                case 0b00:  // MOV
                    Result = Offset8;
                    this.Registers[Rd] = Result;
                    break;
                case 0b01:  // CMP
                    Result = this.Registers[Rd] - Offset8;
                    this.SetCVSub(this.Registers[Rd], Offset8, Result);
                    break;
                case 0b10:  // ADD
                    Result = this.Registers[Rd] + Offset8;
                    this.SetCVAdd(this.Registers[Rd], Offset8, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b11:  // SUB
                    Result = this.Registers[Rd] - Offset8;
                    this.SetCVSub(this.Registers[Rd], Offset8, Result);
                    this.Registers[Rd] = Result;
                    break;
                default:
                    throw new Exception("This cannot happen");
            }

            this.SetNZ(Result);
        }
    }
}
