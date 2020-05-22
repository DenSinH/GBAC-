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

        public ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                this.gba.cpu.PaletteRAM[Address] |
                (this.gba.cpu.PaletteRAM[Address + 1] << 8)
                );
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
            if (this.gba.cpu.DISPCNT.IsSet(ARM7TDMI.DISPCNTFlags.DisplayBG2))
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
            if (this.gba.cpu.DISPCNT.IsSet(ARM7TDMI.DISPCNTFlags.DisplayBG2))
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
            if (scanline < 128 && this.gba.cpu.DISPCNT.IsSet(ARM7TDMI.DISPCNTFlags.DisplayBG2))
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
        
        private struct Size
        {
            public byte Width, Height;
            public Size(byte Width, byte Height)
            {
                this.Width = Width;
                this.Height = Height;
            }
        }

        // size in tiles
        private static readonly Size[] BGSize = new Size[4] { new Size(32, 32), new Size(64, 32), new Size(32, 64), new Size(64, 64) };

        private static uint VRAMIndexRegular(byte TileX, byte TileY, Size ScreenblockSize)
        {
            bool SBdx = TileX > 32;
            bool SBdy = TileY > 32;

            return (uint)((SBdy ? 0x800 : 0) | (SBdx ? 0x400 : 0) | ((TileY & 0x0f) << 5) | (TileX & 0x0f));
        }

        ushort[][] BGScanlines = new ushort[4][] { new ushort[width], new ushort[width], new ushort[width], new ushort[width] };

        private void DrawRegularScreenEntry(ushort[] Line, uint StartX, ushort ScreenEntry, 
                                            byte CharBaseBlock, bool ColorMode)  // based on y = scanline
        {
            byte PaletteBank = (byte)((ScreenEntry & 0xf000) >> 12);
            bool VFlip, HFlip;
            ushort TileID = (ushort)(ScreenEntry & 0x03ff);

            uint Address = (uint)(CharBaseBlock * 0x4000);
            byte dy = (byte)(scanline & 0x7);  // mod 8

            if (!ColorMode)  // 4bpp
            {
                Address |= (uint)(TileID * 0x20);   // Beginning of tile
                Address |= (uint)(dy * 8);          // Beginning of tile line
                uint PaletteBase = (uint)(PaletteBank * 0x20);

                byte PaletteNibble;
                for (byte dx = 0; dx < 4; dx++)  // we need to look at nibbles here
                {
                    PaletteNibble = (byte)(this.gba.cpu.VRAM[Address + dx] & 0x0f);
                    Line[StartX + 2 * dx] = this.GetPaletteEntry(PaletteBase + (uint)2 * PaletteNibble);
                    if (PaletteNibble == 0)  // transparent
                        Line[StartX + 2 * dx] |= 0x8000;

                    PaletteNibble = (byte)((this.gba.cpu.VRAM[Address + dx] & 0xf0) >> 4);
                    Line[StartX + 2 * dx + 1] = this.GetPaletteEntry(PaletteBase + (uint)2 * PaletteNibble);
                    if (PaletteNibble == 0)  // transparent
                        Line[StartX + 2 * dx] |= 0x8000;
                }
            }
            else             // 8bpp
            {
                Address |= (uint)(TileID * 0x40);
                Address |= (uint)(dy * 4);

                for (byte dx = 0; dx < 8; dx++)
                {
                    Line[StartX + dx] = this.GetPaletteEntry((uint)2 * this.gba.cpu.VRAM[Address + dx]);
                }
            }
        }

        private void DrawRegularBGScanline(params byte[] BGs)  // based on y = scanline
        {
            uint HOFS, VOFS;
            byte Priority, CharBaseBlock, ScreenBaseBlock;
            bool ColorMode, Mosaic;
            Size BGSize;

            uint ScreenEntryIndex;
            ushort ScreenEntry;

            ARM7TDMI.cBGControl BGCNT;
            foreach (byte BG in BGs)
            {
                HOFS = this.gba.cpu.BGHOFS[BG].Offset;
                VOFS = this.gba.cpu.BGVOFS[BG].Offset;
                BGCNT = this.gba.cpu.BGCNT[BG];

                Priority = BGCNT.BGPriority;
                CharBaseBlock = BGCNT.CharBaseBlock;
                ScreenBaseBlock = BGCNT.ScreenBaseBlock;
                ColorMode = BGCNT.ColorMode;
                Mosaic = BGCNT.Mosaic;

                BGSize = PPU.BGSize[BG];

                // todo: Fine x
                for (byte CourseX = 0; CourseX < 30; CourseX++)
                {
                    ScreenEntryIndex = PPU.VRAMIndexRegular((byte)(CourseX + (HOFS >> 3)), (byte)((scanline + VOFS) >> 3), BGSize);
                    ScreenEntryIndex |= (uint)(ScreenBaseBlock * 0x800);
                    ScreenEntry = this.gba.cpu.VRAM[ScreenEntryIndex];
                    
                    this.DrawRegularScreenEntry(this.BGScanlines[Priority], (uint)(CourseX * 8), ScreenEntry, CharBaseBlock, ColorMode);
                }
            }
        }

        public void MergeBGs()  // into current scanline
        {
            // only to be called after drawing into the DrawRegularBGScanline array
            for (byte x = 0; x < width; x++)
            {
                for (byte priority = 0; priority < 3; priority++)
                {
                    if ((this.BGScanlines[priority][x] & 0x8000) == 0)  // we set this to 1 if it is to be transparent
                    {
                        this.Display[240 * scanline + x] = this.BGScanlines[priority][x];
                        break;
                    }
                }
            }
        }

        private void Mode0Scanline()
        {
            this.DrawRegularBGScanline(0, 1, 2, 3);
            this.MergeBGs();
        }
    }
}
