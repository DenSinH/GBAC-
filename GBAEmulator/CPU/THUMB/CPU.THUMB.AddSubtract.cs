using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void AddSubtract(ushort Instruction)
        {
            this.Log("THUMB Add Subtract");
            bool ImmediateOperand, Sub;
            byte Rs, Rd;
            uint Operand, Result;

            ImmediateOperand = (Instruction & 0x0400) > 0;
            Sub = (Instruction & 0x0200) > 0;

            Rs = (byte)((Instruction & 0x0038) >> 3);  // Source register
            Rd = (byte)(Instruction & 0x0007);  // Destination register

            if (ImmediateOperand)
            {
                Operand = (uint)((Instruction & 0x01c0) >> 6);
            }
            else
            {
                Operand = this.Registers[(Instruction & 0x01c0) >> 6];
            }
            
            if (Sub)
            {
                Result = this.Registers[Rs] - Operand;
                this.SetCVSub(this.Registers[Rs], Operand, Result);
            }
            else
            {
                Result = this.Registers[Rs] + Operand;
                this.SetCVAdd(this.Registers[Rs], Operand, Result);
            }
            this.Registers[Rd] = Result;

            this.SetNZ(Result);
        }
    }
}
