using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator
{
    partial class PPU
    {
        BlendMode[] WindowBlendMode = new BlendMode[width];

        public void ResetWindowBlendMode()
        {
            BlendMode AlphaBlending = this.gba.mem.IORAMSection.BLDCNT.BlendMode;
            BlendMode Win0In, Win1In, OBJWinIn, WinOut;

            Win0In   = this.gba.mem.IORAMSection.WININ .WindowSpecialEffects(Window.Window0)  ? AlphaBlending : BlendMode.Off;
            Win1In   = this.gba.mem.IORAMSection.WININ .WindowSpecialEffects(Window.Window1)  ? AlphaBlending : BlendMode.Off;
            OBJWinIn = this.gba.mem.IORAMSection.WINOUT.WindowSpecialEffects(Window.OBJ)     ? AlphaBlending : BlendMode.Off;
            WinOut   = this.gba.mem.IORAMSection.WINOUT.WindowSpecialEffects(Window.Outside) ? AlphaBlending : BlendMode.Off;

            this.ResetWindow<BlendMode>(ref WindowBlendMode, Win0In, Win1In, OBJWinIn, WinOut, AlphaBlending);
            
            // override for alpha blending objects (?)
            BlendMode OBJBlendMode = AlphaBlending != BlendMode.Off ? AlphaBlending : BlendMode.Normal;

            for (int x = 0; x < width; x++)
            {
                if (this.OBJBlendingMask[x] != null)  // null means no sprite present, so don't enable the blendmode
                {
                    WindowBlendMode[x] = this.OBJBlendingMask[x] ?? false ? OBJBlendMode : BlendMode.Off;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillWindow<T>(ref T[] Window, T value)
        {
            for (int x = 0; x < width; x++) Window[x] = value;
        }
        
        private void MaskWindow<T>(ref T[] Window, T WindowInEnable, byte X1, byte X2, byte Y1, byte Y2)
        {
            if (Y1 <= Y2)
            {
                // no vert wrap and out of bounds: return
                if (scanline < Y1 || scanline > Y2) return;
            }
            else
            {
                // vert wrap and "in bounds": return
                if ((scanline < Y1) && (scanline > Y2)) return;
            }

            if (X1 <= X2)
            {
                // no hor wrap
                // slice in WININ
                for (int x = X1; x < X2; x++)
                {
                    Window[x] = WindowInEnable;
                } 
            }
            else
            {
                // slices in WININ
                for (int x = 0; x < X2; x++)     Window[x] = WindowInEnable;
                for (int x = X1; x < width; x++) Window[x] = WindowInEnable;
            }
        }

        private void ResetWindow<T>(ref T[] Window, T Win0In, T Win1In, T OBJWinIn, T WinOut, T Default)
        {
            if (!this.gba.mem.IORAMSection.DISPCNT.DisplayOBJWindow() && 
                !this.gba.mem.IORAMSection.DISPCNT.DisplayBGWindow(0) &&
                !this.gba.mem.IORAMSection.DISPCNT.DisplayBGWindow(1))
            {
                FillWindow<T>(ref Window, Default);
                return;
            }

            FillWindow<T>(ref Window, WinOut);  // clear to WINOUT

            byte X1, X2, Y1, Y2;

            // OBJ layer lowest priority
            if (this.gba.mem.IORAMSection.DISPCNT.DisplayOBJWindow())
            {
                for (int x = 0; x < width; x++)
                {
                    // we draw the mask sprites all to priority 0
                    if (this.OBJWindowMask[x] != 0x8000)
                    {
                        Window[x] = OBJWinIn;
                    }
                }
            }

            for (byte window = 1; window <= 1; window--)  // abuse overflow to loop over window = 1, 0
            {
                if (!this.gba.mem.IORAMSection.DISPCNT.DisplayBGWindow(window))
                {
                    continue;
                }
                
                X1 = this.gba.mem.IORAMSection.WINH[window].LowCoord;
                X2 = this.gba.mem.IORAMSection.WINH[window].HighCoord;
                if (X2 > width) X2 = width;

                Y1 = this.gba.mem.IORAMSection.WINV[window].LowCoord;
                Y2 = this.gba.mem.IORAMSection.WINV[window].HighCoord;
                // if (Y2 > height) Y2 = height;  // GBATek says this, but it seems to create wrong behavior

                this.MaskWindow<T>(ref Window, (window == 0) ? Win0In : Win1In, X1, X2, Y1, Y2);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort Blend(ushort ColorA, ushort ColorB, ushort EVA, ushort EVB)
        {
            ushort Blend = 0;
            
            // colors in BGR555 format
            ushort BGR;

            // Blend red
            BGR = (ushort)(((ColorA & 0x001f) * EVA + (ColorB & 0x001f) * EVB) >> 4);                 // 1.4 fixed point
            Blend |= (ushort)(BGR >= 0x1f ? 0x001f : BGR);

            // Blend green
            BGR = (ushort)((((ColorA & 0x03e0) >> 5) * EVA + ((ColorB & 0x03e0) >> 5) * EVB) >> 4);   // 1.4 fixed point
            Blend |= (ushort)(BGR >= 0x1f ? 0x03e0 : (BGR << 5));

            // Blend blue
            BGR = (ushort)((((ColorA & 0x7c00) >> 10) * EVA + ((ColorB & 0x7c00) >> 10) * EVB) >> 4);  // 1.4 fixed point
            Blend |= (ushort)(BGR >= 0x1f ? 0x7c00 : (BGR << 10));

            return Blend;
        }
        
        private bool SetPixel(int ScreenX, ushort Color, BlendMode AlphaBlending, bool IsTop, bool IsBottom, bool WasOBJ, bool IsOBJ = false)
        {
            // returns if the pixel has final value

            // Sprites behave in a different way with respect to alpha blending
            // I copied the Tonc text into case BlendMode.White
            // If there ever was an object in this pixel, we know that AlphaBlending cannot be off,
            // as otherwise the color was final
            if (WasOBJ && IsBottom) AlphaBlending = BlendMode.Normal;

            // assumes Color != 0x8000
            // assumes that if Blendmode is off, Display[ScreenX] = 0x8000
            switch (AlphaBlending)
            {
                case BlendMode.Off:
                    this.Display[ScreenX] = Color;
                    return true;
                case BlendMode.Normal:
                    if (this.Display[ScreenX] != 0x8000)
                    {
                        if (IsBottom)
                        {
                            this.Display[ScreenX] = Blend(this.Display[ScreenX], Color, this.gba.mem.IORAMSection.BLDALPHA.EVA, this.gba.mem.IORAMSection.BLDALPHA.EVB);
                            return true;
                        }

                        // value is not bottom, but previous value was top
                        return false;
                    }
                    else
                    {
                        this.Display[ScreenX] = Color;
                        return !IsTop;  // if it is not top, this is the final color, otherwise, there is a possibility to blend
                    }
                case BlendMode.White:
                case BlendMode.Black:
                    // Display[ScreenX] should always be 0x8000 in this case, as we always return true (color is always final)
                    if (WasOBJ)
                    {
                        this.Display[ScreenX] = Blend(this.Display[ScreenX], (ushort)(AlphaBlending == BlendMode.White ? 0x7fff : 0),
                            (byte)(0x10 - this.gba.mem.IORAMSection.BLDY.EY), this.gba.mem.IORAMSection.BLDY.EY);
                        return true;
                    }

                    if (IsTop)
                    {
                        // blend with white and color is final
                        // EY <= 0x10 so 0x10 - EY >= 0
                        this.Display[ScreenX] = Blend(Color, (ushort)(AlphaBlending == BlendMode.White ? 0x7fff : 0),
                            (byte)(0x10 - this.gba.mem.IORAMSection.BLDY.EY), this.gba.mem.IORAMSection.BLDY.EY);
                    }
                    else
                    {
                        this.Display[ScreenX] = Color;
                    }

                    // color is always final, except when we are rendering an object
                    // this is because of the strange behavior for sprites in White/Black blending modes
                    /*
                     Sprites are affected differently than backgrounds. In particular,
                     the blend mode specified by REG_BLDCNT{6,7} is applied only to the
                     non-overlapping sections (so that effectively only fading works).
                        For the overlapping pixels, the standard blend is always in effect,
                    regardless of the current blend-mode.
                    (Tonc)
                     */
                    return !IsOBJ;
                default:
                    throw new Exception("Yo this does not exist");
            }
        }

        private void MergeBGs(bool RenderOBJ, params byte[] BGs)  // into current scanline
        {
            // determine what blend mode to use per pixel
            this.ResetWindowBlendMode();

            // default parameters
            ushort Backdrop = this.Backdrop;
            byte[] Priorities = new byte[4];
            bool[] Enabled = new bool[4];

            foreach (byte BG in BGs)
            {
                Priorities[BG] = this.gba.mem.IORAMSection.BGCNT[BG].BGPriority;
                Enabled[BG] = this.gba.mem.IORAMSection.DISPCNT.DisplayBG(BG);
            }

            // blending parameters
            bool[] BGTop = new bool[4], BGBottom = new bool[4];
            foreach (byte BG in BGs)
            {
                BGTop[BG]    = this.gba.mem.IORAMSection.BLDCNT.BGIsTop(BG);
                BGBottom[BG] = this.gba.mem.IORAMSection.BLDCNT.BGIsBottom(BG);
            }

            bool OBJTop, OBJBottom, BDTop, BDBottom;
            OBJTop      = this.gba.mem.IORAMSection.BLDCNT.OBJIsTop();
            OBJBottom   = this.gba.mem.IORAMSection.BLDCNT.OBJIsBottom();
            BDTop       = this.gba.mem.IORAMSection.BLDCNT.BDIsTop();
            BDBottom    = this.gba.mem.IORAMSection.BLDCNT.BDIsBottom();

            // only to be called after drawing into the BGScanlines and OBJLayers
            byte priority;
            bool WasOBJ;  // signify that a nontransparent OBJ pixel was present
            int ScreenX = width * scanline;
            
            for (int x = 0; x < width; x++)
            {
                this.Display[ScreenX] = 0x8000;  // reset 1 pixel at a time to prevent artifacts
                WasOBJ = false;

                for (priority = 0; priority < 4; priority++)
                {
                    if (RenderOBJ && this.OBJLayers[priority][x] != 0x8000)
                    {
                        if (this.SetPixel(ScreenX, this.OBJLayers[priority][x], WindowBlendMode[x], OBJTop, OBJBottom, WasOBJ, IsOBJ: true))
                        {
                            priority = 0xee;    // break out of priority loop, and signify that we have found a non-transparent pixel
                            break;
                        }
                        WasOBJ = true;
                    }

                    foreach (byte BG in BGs)
                    {
                        if (!Enabled[BG])
                            continue;

                        if (Priorities[BG] != priority)
                            continue;

                        if (this.BGScanlines[BG][x] == 0x8000)
                            continue;

                        if (this.SetPixel(ScreenX, this.BGScanlines[BG][x], WindowBlendMode[x], BGTop[BG], BGBottom[BG], WasOBJ))
                        {
                            priority = 0xfe;    // break out of priority loop, and signify that we have found a non-transparent pixel
                            break;
                        }
                    }
                }

                if (priority == 4)  // we found no final non-transparent pixel
                {
                    this.SetPixel(ScreenX, Backdrop, WindowBlendMode[x], BDTop, BDBottom, WasOBJ);
                }

                ScreenX++;
            }
        }

        private void Render4bpp(ref ushort[] Line, bool[] Window, int StartX, int XSign, uint TileLineBaseAddress,
            uint PaletteBase, bool Mosaic, byte MosaicHSize)
        {
            // draw 4bpp tile sliver on screen based on tile base address (corrected for course AND fine y)
            // PaletteBase must be PaletteBank * 0x20
            byte PaletteNibble;
            int ScreenX = StartX;
            uint MosaicCorrectedAddress;
            byte VRAMEntry;
            bool UpperNibble;

            for (int dx = 0; dx < 4; dx++)  // we need to look at nibbles here
            {
                MosaicCorrectedAddress = (uint)(TileLineBaseAddress + dx);
                for (int ddx = 0; ddx < 2; ddx++)
                {
                    if (Mosaic && MosaicHSize != 1)
                    {
                        // todo: fix horizontal mosaic
                        UpperNibble = (((dx << 1) % MosaicHSize) & 1) == 1;
                        MosaicCorrectedAddress -= (MosaicCorrectedAddress % MosaicHSize) >> 1;
                    }
                    else
                    {
                        UpperNibble = ddx == 1;
                    }

                    VRAMEntry = this.gba.mem.VRAM[MosaicCorrectedAddress];

                    if (0 <= ScreenX && ScreenX < width)  // ScreenX is a byte, so always greater than 0
                    {
                        if (Line[ScreenX] == 0x8000)
                        {
                            PaletteNibble = (byte)(UpperNibble ? ((VRAMEntry & 0xf0) >> 4) : (VRAMEntry & 0x0f));
                            if (PaletteNibble > 0 && (Window?[ScreenX] ?? true))  // non-transparent
                                Line[ScreenX] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                        }
                    }
                    ScreenX += XSign;
                }
            }
        }

        private void Render8bpp(ref ushort[] Line, bool[] Window,
            int StartX, int XSign, uint TileLineBaseAddress, bool Mosaic, byte MosaicHSize, ushort PaletteOffset = 0)
        {
            // draw 8bpp tile sliver on screen based on tile base address (corrected for course AND fine y)
            int ScreenX = StartX;
            byte VRAMEntry;
            uint MosaicCorrectedAddress;

            for (int dx = 0; dx < 8; dx++)
            {
                MosaicCorrectedAddress = (uint)(TileLineBaseAddress + dx);
                if (Mosaic)
                    MosaicCorrectedAddress -= (MosaicCorrectedAddress % MosaicHSize);

                if (0 <= ScreenX && ScreenX < width)
                {
                    if (Line[ScreenX] == 0x8000)
                    {
                        VRAMEntry = this.gba.mem.VRAM[MosaicCorrectedAddress];
                        if (VRAMEntry != 0 && (Window?[ScreenX] ?? true))
                            Line[ScreenX] = this.GetPaletteEntry(PaletteOffset + 2 * (uint)VRAMEntry);
                    }
                }

                ScreenX += XSign;
            }
        }

    }
}
