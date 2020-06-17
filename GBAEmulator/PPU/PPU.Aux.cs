using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Video
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
                 this.gba.mem.PAL[Address] |
                (this.gba.mem.PAL[Address + 1] << 8)
                );
        }

        private ushort Backdrop
        {
            get => (ushort)(this.gba.mem.PAL[0] | (this.gba.mem.PAL[1] << 8));
        }
    }
}
