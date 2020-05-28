using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator
{
    partial class PPU
    {
        public bool IsVBlank
        {
            get { return (scanline >= 160) && (scanline < 227); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                this.gba.cpu.PaletteRAM[Address] |
                (this.gba.cpu.PaletteRAM[Address + 1] << 8)
                );
        }

        private ushort Backdrop
        {
            get => (ushort)(this.gba.cpu.PaletteRAM[0] | (this.gba.cpu.PaletteRAM[1] << 8));
        }
    }
}
