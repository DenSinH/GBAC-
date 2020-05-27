using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private byte PCRelativeLoad(ushort Instruction)
        {
            byte Rd;
            uint Address;

            Rd = (byte)((Instruction & 0x0700) >> 8);
            this.Log(string.Format("PC relative load, Mem[PC + {0:x2}] -> R{1}", ((Instruction & 0x00ff) << 2), Rd));

            /*
            The value specified by #Imm is a full 10-bit address, but must always be word-aligned
            (ie with bits 1:0 set to 0), since the assembler places #Imm >> 2 in field Word8.

            The value of the PC will be 4 bytes greater than the address of this instruction, but bit
            1 of the PC is forced to 0 to ensure it is word aligned.
            (manual)

            My PC is always 4 bytes ahead, so I don't have to account for this difference.
            */
            Address = (this.PC & 0xffff_fffc) + (uint)((Instruction & 0x00ff) << 2);
            uint Result = this.GetWordAt(Address & 0xffff_fffc);

            byte RotateAmount = (byte)((Address & 0x03) << 3);

            // ROR result for misaligned addresses
            if (RotateAmount != 0)
                Result = this.ROR(Result, RotateAmount);

            this.Registers[Rd] = Result;

            // Normal LDR instructions take 1S + 1N + 1I (incremental)
            return SCycle + NCycle + ICycle;
        }
    }
}
