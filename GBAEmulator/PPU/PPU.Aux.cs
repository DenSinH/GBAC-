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

        public void UpdateRotationScalingParams()
        {
            if (this.IsVBlank)
            {
                this.IO.BG2X.ResetInternal();
                this.IO.BG2Y.ResetInternal();
                this.IO.BG3X.ResetInternal();
                this.IO.BG3Y.ResetInternal();
            }
            else
            {
                this.IO.BG2X.UpdateInternal((uint)this.IO.BG2PB.Full);
                this.IO.BG2Y.UpdateInternal((uint)this.IO.BG2PD.Full);
                this.IO.BG3X.UpdateInternal((uint)this.IO.BG3PB.Full);
                this.IO.BG3Y.UpdateInternal((uint)this.IO.BG3PD.Full);
            }
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
