using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {

        private void SWP(uint Instruction)
        {
            this.Log("Single data swap");

            bool ByteQuantity;
            byte Rn, Rd, Rm;

            ByteQuantity = (Instruction & 0x0040_0000) > 0;  // Byte / Word quantity to swap
            Rn = (byte)((Instruction & 0x000f_0000) >> 16);  // Base Address
            Rd = (byte)((Instruction & 0x0000_f000) >> 12);  // Destination Register
            Rm = (byte)(Instruction & 0x000f);               // Source Register

            uint MemoryContent;
            if (ByteQuantity)
            {
                MemoryContent = this.GetAt<byte>(this.Registers[Rn]);
                this.SetAt<byte>(this.Registers[Rn], (byte)this.Registers[Rm]);
                this.Registers[Rd] = MemoryContent;
            }
            else
            {
                MemoryContent = this.GetAt<uint>(this.Registers[Rn]);
                this.SetAt<uint>(this.Registers[Rn], this.Registers[Rm]);
                this.Registers[Rd] = MemoryContent;
            }
        }

    }
}
