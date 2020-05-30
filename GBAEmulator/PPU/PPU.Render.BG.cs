using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    partial class PPU
    {
        /*
         this actually contains most the logic for rendering, PPU.Render.cs is just everything put together
        */

        private ushort[][] BGScanlines = new ushort[4][] { new ushort[width], new ushort[width], new ushort[width], new ushort[width] };

        private void ResetBGScanlines(params byte[] BGs)
        {
            foreach (byte BG in BGs)
            {
                // reset only the relevant layers
                for (int x = 0; x < width; x++)
                {
                    BGScanlines[BG][x] = 0x8000;
                }
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
                for (priority = 0; priority < 3; priority++)
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

                if (priority == 3)  // we found no non-transparent pixel
                {
                    this.Display[width * scanline + x] = Backdrop;
                }
            }
        }

        private void Render4bpp(ref ushort[] Line, int StartX, int XSign, uint TileLineBaseAddress,
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
                        if (PaletteNibble == 0)  // transparent
                            Line[ScreenX] = 0x8000;
                        else
                            Line[ScreenX] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                    }
                }
                ScreenX += XSign;

                UpperNibble = !Mosaic || MosaicHSize == 1 ||((((dx << 1) + 1) % MosaicHSize) & 1) == 1;

                if (0 <= ScreenX && ScreenX < width)
                {
                    if (Line[ScreenX] == 0x8000)
                    {
                        PaletteNibble = (byte)(UpperNibble ? ((VRAMEntry & 0xf0) >> 4) : (VRAMEntry & 0x0f));
                        if (PaletteNibble == 0)  // transparent
                            Line[ScreenX] = 0x8000;
                        else
                            Line[ScreenX] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                    }
                }
                ScreenX += XSign;
            }
        }

        private void Render8bpp(ref ushort[] Line, int StartX, int XSign, uint TileLineBaseAddress, bool Mosaic, byte MosaicHSize)
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
                    VRAMEntry = this.gba.cpu.VRAM[MosaicCorrectedAddress];
                    if (VRAMEntry != 0)
                        Line[ScreenX] = this.GetPaletteEntry(2 * (uint)VRAMEntry);
                }

                ScreenX +=XSign;
            }
        }

        // ===================================================================================================
        //                                      Regular layers
        // ===================================================================================================

        private static uint VRAMIndexRegular(int TileX, int TileY, byte ScreenblockSize)
        {
            // TileX and TileY are indices of the tile, not of the pixels of the tile
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
                                            byte CharBaseBlock, bool ColorMode, bool Mosaic, byte MosaicHSize)  // based on y = scanline
        {
            byte PaletteBank = (byte)((ScreenEntry & 0xf000) >> 12);
            bool VFlip = (ScreenEntry & 0x0800) > 0, HFlip = (ScreenEntry & 0x0400) > 0;
            ushort TileID = (ushort)(ScreenEntry & 0x03ff);

            ushort Address = (ushort)(CharBaseBlock * 0x4000);

            if (VFlip)
                dy = (byte)(7 - dy);

            // read from right to left if HFlipping
            int XSign = 1;
            if (HFlip)
            {
                StartX += 7;
                XSign = -1;
            }

            if (!ColorMode)  // 4bpp
            {
                Address |= (ushort)(TileID * 0x20);   // Beginning of tile
                Address |= (ushort)(dy * 4);          // Beginning of tile sliver
                
                this.Render4bpp(ref Line, StartX, XSign, Address, (uint)(PaletteBank * 0x20), Mosaic, MosaicHSize);
            }
            else             // 8bpp
            {
                Address |= (ushort)(TileID * 0x40);    // similar to 4bpp
                Address |= (ushort)(dy * 8);

                this.Render8bpp(ref Line, StartX, XSign, Address, Mosaic, MosaicHSize);
            }
        }

        private void RenderRegularBGScanlines(params byte[] BGs)  // based on y = scanline
        {
            ushort HOFS, VOFS;
            byte CharBaseBlock, ScreenBaseBlock, BGSize;
            short EffectiveX, EffectiveY;
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

                EffectiveY = (short)(scanline + VOFS);
                if (Mosaic)
                    EffectiveY -= (short)(EffectiveY % this.gba.cpu.MOSAIC.BGMosaicVSize);

                for (sbyte CourseX = -1; CourseX < 31; CourseX++)
                {
                    EffectiveX = (short)((CourseX << 3) + HOFS);

                    if (Mosaic)
                        EffectiveX -= (short)(EffectiveX % this.gba.cpu.MOSAIC.BGMosaicHSize);

                    // ScreenEntryIndex is the index of the screenentry for the tile we are currently rendering
                    ScreenEntryIndex = PPU.VRAMIndexRegular((int)(EffectiveX >> 3), (int)((EffectiveY) >> 3), BGSize);
                    ScreenEntryIndex += (uint)(ScreenBaseBlock * 0x800);

                    ScreenEntry = (ushort)(this.gba.cpu.VRAM[ScreenEntryIndex + 1] << 8 | this.gba.cpu.VRAM[ScreenEntryIndex]);
                    
                    this.DrawRegularScreenEntry(ref this.BGScanlines[BG], (CourseX * 8) - (HOFS & 0x07), (byte)(EffectiveY & 0x07),
                        ScreenEntry, CharBaseBlock, ColorMode, Mosaic, this.gba.cpu.MOSAIC.BGMosaicHSize);
                }
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
        private void RenderAffineBGScanline(byte BG, ARM7TDMI.cReferencePoint BGxX, ARM7TDMI.cReferencePoint BGxY,
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
            byte Priority = BGCNT.BGPriority;

            bool Mosaic = BGCNT.Mosaic;

            uint ScreenEntryIndex;
            byte AffineScreenEntry;

            int ScreenEntryX, ScreenEntryY;

            for (int ScreenX = 0; ScreenX < width; ScreenX++)
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

                this.BGScanlines[BG][ScreenX] = this.GetAffinePixel(AffineScreenEntry, (byte)ScreenX, CharBaseBlock,
                    (byte)(ScreenEntryX & 7), (byte)(ScreenEntryY & 7));
            }
        }

    }
}
