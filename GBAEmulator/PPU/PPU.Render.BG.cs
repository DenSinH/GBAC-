using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using GBAEmulator.CPU;
using GBAEmulator.Memory.IO;

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

        // bool[BG][x]
        private bool[][] BGWindows = new bool[4][]
        {
             new bool[width], new bool[width], new bool[width], new bool[width]
        };

        
        private void ResetBGWindows(params byte[] BGs)
        {
            foreach (byte BG in BGs)
            {
                this.ResetWindow<bool>(ref BGWindows[BG],
                    this.IO.WININ.WindowBGEnable(Window.Window0, BG), this.IO.WININ.WindowBGEnable(Window.Window1, BG),
                    this.IO.WINOUT.WindowBGEnable(Window.OBJ, BG), this.IO.WINOUT.WindowBGEnable(Window.Outside, BG), true);
            }
        }

        // ===================================================================================================
        //                                      Regular layers
        // ===================================================================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private void DrawRegularScreenEntry(ref ushort[] Line, ref bool[] Window, int StartX, byte dy, ushort ScreenEntry,
                                            byte CharBaseBlock, bool ColorMode, bool Mosaic, byte MosaicHSize)  // based on y = scanline
        {
            byte PaletteBank = (byte)((ScreenEntry & 0xf000) >> 12);
            bool VFlip = (ScreenEntry & 0x0800) > 0, HFlip = (ScreenEntry & 0x0400) > 0;
            ushort TileID = (ushort)(ScreenEntry & 0x03ff);

            uint Address = (uint)(CharBaseBlock * 0x4000);

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
                Address += (uint)(TileID * 0x20);   // Beginning of tile
                Address += (uint)(dy * 4);          // Beginning of tile sliver

                this.Render4bpp(ref Line, Window, StartX, XSign, Address, (uint)(PaletteBank * 0x20), Mosaic, MosaicHSize);
            }
            else             // 8bpp
            {
                Address += (uint)(TileID * 0x40);    // similar to 4bpp
                Address += (uint)(dy * 8);

                this.Render8bpp(ref Line, Window, StartX, XSign, Address, Mosaic, MosaicHSize);
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

            cBGControl BGCNT;

            foreach (byte BG in BGs)
            {
                if (!this.IO.DISPCNT.DisplayBG(BG))
                    // Background disabled, does not need rendering
                    continue;

                HOFS  = this.IO.BGHOFS[BG].Offset;
                VOFS  = this.IO.BGVOFS[BG].Offset;
                BGCNT = this.IO.BGCNT[BG];

                CharBaseBlock   = BGCNT.CharBaseBlock;
                ScreenBaseBlock = BGCNT.ScreenBaseBlock;
                ColorMode       = BGCNT.ColorMode;
                Mosaic          = BGCNT.Mosaic;
                BGSize          = BGCNT.ScreenSize;

                // correct address for mosaic
                EffectiveY = (short)(scanline + VOFS);
                if (Mosaic)
                    EffectiveY -= (short)(EffectiveY % this.IO.MOSAIC.BGMosaicVSize);

                for (sbyte CourseX = -1; CourseX < 31; CourseX++)
                {
                    EffectiveX = (short)((CourseX << 3) + HOFS);

                    if (Mosaic)
                        EffectiveX -= (short)(EffectiveX % this.IO.MOSAIC.BGMosaicHSize);

                    // ScreenEntryIndex is the index of the screenentry for the tile we are currently rendering
                    ScreenEntryIndex = PPU.VRAMIndexRegular((int)(EffectiveX >> 3), (int)((EffectiveY) >> 3), BGSize);
                    ScreenEntryIndex += (uint)(ScreenBaseBlock * 0x800);

                    ScreenEntry = (ushort)(this.gba.mem.VRAM[ScreenEntryIndex + 1] << 8 | this.gba.mem.VRAM[ScreenEntryIndex]);
                    
                    this.DrawRegularScreenEntry(
                        ref this.BGScanlines[BG],
                        ref this.BGWindows[BG],
                        StartX: (CourseX * 8) - (HOFS & 0x07),
                        dy: (byte)(EffectiveY & 0x07),
                        ScreenEntry: ScreenEntry,
                        CharBaseBlock: CharBaseBlock,
                        ColorMode: ColorMode,
                        Mosaic: Mosaic,
                        MosaicHSize: this.IO.MOSAIC.BGMosaicHSize
                        );
                }
            }
        }

        // ===================================================================================================
        //                                      Affine layers
        // ===================================================================================================

        private static readonly ushort[] AffineSizeTable = new ushort[4] { 128, 256, 512, 1024 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetAffinePixel(byte TileID, uint ScreenX, byte CharBaseBlock, byte dx, byte dy)
        {
            // TileID is equivalent with ScreenEntry in affine backgrounds
            // we only use 8bpp mode for affine layers (Tonc)
            
            ushort Address = (ushort)(CharBaseBlock * 0x4000 | (TileID * 0x40) | (dy * 8) | dx);
            byte VRAMEntry = this.gba.mem.VRAM[Address];

            if (VRAMEntry == 0) return 0x8000;  // transparent

            return this.GetPaletteEntry((uint)2 * VRAMEntry);
        }

        // based on y = scanline
        private void RenderAffineBGScanline(byte BG, cReferencePoint BGxX, cReferencePoint BGxY,
                            cRotationScaling PA, cRotationScaling PB, cRotationScaling PC, cRotationScaling PD)
        {
            // ! only to be used with BG = 2 or BG = 3 !
            if (!this.IO.DISPCNT.DisplayBG(BG))  // Background disabled, does not need rendering
                return;

            cBGControl BGCNT = this.IO.BGCNT[BG];
            
            ushort BGSize = AffineSizeTable[BGCNT.ScreenSize];

            byte CharBaseBlock = BGCNT.CharBaseBlock;
            uint ScreenEntryBaseAddress = (uint)BGCNT.ScreenBaseBlock * 0x800;
            byte Priority      = BGCNT.BGPriority;
            bool Mosaic        = BGCNT.Mosaic;

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
                
                AffineScreenEntry = this.gba.mem.VRAM[ScreenEntryIndex];

                if (this.BGWindows[BG][ScreenX])
                {
                    this.BGScanlines[BG][ScreenX] = this.GetAffinePixel(AffineScreenEntry, (byte)ScreenX, CharBaseBlock,
                        (byte)(ScreenEntryX & 7), (byte)(ScreenEntryY & 7));
                }
            }
        }

    }
}
