using System;

using GBAEmulator.IO;

namespace GBAEmulator.Video
{
    public partial class PPU
    {
        private readonly GBA gba;
        private readonly IORAMSection IO;

        /*
         The GBA is capable of displaying 16bit colors in a 5.5.5 format. That means 5 bits for red, 5 for green and 5 for blue;
         the leftover bit is unused. Basically, the bit-pattern looks like this: “ xbbbbbgggggrrrrr”.

         (Tonc)
        */
        readonly ushort[] Display;

        const int width = 240;
        const int height = 160;

        const int ScanlinesPerFrame = 228;
        const ushort Transparent = 0x8000;

        public PPU(GBA gba, ushort[] display, IORAMSection IO)
        {
            this.gba = gba;
            this.IO = IO;
            this.Display = display;

            this.ResetBGScanlines(0, 1, 2, 3);
            this.ResetOBJScanlines();
        }
    }
}
