using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void MoveShiftedRegister(ushort Instruction)
        {
            this.Log("Move shifted register");
            byte Opcode, Offset5, Rs, Rd;
            Opcode = (byte)((Instruction & 0x1800) >> 11);
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);
            Rs = (byte)((Instruction & 0x0038) >> 3);  // Source Register
            Rd = (byte)(Instruction & 0x007);  // Destination Register

            uint Result = this.Registers[Rs];
            Result = ShiftOperand(Result, true, Opcode, Offset5, true);
            this.SetNZ(Result);

            this.Registers[Rd] = Result;
        }
    }
}
