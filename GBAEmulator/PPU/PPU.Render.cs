using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    partial class PPU
    {
        public byte scanline = 0;
        public ulong frame = 0;

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
                    case 1:
                        this.Mode1Scanline();
                        break;
                    case 2:
                        this.Mode2Scanline();
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
                    default:
                        throw new Exception("Invalid mode");
                }
            }
            
            scanline++;
            if (scanline == 227)
            {
                scanline = 0;
                frame++;
            }
        }

        private void Mode0Scanline()
        {
            this.ResetGBScanlines(0, 1, 2, 3);
            this.DrawRegularBGScanline(0, 1, 2, 3);
            this.MergeBGs(0, 1, 2, 3);
        }

        private void Mode1Scanline()
        {
            this.ResetGBScanlines(0, 1, 2);
            this.DrawRegularBGScanline(0, 1);
            this.DrawAffineBGScanline(2, this.gba.cpu.BG2X, this.gba.cpu.BG2Y,
                this.gba.cpu.BG2PA, this.gba.cpu.BG2PB, this.gba.cpu.BG2PC, this.gba.cpu.BG2PD);
            this.MergeBGs(0, 1, 2);
        }

        private void Mode2Scanline()
        {
            this.ResetGBScanlines(2, 3);
            this.DrawAffineBGScanline(2, this.gba.cpu.BG2X, this.gba.cpu.BG2Y,
                this.gba.cpu.BG2PA, this.gba.cpu.BG2PB, this.gba.cpu.BG2PC, this.gba.cpu.BG2PD);
            this.DrawAffineBGScanline(3, this.gba.cpu.BG3X, this.gba.cpu.BG3Y,
                this.gba.cpu.BG3PA, this.gba.cpu.BG3PB, this.gba.cpu.BG3PC, this.gba.cpu.BG3PD);
            this.MergeBGs(2, 3);
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
    }
}
