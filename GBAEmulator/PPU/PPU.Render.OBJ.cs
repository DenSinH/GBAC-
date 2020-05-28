using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    partial class PPU
    {
        ushort[][] OBJLayers = new ushort[4][] { new ushort[width], new ushort[width], new ushort[width], new ushort[width] };

        private void ResetOBJScanlines()
        {
            for (int Priority = 0; Priority < 4; Priority++)
            {
                for (int x = 0; x < width; x++)
                {
                    OBJLayers[Priority][x] = 0x8000;  // transparent
                }
            }
        }

        struct OBJSize
        {
            public readonly byte Width, Height;
            public OBJSize(byte Width, byte Height)
            {
                this.Width = Width;
                this.Height = Height;
            }
        }

        // [Shape][Size], see table on Tonc
        private static readonly OBJSize[][] GetOBJSize = new OBJSize[3][]
        {
            new OBJSize[4] {new OBJSize(8, 8), new OBJSize(16, 16), new OBJSize(32, 32), new OBJSize(64, 64) },
            new OBJSize[4] {new OBJSize(16, 8), new OBJSize(32, 8), new OBJSize(32, 16), new OBJSize(64, 32) },
            new OBJSize[4] {new OBJSize(8, 16), new OBJSize(8, 32), new OBJSize(16, 32), new OBJSize(32, 64) }
        };

        bool OAM2DMap;  // false for 1D mapping, true for 2D

        private void RenderOBJs()  // assumes y = scanline
        {
            this.ResetOBJScanlines();

            ushort OBJ_ATTR0, OBJ_ATTR1, OBJ_ATTR2;
            short OBJy;
            byte OBJMode, GFXMode;
            bool ColorMode, Mosaic;

            OBJSize OBJsz;

            this.OAM2DMap = this.gba.cpu.DISPCNT.IsSet(ARM7TDMI.DISPCNTFlags.OBJVRamMapping);

            for (ushort i = 0; i < 0x400; i += 8)  // 128 objects in OAM
            {
                /* Fetch all data for object */

                OBJ_ATTR0 = (ushort)(this.gba.cpu.OAM[i] | (this.gba.cpu.OAM[i + 1] << 8));

                OBJMode = (byte)((OBJ_ATTR0 & 0x0300) >> 8);

                if (OBJMode == 0b10)
                    continue;  // sprite hidden

                OBJ_ATTR1 = (ushort)(this.gba.cpu.OAM[i + 2] | (this.gba.cpu.OAM[i + 3] << 8));
                OBJ_ATTR2 = (ushort)(this.gba.cpu.OAM[i + 4] | (this.gba.cpu.OAM[i + 5] << 8));

                // when OBJ is off screen, it can also be at the top
                OBJy = (short)(OBJ_ATTR0 & 0x00ff);
                if (OBJy > height)
                    OBJy = (short)(OBJy - 0x100);

                GFXMode = (byte)((OBJ_ATTR0 & 0x0c00) >> 10);
                Mosaic = (OBJ_ATTR0 & 0x1000) > 0;
                ColorMode = (OBJ_ATTR0 & 0x2000) > 0;

                OBJsz = PPU.GetOBJSize[(OBJ_ATTR0 & 0xc000) >> 14][(OBJ_ATTR1 & 0xc000) >> 14];

                /* Draw object */
                if (OBJMode == 0b00)
                {
                    if ((OBJy <= scanline) && (OBJy + OBJsz.Height > scanline))
                        this.RenderRegularOBJ(OBJy, OBJsz, ColorMode, Mosaic, OBJ_ATTR1, OBJ_ATTR2);
                }
                else if (OBJMode == 0b01)
                {
                    if ((OBJy <= scanline) && (OBJy + OBJsz.Height > scanline))
                    {
                        this.RenderAffineOBJ(OBJy, OBJsz, ColorMode, Mosaic, OBJ_ATTR1, OBJ_ATTR2, false);
                    }
                }
                else if (OBJMode == 0b11)
                {
                    if ((OBJy <= scanline) && (OBJy + 2 * OBJsz.Height > scanline))
                    {
                        this.RenderAffineOBJ(OBJy, OBJsz, ColorMode, Mosaic, OBJ_ATTR1, OBJ_ATTR2, true);
                    }
                }
            }
        }

        private void RenderRegularOBJ(short OBJy, OBJSize OBJsz, bool ColorMode, bool Mosaic, ushort OBJ_ATTR1, ushort OBJ_ATTR2)
        {
            ushort StartX = (ushort)(OBJ_ATTR1 & 0x01ff);
            sbyte XSign = 1;
            bool VFlip = (OBJ_ATTR1 & 0x2000) > 0;
            bool HFlip = (OBJ_ATTR1 & 0x1000) > 0;

            ushort TileID = (ushort)(OBJ_ATTR2 & 0x03ff);
            byte Priority = (byte)((OBJ_ATTR2 & 0x0c00) >> 10);
            byte PaletteBank = (byte)((OBJ_ATTR2 & 0xf000) >> 12);

            byte dy = (byte)(scanline - OBJy);   // between 0 and OBJsz.Height (8, 16, 32, 64)
            if (Mosaic)
                dy -= (byte)(dy % this.gba.cpu.MOSAIC.OBJMosaicVSize);

            if (VFlip)
                dy = (byte)(OBJsz.Height - dy);
            
            uint SliverBaseAddress;  // base address for horizontal sprite sliver
            if (HFlip)
            {
                XSign = -1;
                // tiles are also in a different order when we flip horizontally
                StartX += OBJsz.Width;
            }

            if (!ColorMode)  // 4bpp
            {
                SliverBaseAddress = (uint)(TileID * 0x20);
                // removed shifting for less arithmetic, logically OBJsz.Width should be OBJsz.Width >> 3 for the width in tiles, and
                // 4 should be 0x20. This way we wrap around with the number of tiles, but since OBJsz.Width is a power of 2,
                // this is ever so slightly faster, at least I think.
                SliverBaseAddress += (uint)(this.OAM2DMap ? (OBJsz.Width * (dy >> 3) * 4) : (32 * 0x20 * (dy >> 3)));
                SliverBaseAddress += (uint)(4 * (dy & 0x07));   // offset within tile

                // prevent overflow, not sure what is supposed to happen
                if (SliverBaseAddress + (OBJsz.Width >> 3) * 0x20 > 0x8000)  
                    SliverBaseAddress = 0;

                // base address for sprites is 0x10000 in OAM
                SliverBaseAddress = (SliverBaseAddress & 0x7fff) | 0x10000;  

                for (int dTileX = 0; dTileX < (OBJsz.Width >> 3); dTileX++)
                {
                    // foreground palette starts at 0x0500_0200
                    // we can use our same rendering method as for background, as we simply render a tile
                    this.Render4bpp(ref this.OBJLayers[Priority], StartX, XSign,
                        (uint)(SliverBaseAddress + (0x20 * dTileX)), (uint)(0x200 + PaletteBank * 0x20),
                        Mosaic, this.gba.cpu.MOSAIC.OBJMosaicHSize);
                    StartX = (byte)(StartX + 8 * XSign);
                }
            }
            else
            {
                // Tonc about Sprite tile memory offsets: Always per 4bpp tile size: start = base + id * 32
                SliverBaseAddress = (uint)(TileID * 0x20);
                // removed shifting for less arithmetic, like in 4bpp
                SliverBaseAddress += (uint)(this.OAM2DMap ? (OBJsz.Width * (dy >> 3) * 8) : (32 * 0x40 * (dy >> 3)));
                SliverBaseAddress += (uint)(8 * (dy & 0x07));   // offset within tile

                // prevent overflow, not sure what is supposed to happen
                if (SliverBaseAddress + (OBJsz.Width >> 3) * 0x20 > 0x8000)  
                    SliverBaseAddress = 0;

                SliverBaseAddress = (SliverBaseAddress & 0x7fff) | 0x10000;

                for (int dTileX = 0; dTileX < (OBJsz.Width >> 3); dTileX++)
                {
                    // we can use our same rendering method as for background, as we simply render a tile
                    this.Render8bpp(ref this.OBJLayers[Priority], StartX, XSign, (uint)(SliverBaseAddress + (0x40 * dTileX)),
                        Mosaic, this.gba.cpu.MOSAIC.OBJMosaicHSize);
                    StartX = (byte)(StartX + 8 * XSign);
                }
            }
        }

        private ushort GetAffineOBJPixel(uint TileID, OBJSize OBJsz,
            byte px, byte py, bool ColorMode, byte PaletteBank)
        {
            /*
             Though this is really similar to regular objects, we cannot just do it in a straight line, so we have to calculate the address
             over and over. For regular sprites / backgrounds it is faster to just do it all in a row
             */
            uint PixelAddress = 0x10000;   // OBJ vram starts at 0x10000 within VRAM
            if (!ColorMode)     // 4bpp
            {
                PixelAddress += (uint)(TileID * 0x20);
                // removed shifting for less arithmetic, like in regular objects
                PixelAddress += (uint)(this.OAM2DMap ? (OBJsz.Width * (py >> 3) * 4) : (32 * 0x20 * (py >> 3)));
                PixelAddress += (uint)(4 * (py & 0x07));
                PixelAddress += (uint)(0x20 * (px >> 3));

                byte PaletteNibble = this.gba.cpu.VRAM[PixelAddress + ((px & 0x07) >> 1)];
                if ((px & 1) == 1)
                    PaletteNibble >>= 4;

                PaletteNibble &= 0x0f;
                if (PaletteNibble == 0)
                    return 0x8000;

                return this.GetPaletteEntry(0x200 + (uint)PaletteBank * 0x20 + (uint)(2 * PaletteNibble));
            }
            else                // 8bpp
            {
                // Tonc about Sprite tile memory offsets: Always per 4bpp tile size: start = base + id * 32
                PixelAddress += (uint)(TileID * 0x20);
                // removed shifting for less arithmetic:
                PixelAddress += (uint)(this.OAM2DMap ? (OBJsz.Width * (py >> 3) * 8) : (32 * 0x40 * (py >> 3)));
                PixelAddress += (uint)(8 * (py & 0x07));
                PixelAddress += (uint)(0x40 * (px >> 3));

                byte VRAMEntry = this.gba.cpu.VRAM[PixelAddress + (px & 0x07)];
                if (VRAMEntry == 0)
                    return 0x8000;

                return this.GetPaletteEntry(2 * (uint)VRAMEntry);
            }
        }

        private void RenderAffineOBJ(short OBJy, OBJSize OBJsz, bool ColorMode, bool Mosaic,
            ushort OBJ_ATTR1, ushort OBJ_ATTR2, bool DoubleRendering)
        {
            ushort StartX = (ushort)(OBJ_ATTR1 & 0x01ff);
            byte AffineIndex = (byte)((OBJ_ATTR1 & 0x3e00) >> 9);

            ushort TileID = (ushort)(OBJ_ATTR2 & 0x03ff);
            byte Priority = (byte)((OBJ_ATTR2 & 0x0c00) >> 10);
            byte PaletteBank = (byte)((OBJ_ATTR2 & 0xf000) >> 12);

            ushort RotScaleIndex = (ushort)(32 * AffineIndex + 6);

            // PA, PB, PC, PD:
            short[] RotateScaleParams = new short[4];
            for (int di = 0; di < 4; di++)
            {
                RotateScaleParams[di] = (short)(this.gba.cpu.OAM[RotScaleIndex] | (this.gba.cpu.OAM[RotScaleIndex + 1] << 8));
                RotScaleIndex += 8;
            }

            // todo: SIMD?
            uint px, py;
            uint px0 = (uint)(OBJsz.Width >> 1);
            uint py0 = (uint)(OBJsz.Height >> 1);

            short dy;
            if (!DoubleRendering)
                dy = (short)(scanline - OBJy - (OBJsz.Height >> 1));
            else
                dy = (short)(scanline - OBJy - OBJsz.Height);

            short dx = (short)((DoubleRendering ? -OBJsz.Width : -(OBJsz.Width >> 1)) - 1);

            // What the object width is to be interpreted as for looping over x coordinates
            byte FictionalOBJWidth = (byte)(DoubleRendering ? 2 * OBJsz.Width : OBJsz.Width);

            for (int ix = 0; ix < FictionalOBJWidth; ix++)
            {
                // we started at one less than we should have, so that we could simply increment instead of do the calculation again
                dx++;

                if ((StartX + ix < 0) || (StartX + ix) >= width)
                    continue;

                if (this.OBJLayers[Priority][StartX + ix] != 0x8000)
                    continue;

                // transform
                px = (uint)(((RotateScaleParams[0] * dx + RotateScaleParams[1] * dy) >> 8) + px0);
                py = (uint)(((RotateScaleParams[2] * dx + RotateScaleParams[3] * dy) >> 8) + py0);

                if (px >= OBJsz.Width || py >= OBJsz.Height)
                    continue;

                this.OBJLayers[Priority][StartX + ix] = this.GetAffineOBJPixel(TileID, OBJsz, (byte)px, (byte)py, ColorMode, PaletteBank);
            }
        }
    }
}
