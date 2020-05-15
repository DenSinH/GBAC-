using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void AddSubtract(ushort Instruction)
        {
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
                this.C = (byte)(Operand <= this.Registers[Rs] ? 1 : 0);
                this.V = (byte)(((this.Registers[Rs] ^ Operand) & (~Operand ^ Result)) >> 31);
            }
            else
            {
                Result = this.Registers[Rs] + Operand;
                this.C = (byte)(this.Registers[Rs] + Operand > 0xffff_ffff ? 1 : 0);
                this.V = (byte)((this.Registers[Rs] ^ Result) & (~(this.Registers[Rs] ^ Operand)) >> 31);
            }
            this.Registers[Rd] = Result;

            this.SetNZ(Result);
        }
    }
}
