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
        private readonly ManualResetEventSlim StartDrawing = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim DoneDrawing = new ManualResetEventSlim(true);
        private bool ShutDown;
        private bool Alive = true;

        private const int DRAW_READY = 0;
        private const int DRAW_MERGING = 1;
        private const int DRAW_BUSY = 2;
        private volatile int DrawState = DRAW_READY;
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
            if (Interlocked.CompareExchange(ref DrawState, DRAW_READY, DRAW_READY) != DRAW_READY)
            {
                this.DoneDrawing.Wait();
            }
            this.DoneDrawing.Reset();

            this.IO.UpdateLCD();
            this.DrawState = DRAW_BUSY;
            this.StartDrawing.Set();
#else
            this.DrawScanline();
            if (++scanline == 228)
            {
                scanline = 0;
                frame++;
            }
#endif
        }

        [Conditional("THREADED_RENDERING")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BusyWait()
        {
#if THREADED_RENDERING
            // wait until state is DRAW_MERGE or DRAW_READY
            while (Interlocked.CompareExchange(ref DrawState, DRAW_BUSY, DRAW_BUSY) == DRAW_BUSY)
            {
                // this.DoneDrawing.Wait();
            }
#endif
        }

#if THREADED_RENDERING
        public void GetRenderStatus()
        {
            Console.WriteLine($"Drawing state {this.gba.ppu.DrawState}");
            Console.WriteLine($"StartDrawing {this.gba.ppu.StartDrawing.Wait(0)}");
            Console.WriteLine($"DoneDrawing {this.gba.ppu.DoneDrawing.Wait(0)}");
            Console.WriteLine();
        }

        public void PowerOff()
        {
            this.ShutDown = true;

            // allow the ppu to start drawing one last time so that the mainloop can end
            this.StartDrawing.Set();

            // wait for the PPU to die
            while (this.Alive) { Thread.Sleep(10); }
        }

        public void Mainloop()
        {
            while (!this.ShutDown)
            {
                StartDrawing.Wait();
                StartDrawing.Reset();

                this.DrawScanline();
                if (++scanline == 228)
                {
                    scanline = 0;
                    frame++;
                }

                this.UpdateRotationScalingParams();

                DoneDrawing.Set();
                this.DrawState = DRAW_READY;
            }

            this.Alive = false;
        }
#endif
    }
}
