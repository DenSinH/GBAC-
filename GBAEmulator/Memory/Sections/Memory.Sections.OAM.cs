using System;

namespace GBAEmulator.Memory.Sections
{
    public class OAMSection : MirroredMemorySection
    {
        public OAMSection() : base(0x400) { }

        public override void SetByteAt(uint address, byte value)
        {
            /*
            GBATek:
                Writing 8bit Data to Video Memory
            Video Memory (BG, OBJ, OAM, Palette) can be written to in 16bit and 32bit units only.
            Attempts to write 8bit data (by STRB opcode) won't work:

            Writes ... 
            to OAM (7000000h-70003FFh) are ignored, the memory content remains unchanged.
            */
        }
    }
}
