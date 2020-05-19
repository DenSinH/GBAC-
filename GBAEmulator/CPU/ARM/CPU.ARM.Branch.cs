﻿using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {

        private void BX(uint Instruction)
        {
            // Branch & Exchange instruction
            byte Rn = (byte)(Instruction & 0x0f);
            uint Target = this.Registers[Rn];
            this.state = (State)(Target & 0x01);
            this.PC = Target & 0xffff_fffc;  // Allow for pre-fetch
            this.PipelineFlush();
            
            this.Log("BX: new state: " + this.state);

            // 2S + 1N cycles
        }

        private void Branch(uint Instruction)
        {
            // Branch / Branch with Link
            this.Log("ARM Branch");

            if ((Instruction & 0x0100_0000) > 0)  // Link bit
            {
                this.Registers[14] = (this.PC & 0xffff_fffc) - 4;  // PC is 8 ahead (Prefetch /Decode/ Execute), should be 4
            }

            uint Offset = Instruction & 0xff_ffff;  // 24 bit offset
            bool Negative = (Offset & 0x80_0000) > 0;
            int TrueOffset = Negative? (int)Offset - 0x100_0000 : (int)Offset;  // 2's complement
            TrueOffset <<= 2;

            this.PC = (uint)(this.PC + TrueOffset);
            this.PipelineFlush();

            // 2S + 1N cycles
        }
    }
}
