using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int AddSubtract(ushort Instruction)
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
                this.Log(string.Format("Add Subtract: R{0} +/- {1:x2} -> R{2}", Rs, Operand, Rd));
            }
            else
            {
                Operand = this.Registers[(Instruction & 0x01c0) >> 6];
                this.Log(string.Format("Add Subtract: R{0} +/- R{1} -> R{2}", Rs, (Instruction & 0x01c0) >> 6, Rd));
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

            // equivalent instruction is ADD/SUB #/Rs. Rd cannot be PC as we are in THUMB mode
            return this.DataProcessingTimings(ImmediateOperand);
        }
    }
}
