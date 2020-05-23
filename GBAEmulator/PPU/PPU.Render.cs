using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    partial class PPU
    {
        public byte scanline = 0;
        public ulong frame = 0;
        
        public bool IsVBlank
        {
            get { return (scanline >= 160) && (scanline < 227); }
        }

        private ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                this.gba.cpu.PaletteRAM[Address] |
                (this.gba.cpu.PaletteRAM[Address + 1] << 8)
                );
        }

        private ushort Backdrop
        {
            get => (ushort)(this.gba.cpu.PaletteRAM[0] | (this.gba.cpu.PaletteRAM[1] << 8));
        }

        public void DrawScanline()
        {
            /*
            GBATek:
              Mode  Rot/Scal Layers Size               Tiles Colors       Features
              0     No       0123   256x256..512x515   1024  16/16..256/1 SFMABP
              1     Mixed    012-   (BG0,BG1 as above Mode 0, BG2 as below Mode 2)
              2     Yes      --23   128x128..1024x1024 256   256/1        S-MABP
              3     Yes      --2-   240x160            1     32768        --MABP
              4     Yes      --2-   240x160            2     256/1        --MABP
              5     Yes      --2-   160x128            2     32768        --MABP
             */
            if (!IsVBlank)
            {
                switch (this.gba.cpu.DISPCNT.BGMode)
                {
                    case 0:
                        this.Mode0Scanline();
                        break;
                    case 3:
                        this.Mode3Scanline();
                        break;
                    case 4:
                        this.Mode4Scanline();
                        break;
                    case 5:
                        this.Mode5Scanline();
                        break;
#if DEBUG
                    default:
                        Console.Error.WriteLine(string.Format("BG Mode {0} not implemented yet", this.gba.cpu.DISPCNT.BGMode));
                        break;
#else
                    default:
                        if (scanline == 0)
                            Console.WriteLine(string.Format("BG Mode {0} not implemented yet", this.gba.cpu.DISPCNT.BGMode));
                        this.Mode4Scanline();
                        break;
#endif
                }
            }
            
            scanline++;
            if (scanline == 227)
            {
                scanline = 0;
                frame++;
            }
        }

        private void Mode3Scanline()
        {
            // we render on BG2
            if (this.gba.cpu.DISPCNT.DisplayBG(2))
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[240 * scanline + x] = (ushort)((this.gba.cpu.VRAM[480 * scanline + 2 * x + 1] << 8) |
                                                                 this.gba.cpu.VRAM[480 * scanline + 2 * x]);
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[240 * scanline + x] = 0;
                }
            }
        }
        
        private void Mode4Scanline()
        {
            ushort offset = (ushort)(this.gba.cpu.DISPCNT.IsSet(ARM7TDMI.DISPCNTFlags.DPFrameSelect) ? 0xa000 : 0);

            // we render on BG2
            if (this.gba.cpu.DISPCNT.DisplayBG(2))
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[240 * scanline + x] = this.GetPaletteEntry((uint)this.gba.cpu.VRAM[offset + 240 * scanline + x] << 1);
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[240 * scanline + x] = 0;
                }
            }
        }

        private void Mode5Scanline()
        {
            // I don't think this is working properly
            if (scanline < 128 && this.gba.cpu.DISPCNT.DisplayBG(2))
            {
                ushort offset = (ushort)(this.gba.cpu.DISPCNT.IsSet(ARM7TDMI.DISPCNTFlags.DPFrameSelect) ? 0xa000 : 0);

                // smaller format
                for (int x = 0; x < 160; x++)
                {
                    this.Display[240 * scanline + x] = this.GetPaletteEntry((uint)this.gba.cpu.VRAM[offset + width * scanline + x] << 1);
                }

                for (int x = 160; x < width; x++)
                {
                    this.Display[240 * scanline + x] = 0;
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[240 * scanline + x] = 0;
                }
            }
        }
        
        private static uint VRAMIndexRegular(int TileX, int TileY, byte ScreenblockSize)
        {
            switch(ScreenblockSize)
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

        ushort[][] BGScanlines = new ushort[4][] { new ushort[width], new ushort[width], new ushort[width], new ushort[width] };

        private void ResetGBScanlines(params byte[] BGs)
        {
            foreach (byte BG in BGs)
            {
                for (byte x = 0; x < width; x++)
                {
                    // nice and hardcoded
                    BGScanlines[BG][x] = 0;
                }
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
                    ScreenX = (byte)(StartX + 2 * dx);
                    if (0 <= ScreenX && ScreenX < width)
                    {
                        PaletteNibble = (byte)(this.gba.cpu.VRAM[Address + dx] & 0x0f);
                        Line[ScreenX] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                        if (PaletteNibble == 0)  // transparent
                            Line[ScreenX] |= 0x8000;
                    }

                    if (0 <= ScreenX && ScreenX < width - 1)
                    {
                        PaletteNibble = (byte)((this.gba.cpu.VRAM[Address + dx] & 0xf0) >> 4);
                        Line[ScreenX + 1] = this.GetPaletteEntry(PaletteBase + (uint)(2 * PaletteNibble));
                        if (PaletteNibble == 0)  // transparent
                            Line[ScreenX + 1] |= 0x8000;
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

        public void MergeBGsRegular(params byte[] BGs)  // into current scanline
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

        private void Mode0Scanline()
        {
            this.ResetGBScanlines(0, 1, 2, 3);
            this.DrawRegularBGScanline(0, 1, 2, 3);
            this.MergeBGsRegular(0, 1, 2, 3);
        }
    }
}
