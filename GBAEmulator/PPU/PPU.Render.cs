using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    this.Display[240 * scanline + x] = this.gba.cpu.GetPaletteEntry((uint)this.gba.cpu.VRAM[offset + 240 * scanline + x] << 1);
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
                    this.Display[240 * scanline + x] = this.gba.cpu.GetPaletteEntry((uint)this.gba.cpu.VRAM[offset + width * scanline + x] << 1);
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
