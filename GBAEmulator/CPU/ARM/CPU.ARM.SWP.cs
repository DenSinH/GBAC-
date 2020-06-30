using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int SWP(uint Instruction)
        {
            bool ByteQuantity;
            byte Rn, Rd, Rm;

            ByteQuantity = (Instruction & 0x0040_0000) > 0;  // Byte / Word quantity to swap
            Rn = (byte)((Instruction & 0x000f_0000) >> 16);  // Base Address
            Rd = (byte)((Instruction & 0x0000_f000) >> 12);  // Destination Register
            Rm = (byte)(Instruction & 0x000f);               // Source Register

            uint MemoryContent;
            if (ByteQuantity)
            {
                MemoryContent = this.mem.GetByteAt(this.Registers[Rn]);
                this.mem.SetByteAt(this.Registers[Rn], (byte)this.Registers[Rm]);
                this.Registers[Rd] = MemoryContent;
            }
            else
            {
                uint Address = this.Registers[Rn];
                MemoryContent = this.mem.GetWordAt(Address );
                byte RotateAmount = (byte)((Address & 0x03) << 3);

                // ROR result for misaligned addresses
                if (RotateAmount != 0)
                    MemoryContent = ROR(MemoryContent, RotateAmount);

                this.mem.SetWordAt(Address, this.Registers[Rm]);  // force align happens in memory handler
                this.Registers[Rd] = MemoryContent;
            }

            this.Log(string.Format("Single data swap: R{0} <- Mem[R{1}] <- R{2}", Rd, Rn, Rm));

            // Swap instructions take 1S + 2N +1I incremental cycles to execute
            return ICycle;
        }
    }
}
