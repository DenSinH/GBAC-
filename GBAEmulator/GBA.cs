using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    public class GBA
    {
        public ARM7TDMI cpu;
        public PPU ppu;
        public ushort[] display;

        public GBA(ushort[] display)
        {
            this.cpu = new ARM7TDMI(this);
            this.ppu = new PPU(this, display);

            this.display = display;
        }

        public void Test()
        {
            cpu.TestGBASuite("arm");
            cpu.TestReadWrite();
            cpu.TestReadWrite();
        }

        private void RunFrame()
        {
            /*
            subject	    length	cycles
            pixel	    1	            4
            HDraw	    240px	        960
            HBlank	    68px	        272
            scanline	Hdraw+Hbl	    1232
            VDraw	    160*scanline	197120
            VBlank	    68*scanline	    83776
            refresh	    VDraw+Vbl	    280896

            from: https://www.coranac.com/tonc/text/video.htm
             */
            int cycle;

            // CHECK VBLANK
            // CLEAR HBLANK
            for (cycle = 0; cycle < 960; cycle++)
                this.cpu.Step();

            // SET HBLANK
            this.ppu.HackyMode4Scanline();

            for (cycle = 0; cycle < 272; cycle++)
                this.cpu.Step();
        }

        public void Run()
        {
            cpu.LoadRom("../../Tests/GBASuite/thumb.gba");
            cpu.SkipBios();

            while (true)
            {
                this.RunFrame();
            }
        }
    }
}
