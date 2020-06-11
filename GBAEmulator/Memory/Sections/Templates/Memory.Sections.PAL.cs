using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Sections.Templates
{
    public class PALSection : MirroredMemorySection
    {
        public PALSection() : base(0x400) { }

        public override void SetByteAt(uint address, byte value)
        {
            /*
             GBATek:
                Writing 8bit Data to Video Memory
            Video Memory (BG, OBJ, OAM, Palette) can be written to in 16bit and 32bit units only.
            Attempts to write 8bit data (by STRB opcode) won't work:

            ...

            Writes to ...
            and to Palette (5000000h-50003FFh) are writing the new 8bit value to BOTH upper and
            lower 8bits of the addressed halfword, ie. "[addr AND NOT 1]=data*101h".
            */
            this.Storage[address & 0x3fe] = value;
            this.Storage[(address & 0x3fe) + 1] = value;
        }
    }
}
