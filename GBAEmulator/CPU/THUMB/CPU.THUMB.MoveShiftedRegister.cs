using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void MoveShiftedRegister(ushort Instruction)
        {
            byte Opcode, Offset5, Rs, Rd;
            Opcode = (byte)((Instruction & 0x1800) >> 11);
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);
            Rs = (byte)((Instruction & 0x0038) >> 3);  // Source Register
            Rd = (byte)(Instruction & 0x007);  // Destination Register

            uint Value = this.Registers[Rs];
            if (Offset5 > 0)
            {
                switch (Opcode)
                {
                    case 0b00:  // Logical Left
                        C = (byte)((Value>> (32 - Offset5)) & 0x01);  // Bit (32 - Offset5) of contents of Rm
                        Value<<= Offset5;
                        break;
                    case 0b01:  // Logical Right
                        C = (byte)((Value>> (Offset5 - 1)) & 0x01);  // Bit (Offset5 - 1) of contents of Rm
                        Value>>= Offset5;
                        break;
                    case 0b10:  // Arithmetic Right
                        C = (byte)((Value>> (Offset5 - 1)) & 0x01);  // Bit (Offset5 - 1) of contents of Rm, similar to LSR
                        bool Bit31 = (Value& 0x8000_0000) > 0;
                        Value>>= Offset5;
                        if (Bit31)
                        {
                            Value|= (uint)(((1 << Offset5) - 1) << (32 - Offset5));
                        }
                        break;
                }
            }
            this.SetNZ(Value);

            this.Registers[Rd] = Value;
        }
    }
}
