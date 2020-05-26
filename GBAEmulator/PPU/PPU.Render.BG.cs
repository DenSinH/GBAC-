using System;
using System.Numerics;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    partial class PPU
    {
        /*
         this actually contains most the logic for rendering, PPU.Render.cs is just everything put together
        */

        ushort[][] BGScanlines = new ushort[4][] { new ushort[width], new ushort[width], new ushort[width], new ushort[width] };

        private void ResetGBScanlines(params byte[] BGs)
        {
            foreach (byte BG in BGs)
            {
                for (byte x = 0; x < width; x++)
                {
                    BGScanlines[BG][x] = 0;
                }
            }
        }

        // ===================================================================================================
        //                                      Regular layers
        // ===================================================================================================

        private static uint VRAMIndexRegular(int TileX, int TileY, byte ScreenblockSize)
        {
            switch (ScreenblockSize)
            {
                case 0b00:  // 32x32
                    return (uint)(((TileY & 0x1f) << 6) | ((TileX & 0x1f) << 1));
                case 0b01:  // 64x32
                    return (uint)(((TileX & 0x3f) > 31 ? 0x800 : 0) | ((TileY & 0x1f) << 6) | ((TileX & 0x1f) << 1));
                case 0b10:  // 32x64
                    return (uint)(((TileY & 0x3f) > 31 ? 0x800 : 0) | ((TileY & 0x1f) << 6) | ((TileX & 0x1f) << 1));
                case 0b11:  // 64x64
                    return (uint)(((TileY & 0x3f) > 31 ? 0x1000 : 0) | ((TileX & 0x3f) > 31 ? 0x800 : 0) | ((TileY & 0x1f) << 6) | ((TileX & 0x1f) << 1));
                default:
                    throw new Exception("Yo this is not supposed to happen");
            }
        }

        private void DrawRegularScreenEntry(ref ushort[] Line, int StartX, byte dy, ushort ScreenEntry,
                                            byte CharBaseBlock, bool ColorMode)  // based on y = scanline
        {
            byte PaletteBank = (byte)((ScreenEntry & 0xf000) >> 12);
            bool VFlip = (ScreenEntry & 0x0800) > 0, HFlip = (ScreenEntry & 0x0400) > 0;
            ushort TileID = (ushort)(ScreenEntry & 0x03ff);

            ushort Address = (ushort)(CharBaseBlock * 0x4000);

            if (VFlip)
                dy = (byte)(7 - dy);

            // read from right to left if HFlipping
            sbyte s = 1;
            if (HFlip)
            {
                StartX += 7;
                s = -1;
            }

            byte ScreenX;

            if (!ColorMode)  // 4bpp
            {
                Address |= (ushort)(TileID * 0x20);   // Beginning of tile
                Address |= (ushort)(dy * 4);          // Beginning of tile line

                uint PaletteBase = (uint)(PaletteBank * 0x20);

                byte PaletteNibble;
                for (byte dx = 0; dx < 4; dx++)  // we need to look at nibbles here
                {
                    ScreenX = (byte)(StartX + 2 * s * dx);
                    if (0 <= ScreenX && ScreenX < width)
                    {
                        PaletteNibble = (byte)(this.gba.cpu.VRAM[Address + dx] & 0x0f);
                        if (PaletteNibble == 0)  // transparent
                            Line[ScreenX] = 0x8000;
                        else
                            Line[ScreenX] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                        
                    }

                    if (0 <= ScreenX + s && ScreenX < width - s)
                    {
                        PaletteNibble = (byte)((this.gba.cpu.VRAM[Address + dx] & 0xf0) >> 4);
                        if (PaletteNibble == 0)  // transparent
                            Line[ScreenX + s] = 0x8000;
                        else
                            Line[ScreenX + s] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                        
                    }
                }

                // vertical gridlines:
                // if (0 <= StartX && StartX < width)
                //     Line[StartX] = 0x7fff;
            }
            else             // 8bpp
            {
                Address |= (ushort)(TileID * 0x40);
                Address |= (ushort)(dy * 8);

                for (byte dx = 0; dx < 8; dx++)
                {
                    ScreenX = (byte)(StartX + s * dx);
                    if (0 <= ScreenX && ScreenX < width)
                        Line[ScreenX] = this.GetPaletteEntry((uint)2 * this.gba.cpu.VRAM[Address + dx]);
                }
            }
        }

        private void DrawRegularBGScanline(params byte[] BGs)  // based on y = scanline
        {
            ushort HOFS, VOFS;
            byte CharBaseBlock, ScreenBaseBlock, BGSize;
            bool ColorMode, Mosaic;

            uint ScreenEntryIndex;
            ushort ScreenEntry;

            ARM7TDMI.cBGControl BGCNT;
            foreach (byte BG in BGs)
            {
                if (!this.gba.cpu.DISPCNT.DisplayBG(BG))
                    // Background disabled, does not need rendering
                    continue;

                HOFS = this.gba.cpu.BGHOFS[BG].Offset;
                VOFS = this.gba.cpu.BGVOFS[BG].Offset;
                BGCNT = this.gba.cpu.BGCNT[BG];

                CharBaseBlock = BGCNT.CharBaseBlock;
                ScreenBaseBlock = BGCNT.ScreenBaseBlock;
                ColorMode = BGCNT.ColorMode;
                Mosaic = BGCNT.Mosaic;

                BGSize = BGCNT.ScreenSize;

                for (sbyte CourseX = -1; CourseX < 31; CourseX++)
                {
                    // ScreenEntryIndex is the index of the screenentry for the tile we are currently rendering
                    ScreenEntryIndex = PPU.VRAMIndexRegular((int)(CourseX + (HOFS >> 3)), (int)((scanline + VOFS) >> 3), BGSize);
                    ScreenEntryIndex += (uint)(ScreenBaseBlock * 0x800);

                    ScreenEntry = (ushort)(this.gba.cpu.VRAM[ScreenEntryIndex + 1] << 8 | this.gba.cpu.VRAM[ScreenEntryIndex]);

                    this.DrawRegularScreenEntry(ref this.BGScanlines[BG], (CourseX * 8) - (HOFS & 0x07), (byte)((scanline + VOFS) & 0x07),
                        ScreenEntry, CharBaseBlock, ColorMode);
                }
            }
        }

        public void MergeBGs(params byte[] BGs)  // into current scanline
        {
            ushort Backdrop = this.Backdrop;

            bool[] enabled = new bool[4];
            byte[] priorities = new byte[4];

            foreach (byte BG in BGs)
            {
                enabled[BG] = this.gba.cpu.DISPCNT.DisplayBG(BG);
                priorities[BG] = this.gba.cpu.BGCNT[BG].BGPriority;
            }

            // only to be called after drawing into the DrawRegularBGScanline array
            byte priority;
            for (byte x = 0; x < width; x++)
            {
                for (priority = 0; priority < 3; priority++)
                {
                    foreach (byte BG in BGs)
                    {
                        if (!enabled[BG])
                            continue;

                        if (priorities[BG] != priority)
                            continue;

                        if ((this.BGScanlines[BG][x] & 0x8000) == 0)  // we set this to 1 if it is to be transparent
                        {
                            this.Display[240 * scanline + x] = this.BGScanlines[BG][x];
                            priority = 0xfe;    // break out of priority loop as well
                                                // this way priority is 0xff after the loop if a pixel was drawn
                            break;
                        }
                    }
                }

                if (priority == 3)
                    this.Display[240 * scanline + x] = Backdrop;
            }
        }

        // ===================================================================================================
        //                                      Affine layers
        // ===================================================================================================

        private static readonly ushort[] AffineSizes = new ushort[4] { 128, 256, 512, 1024 };

        private ushort GetAffinePixel(byte TileID, uint ScreenX, byte CharBaseBlock, byte dx, byte dy)
        {
            // TileID is equivalent with ScreenEntry in affine backgrounds
            // we only use 8bpp mode for affine layers (Tonc)
            ushort Address = (ushort)(CharBaseBlock * 0x4000 | (TileID * 0x40) | (dy * 8) | dx);

            return this.GetPaletteEntry((uint)2 * this.gba.cpu.VRAM[Address]);
        }

        // based on y = scanline
        private void DrawAffineBGScanline(byte BG, ARM7TDMI.cReferencePoint BGxX, ARM7TDMI.cReferencePoint BGxY,
            ARM7TDMI.cRotationScaling PA, ARM7TDMI.cRotationScaling PB, ARM7TDMI.cRotationScaling PC, ARM7TDMI.cRotationScaling PD)
        {
            // ! only to be used with BG = 2 or BG = 3
            if (!this.gba.cpu.DISPCNT.DisplayBG(BG))
                // Background disabled, does not need rendering
                return;
            
            ARM7TDMI.cBGControl BGCNT = this.gba.cpu.BGCNT[BG];
            
            ushort BGSize = PPU.AffineSizes[BGCNT.ScreenSize];

            byte CharBaseBlock = BGCNT.CharBaseBlock;
            uint ScreenEntryBaseAddress = (uint)BGCNT.ScreenBaseBlock * 0x800;

            bool Mosaic;

            uint ScreenEntryIndex;
            byte AffineScreenEntry;
            
            Mosaic = BGCNT.Mosaic;

            int ScreenEntryX, ScreenEntryY;
            for (byte ScreenX = 0; ScreenX < width; ScreenX++)
            {
                ScreenEntryX = (((int)BGxX.InternalRegister + PA.Full * ScreenX) >> 8);  // >> 8 because it is fractional
                ScreenEntryY = (((int)BGxY.InternalRegister + PC.Full * ScreenX) >> 8);  // >> 8 because it is fractional

                if (ScreenEntryX < 0 || ScreenEntryX >= BGSize || ScreenEntryY < 0 || ScreenEntryY >= BGSize)
                {
                    if (!BGCNT.DisplayAreaOverflow)
                    {
                        this.BGScanlines[BG][ScreenX] = 0x8000;  // transparent
                        continue;
                    }
                    
                    // wraparound: modulo BGSize (power of 2)
                    ScreenEntryX &= (BGSize - 1);
                    ScreenEntryY &= (BGSize - 1);
                }

                // similar to regular now
                ScreenEntryIndex = (ScreenEntryBaseAddress | (uint)((ScreenEntryY >> 3) * (BGSize >> 3))| (uint)(ScreenEntryX >> 3));

                // Console.Write(ScreenEntryIndex.ToString("x3") + " ");
                AffineScreenEntry = this.gba.cpu.VRAM[ScreenEntryIndex];

                this.BGScanlines[BG][ScreenX] = this.GetAffinePixel(AffineScreenEntry, ScreenX, CharBaseBlock,
                    (byte)(ScreenEntryX & 7), (byte)(ScreenEntryY & 7));
            }
        }

    }
}
