using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

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

        public readonly bool[] ExternalBGEnable = new bool[4] { true, true, true, true };
        public bool ExternalOBJEnable = true;
        public bool ExternalWindowingEnable = true;
        public bool ExternalBlendingEnable = true;
#if THREADED_RENDERING
        public readonly ManualResetEventSlim StartDrawing = new ManualResetEventSlim(false);
        public readonly ManualResetEventSlim DoneDrawing = new ManualResetEventSlim(true);
        public bool ShutDown;
        public volatile bool Drawing = false;
#endif

        public PPU(GBA gba, ushort[] display, IORAMSection IO)
        {
            this.gba = gba;
            this.IO = IO;
            this.Display = display;

            this.ResetBGScanlines(0, 1, 2, 3);
            this.ResetOBJScanlines();
        }

        public void Trigger()
        {
#if THREADED_RENDERING
            this.Wait();
            this.DoneDrawing.Reset();
            scanline++;
            if (scanline == 228)
            {
                scanline = 0;
                frame++;
            }
            if (!this.IsVBlank)
            {
                this.Drawing = true;
                this.StartDrawing.Set();
            }
#else
            this.DrawScanline();
            scanline++;
            if (scanline == 228)
            {
                scanline = 0;
                frame++;
            }
#endif
        }

        [Conditional("THREADED_RENDERING")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
#if THREADED_RENDERING
            if (this.Drawing)
            {
                DoneDrawing.Wait();
            }
#endif
        }

#if THREADED_RENDERING
        public void Mainloop()
        {
            while (!this.ShutDown)
            {
                StartDrawing.Wait();
                StartDrawing.Reset();
                this.DrawScanline();
                this.Drawing = false;
                DoneDrawing.Set();
            }
        }
#endif
    }
}
