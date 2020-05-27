using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private byte MovCmpAddSubImmediate(ushort Instruction)
        {
            byte opcode, Rd, Offset8;
            uint Result;

            opcode = (byte)((Instruction & 0x1800) >> 11);
            Rd = (byte)((Instruction & 0x0700) >> 8);  // Source/Destination register
            Offset8 = (byte)(Instruction & 0x00ff);

            switch (opcode)
            {
                case 0b00:  // MOV
                    this.Log(string.Format("MOV/CMP/ADD/SUB Immediate: MOV {0:x4} -> R{1}", Offset8, Rd));
                    Result = Offset8;
                    this.Registers[Rd] = Result;
                    break;
                case 0b01:  // CMP
                    this.Log(string.Format("MOV/CMP/ADD/SUB Immediate: CMP R{1} - {0:x4}", Offset8, Rd));
                    Result = this.Registers[Rd] - Offset8;
                    this.SetCVSub(this.Registers[Rd], Offset8, Result);
                    break;
                case 0b10:  // ADD
                    this.Log(string.Format("MOV/CMP/ADD/SUB Immediate: ADD {0:x4} + R{1} -> R{1}", Offset8, Rd));
                    Result = this.Registers[Rd] + Offset8;
                    this.SetCVAdd(this.Registers[Rd], Offset8, Result);
                    this.Registers[Rd] = Result;
                    break;
                case 0b11:  // SUB
                    this.Log(string.Format("MOV/CMP/ADD/SUB Immediate: SUB R{1} - {0:x4} -> R{1}", Offset8, Rd));
                    Result = this.Registers[Rd] - Offset8;
                    this.SetCVSub(this.Registers[Rd], Offset8, Result);
                    this.Registers[Rd] = Result;
                    break;
                default:
                    throw new Exception("This cannot happen");
            }

            this.SetNZ(Result);

            // equivalent instruction is MOV/CMP/SUB/ADD #imm. Rd cannot be PC as we are in THUMB mode
            return this.DataProcessingTimings(true, false);
        }
    }
}
