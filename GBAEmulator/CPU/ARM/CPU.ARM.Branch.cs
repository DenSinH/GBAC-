using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {

        private int BX(uint Instruction)
        {
            // Branch & Exchange instruction
            byte Rn = (byte)(Instruction & 0x0f);
            uint Target = this.Registers[Rn];
            this.state = (State)(Target & 0x01);
            if (this.state == State.ARM)
                this.PC = Target & 0xffff_fffc;  // Allow for pre-fetch
            else
                this.PC = Target & 0xffff_fffe;
            this.PipelineFlush();

            this.Log(string.Format("BX: R{0} -> PC", Rn));

            // no I cycles
            return 0;
        }

        private int Branch(uint Instruction)
        {
            // Branch / Branch with Link
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


            this.Log(string.Format("ARM branch (with link?) Offset {0:x8}", TrueOffset));

            // no I cycles
            return 0;
        }
    }
}
