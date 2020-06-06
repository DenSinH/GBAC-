using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator
{
    partial class PPU
    {
        public bool IsVBlank
        {
            get { return (scanline >= 160) && (scanline < ScanlinesPerFrame); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                this.gba.mem.PaletteRAM[Address] |
                (this.gba.mem.PaletteRAM[Address + 1] << 8)
                );
        }

        private ushort Backdrop
        {
            get => (ushort)(this.gba.mem.PaletteRAM[0] | (this.gba.mem.PaletteRAM[1] << 8));
        }
    }
}
