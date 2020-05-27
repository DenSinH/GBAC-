using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private byte MoveShiftedRegister(ushort Instruction)
        {
            byte Opcode, Offset5, Rs, Rd;
            Opcode = (byte)((Instruction & 0x1800) >> 11);
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);
            Rs = (byte)((Instruction & 0x0038) >> 3);  // Source Register
            Rd = (byte)(Instruction & 0x007);  // Destination Register

            this.Log(string.Format("Move shifted register, R{0} SHIFT {1} -> R{2}", Rs, Offset5, Rd));
            uint Result = this.Registers[Rs];
            Result = ShiftOperand(Result, true, Opcode, Offset5, true);
            this.SetNZ(Result);

            this.Registers[Rd] = Result;
            
            // equivalent instruction is MOVS #imm, Rd cannot be PC
            return this.DataProcessingTimings(true, false);
        }
    }
}
