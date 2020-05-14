﻿using System;

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

            // 2S + 1N cycles
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

            // 2S + 1N cycles
        }
    }
}
