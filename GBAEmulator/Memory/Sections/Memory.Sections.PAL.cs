using System;

using GBAEmulator.Video;

namespace GBAEmulator.Memory.Sections
{
    public class PALSection : MirroredMemorySection
    {
        private PPU ppu;
        public PALSection() : base(0x400) { }

        public void Init(PPU ppu)
        {
            this.ppu = ppu;
        }

        public override void SetByteAt(uint address, byte value)
        {
#if !UNSAFE_RENDERING
            if (this.GetByteAt(address) != value) this.ppu.BusyWait();
#endif
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

        public override void SetHalfWordAt(uint address, ushort value)
        {
#if !UNSAFE_RENDERING
            if (this.GetHalfWordAt(address) != value) this.ppu.BusyWait();
#endif
            base.SetHalfWordAt(address, value);
        }

        public override void SetWordAt(uint address, uint value)
        {
#if !UNSAFE_RENDERING
            if (this.GetWordAt(address) != value) this.ppu.BusyWait();
#endif
            base.SetWordAt(address, value);
        }
    }
}
