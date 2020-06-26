using System;
using System.Threading;

using GBAEmulator.CPU;

namespace GBAEmulator.Video
{
    partial class PPU
    {
        public byte scanline = 0;
        public volatile uint frame = 0;  // reset in different thread as well

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
                // Console.WriteLine(this.gba.mem.DISPCNT.BGMode);
                switch (this.IO.DISPCNT.BGMode)
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
                        throw new Exception("Invalid Rendering Mode");
                }
            }
        }

        private void Mode0Scanline()
        {
            bool DoRenderOBJs = this.IO.DISPCNT.IsSet(DISPCNTFlags.DisplayOBJ) && ExternalOBJEnable;
            
            this.ResetBGScanlines(0, 1, 2, 3);
            this.ResetBGWindows(0, 1, 2, 3);
            this.ResetOBJWindow();

            if (DoRenderOBJs)
            {
                this.RenderOBJs();
            }

            this.RenderRegularBGScanlines(0, 1, 2, 3);
            this.MergeBGs(DoRenderOBJs, 0, 1, 2, 3);
        }

        private void Mode1Scanline()
        {
            bool DoRenderOBJs = this.IO.DISPCNT.IsSet(DISPCNTFlags.DisplayOBJ) && ExternalOBJEnable;
            
            this.ResetBGScanlines(0, 1, 2);
            this.ResetBGWindows(0, 1, 2);
            this.ResetOBJWindow();

            if (DoRenderOBJs)
            {
                this.RenderOBJs();
            }

            this.RenderRegularBGScanlines(0, 1);
            this.RenderAffineBGScanline(2, this.IO.BG2X, this.IO.BG2Y,
                this.IO.BG2PA, this.IO.BG2PB, this.IO.BG2PC, this.IO.BG2PD);
            this.MergeBGs(DoRenderOBJs, 0, 1, 2);
        }

        private void Mode2Scanline()
        {
            bool DoRenderOBJs = this.IO.DISPCNT.IsSet(DISPCNTFlags.DisplayOBJ) && ExternalOBJEnable;
            
            this.ResetBGScanlines(2, 3);
            this.ResetBGWindows(2, 3);
            this.ResetOBJWindow();

            if (DoRenderOBJs)
            {
                this.RenderOBJs();
            }

            this.RenderAffineBGScanline(2, this.IO.BG2X, this.IO.BG2Y,
                this.IO.BG2PA, this.IO.BG2PB, this.IO.BG2PC, this.IO.BG2PD);
            this.RenderAffineBGScanline(3, this.IO.BG3X, this.IO.BG3Y,
                this.IO.BG3PA, this.IO.BG3PB, this.IO.BG3PC, this.IO.BG3PD);
            this.MergeBGs(DoRenderOBJs, 2, 3);
        }

        private void Mode3Scanline()
        {
            // we render on BG2
            bool DoRenderOBJs = this.IO.DISPCNT.IsSet(DISPCNTFlags.DisplayOBJ) && ExternalOBJEnable;

            this.ResetBGWindows(2);
            this.ResetOBJWindow();

            if (DoRenderOBJs)
            {
                this.RenderOBJs();
            }

            if (this.IO.DISPCNT.DisplayBG(2))
            {
                for (int x = 0; x < width; x++)
                {
                    int priority = 4;

                    if (OBJWindow[x])
                    {
                        for (priority = 0; priority < 4; priority++)
                        {
                            if (this.OBJLayers[priority][x] != Transparent)
                            {
                                this.Display[width * scanline + x] = this.OBJLayers[priority][x];
                                priority = 0xff;  // break out of both loops
                                break;
                            }
                        }
                    }
                    if (priority == 4)  // no sprite found
                    {
                        if (this.BGWindows[2][x])
                        {
                            this.Display[width * scanline + x] = (ushort)((this.gba.mem.VRAM[2 * width * scanline + 2 * x + 1] << 8) |
                                                                           this.gba.mem.VRAM[2 * width * scanline + 2 * x]);
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[width * scanline + x] = 0;
                }
            }
        }
        
        private void Mode4Scanline()
        {
            // we render on BG2
            ushort offset = (ushort)(this.IO.DISPCNT.IsSet(DISPCNTFlags.DPFrameSelect) ? 0xa000 : 0);
            bool DoRenderOBJs = this.IO.DISPCNT.IsSet(DISPCNTFlags.DisplayOBJ) && ExternalOBJEnable;

            this.ResetBGWindows(2);
            this.ResetOBJWindow();

            if (DoRenderOBJs)
            {
                this.RenderOBJs();
            }

            if (this.IO.DISPCNT.DisplayBG(2))
            {
                for (int x = 0; x < width; x++)
                {
                    int priority = 4;

                    if (OBJWindow[x])
                    {
                        for (priority = 0; priority < 4; priority++)
                        {
                            if (this.OBJLayers[priority][x] != 0x8000)
                            {
                                this.Display[width * scanline + x] = this.OBJLayers[priority][x];
                                priority = 0xff;  // break out of both loops
                                break;
                            }
                        }
                    }
                    if (priority == 4)  // no sprite found
                    {
                        if (this.BGWindows[2][x])
                        {
                            this.Display[width * scanline + x] = this.GetPaletteEntry((uint)this.gba.mem.VRAM[offset + width * scanline + x] << 1);
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[width * scanline + x] = 0;
                }
            }
        }

        private void Mode5Scanline()
        {
            // I don't think this is working properly
            // I can't find much on mode 5 rendering though, so I'll just leave it
            if (scanline < 128 && this.IO.DISPCNT.DisplayBG(2))
            {
                ushort offset = (ushort)(this.IO.DISPCNT.IsSet(DISPCNTFlags.DPFrameSelect) ? 0xa000 : 0);

                // smaller format
                for (int x = 0; x < 160; x++)
                {
                    this.Display[width * scanline + x] = this.GetPaletteEntry((uint)this.gba.mem.VRAM[offset + width * scanline + x] << 1);
                }

                for (int x = 160; x < width; x++)
                {
                    this.Display[width * scanline + x] = 0;
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    this.Display[width * scanline + x] = 0;
                }
            }
        }
    }
}
