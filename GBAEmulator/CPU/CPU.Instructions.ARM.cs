using System;

namespace GBAEmulator.CPU
{
    partial class CPU
    {

        private void BX(uint Instruction)
        {
            // Branch & Exchange instruction
            uint Rn = this.Registers[Instruction & 0x0f];
            this.state = (State)(Rn & 0x01);
            this.PC = Rn;
            this.PipelineFlush();
        }

        private void Branch(uint Instruction)
        {
            // Branch / Branch with Link

            if ((Instruction & 0x0100_0000) > 0)  // Link bit
            {
                this.Registers[14] = this.PC - 2;  // Allow for prefetch, PC is 3 ahead (Prefetch /Decode/ Execute), just prefetched this + 3
            }

            uint Offset = Instruction & 0xff_ffff;  // 24 bit offset
            bool Negative = (Offset & 0x80_0000) > 0;
            int TrueOffset = Negative? (int)Offset - 0x100_0000 : (int)Offset;  // 2's complement
            TrueOffset <<= 2;

            this.PC = (uint)(this.PC + TrueOffset);
        }

        private void DataProcessing(uint Instruction)
        {
            bool Immediate = (Instruction & 0x0200_0000) > 0;
            byte OpCode = (byte)((Instruction & 0x01e0_0000) >> 21);
            bool SetConditions = (Instruction & 0x0010_0000) > 0;

            uint Op1 = Registers[(Instruction & 0x000f_0000) >> 16];
            byte Target = (byte)((Instruction & 0x0000_f000) >> 12);
            uint Op2;
            if (!Immediate)
            {
                Op2 = this.Registers[Instruction & 0x0f];  // Register Rm;
                // Shift amount is either bottom byte of register or immediate value
                byte ShiftAmount = (byte)(((Instruction & 0x10) > 0) ? this.Registers[(Instruction & 0xf00) >> 8] & 0xff : ((Instruction & 0xf80) >> 7));
                // todo: rotate right extended
                // todo: If this byte is zero, the unchanged contents of Rm will be used as the second operand,
                //       and the old value of the CPSR C flag will be passed on as the shifter carry output.

                switch ((Instruction & 0x60) >> 4)  // Shift type
                {
                    case 0b00:  // Logical Left
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (32 - ShiftAmount)) & 0x01);  // Bit (32 - ShiftAmount) of contents of Rm
                        }
                        Op2 <<= ShiftAmount;
                        break;
                    case 0b01:  // Logical Right
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm
                        }
                        Op2 >>= ShiftAmount;
                        break;
                    case 0b10:  // Arithmetic Right
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                        }
                        bool Bit31 = (Op2 & 0x8000_0000) > 0;
                        Op2 >>= ShiftAmount;
                        if (Bit31)
                        {
                            Op2 |= (uint)(((1 << ShiftAmount) - 1) << (32 - ShiftAmount));
                        }
                        break;
                    case 0b11:  // Rotate Right
                        if (SetConditions)
                        {
                            C = (byte)((Op2 >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                        }
                        Op2 = (uint)((Op2 >> ShiftAmount) | ((Op2 & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
                        break;
                }
            }
        }

    }
}
