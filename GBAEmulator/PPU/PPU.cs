using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    public partial class PPU
    {
        GBA gba;

        /*
         The GBA is capable of displaying 16bit colors in a 5.5.5 format. That means 5 bits for red, 5 for green and 5 for blue;
         the leftover bit is unused. Basically, the bit-pattern looks like this: “ xbbbbbgggggrrrrr”.
         There are a number of defines and macros in color.h that will make dealing with color easier.

         (Tonc)
        */
        readonly ushort[] Display;

        const int width = 240;
        const int height = 160;

        const int ScanlinesPerFrame = 228;

        public PPU(GBA gba, ushort[] display)
        {
            this.gba = gba;
            this.Display = display;

            this.ResetBGScanlines(0, 1, 2, 3);
            this.ResetOBJScanlines();
        }
    }
}
