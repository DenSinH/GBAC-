using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator
{
    partial class PPU
    {
        int scanline = 0;
        
        public bool IsVBlank
        {
            get { return (scanline >= 160) && (scanline < 227); }
        }


        public void HackyMode4Scanline()
        {
            if (!IsVBlank)
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[240 * scanline + x] = this.gba.cpu.GetPaletteEntry(this.gba.cpu.VRAM[240 * scanline + x]);
                }
            }

            scanline++;
            if (scanline == 227)
                scanline = 0;

        }
    }
}
