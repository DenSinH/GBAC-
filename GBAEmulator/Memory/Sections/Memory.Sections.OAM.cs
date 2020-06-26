using System;

using GBAEmulator.Video;

namespace GBAEmulator.Memory.Sections
{
    public class OAMSection : MirroredMemorySection
    {
        private PPU ppu;

        public OAMSection() : base(0x400) { }

        public void Init(PPU ppu)
        {
            this.ppu = ppu;
        }

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

        public override void SetHalfWordAt(uint address, ushort value)
        {
            this.ppu.Wait();
            base.SetHalfWordAt(address, value);
        }

        public override void SetWordAt(uint address, uint value)
        {
            this.ppu.Wait();
            base.SetWordAt(address, value);
        }
    }
}
