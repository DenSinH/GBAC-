using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator
{
    partial class PPU
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillWindow(ref bool[] Window, bool value)
        {
            if (value) for (int x = 0; x < width; x++) Window[x] = true;
            else Window = new bool[width];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MaskWindow(ref bool[] Window, bool WindowInEnable, byte X1, byte X2, byte Y1, byte Y2)
        {
            if (Y1 <= Y2)
            {
                // no vert wrap
                if (scanline < Y1 || scanline > Y2) return;

                if (X1 <= X2)
                {
                    // no hor wrap
                    // slice in WININ
                    for (int x = X1; x < X2; x++) Window[x] = WindowInEnable;
                }
                else
                {
                    // slices in WININ
                    for (int x = 0; x < X2; x++) Window[x] = WindowInEnable;
                    for (int x = X1; x < width; x++) Window[x] = WindowInEnable;
                }
            }
            else
            {
                if ((scanline < Y1) && (scanline > Y2)) return;

                if (X1 <= X2)
                {
                    // no hor wrap
                    // slice in WININ
                    for (int x = X1; x < X2; x++) Window[x] = WindowInEnable;
                }
                else
                {
                    // slices in WININ
                    for (int x = 0; x < X2; x++) Window[x] = WindowInEnable;
                    for (int x = X1; x < width; x++) Window[x] = WindowInEnable;
                }
            }
        }

        private void ResetWindow(ref bool[] Window, bool Win0In, bool Win1In, bool OBJWinIn, bool WinOut)
        {
            if (!this.gba.cpu.DISPCNT.DisplayOBJWindow() && 
                !this.gba.cpu.DISPCNT.DisplayBGWindow(0) &&
                !this.gba.cpu.DISPCNT.DisplayBGWindow(1))
            {
                FillWindow(ref Window, true);
                return;
            }

            FillWindow(ref Window, WinOut);  // clear to WINOUT

            byte X1, X2, Y1, Y2;

            // OBJ layer lowest priority
            if (this.gba.cpu.DISPCNT.DisplayOBJWindow())
            {
                // assume OBJ layer 0 is the mask!
                // has to be filled BEFORE calling this method
                for (int x = 0; x < width; x++)
                {
                    // we draw the mask sprites all to priority 0
                    if (this.OBJMask[x] != 0x8000)
                    {
                        Window[x] = OBJWinIn;
                    }
                }
                // leaves object layer 0 filled!
            }

            for (byte window = 1; window <= 1; window--)
            {
                if (!this.gba.cpu.DISPCNT.DisplayBGWindow(window))
                {
                    continue;
                }

                X1 = this.gba.cpu.WINH[window].LowCoord;
                X2 = this.gba.cpu.WINH[window].HighCoord;
                if (X2 > width) X2 = width;

                Y1 = this.gba.cpu.WINV[window].LowCoord;
                Y2 = this.gba.cpu.WINV[window].HighCoord;
                // if (Y2 > height) Y2 = height;

                this.MaskWindow(ref Window, (window == 0) ? Win0In : Win1In,
                    X1, X2, Y1, Y2);
            }
        }


        private void MergeBGs(bool RenderOBJ, params byte[] BGs)  // into current scanline
        {
            ushort Backdrop = this.Backdrop;
            byte[] Priorities = new byte[4];
            bool[] Enabled = new bool[4];

            foreach (byte BG in BGs)
            {
                Priorities[BG] = this.gba.cpu.BGCNT[BG].BGPriority;
                Enabled[BG] = this.gba.cpu.DISPCNT.DisplayBG(BG);
            }

            // only to be called after drawing into the DrawRegularBGScanline array
            byte priority;
            for (int x = 0; x < width; x++)
            {
                for (priority = 0; priority < 4; priority++)
                {
                    if (RenderOBJ)
                    {
                        if (this.OBJLayers[priority][x] != 0x8000)
                        {
                            this.Display[width * scanline + x] = this.OBJLayers[priority][x];
                            priority = 0xee;    // break out of priority loop, and signify that we have found a non-transparent pixel
                            break;
                        }
                    }

                    foreach (byte BG in BGs)
                    {
                        if (!Enabled[BG])
                            continue;

                        if (Priorities[BG] != priority)
                            continue;

                        if (this.BGScanlines[BG][x] != 0x8000)
                        {
                            this.Display[width * scanline + x] = this.BGScanlines[BG][x];
                            priority = 0xfe;    // break out of priority loop, and signify that we have found a non-transparent pixel
                            break;
                        }
                    }
                }

                if (priority == 4)  // we found no non-transparent pixel
                {
                    this.Display[width * scanline + x] = Backdrop;
                }
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
                if (Mosaic && MosaicHSize != 1)
                {
                    // todo: fix horizontal mosaic
                    UpperNibble = (((dx << 1) % MosaicHSize) & 1) == 1;
                    MosaicCorrectedAddress -= (MosaicCorrectedAddress % MosaicHSize) >> 1;
                }
                else
                {
                    UpperNibble = false;
                }

                VRAMEntry = this.gba.cpu.VRAM[MosaicCorrectedAddress];

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

                UpperNibble = !Mosaic || MosaicHSize == 1 || ((((dx << 1) + 1) % MosaicHSize) & 1) == 1;

                if (0 <= ScreenX && ScreenX < width)
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

                if (0 <= ScreenX && ScreenX < width)  // ScreenX is a byte, so always >= 0
                {
                    if (Line[ScreenX] == 0x8000)
                    {
                        VRAMEntry = this.gba.cpu.VRAM[MosaicCorrectedAddress];
                        if (VRAMEntry != 0 && (Window?[ScreenX] ?? true))
                            Line[ScreenX] = this.GetPaletteEntry(PaletteOffset + 2 * (uint)VRAMEntry);
                    }
                }

                ScreenX += XSign;
            }
        }

    }
}
