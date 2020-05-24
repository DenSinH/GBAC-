using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    public class GBA
    {
        public ARM7TDMI cpu;
        public PPU ppu;
        public ushort[] display;

        public bool ShutDown;

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

        private void RunLine()
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
            
            this.cpu.DISPSTAT.SetVBlank(this.ppu.IsVBlank);  // set VBlank to correct value
            this.cpu.DISPSTAT.SetHBlank(false);
            this.cpu.VCOUNT.CurrentScanline = this.ppu.scanline;  // we also check for IRQ's this way

            for (cycle = 0; cycle < 960; cycle++)
                this.cpu.Step();

            this.cpu.DISPSTAT.SetHBlank(true);
            this.ppu.DrawScanline();

            for (cycle = 0; cycle < 272; cycle++)
                this.cpu.Step();
        }

        public void Run()
        {
            // cpu.LoadRom("../../roms/KirbyNightmare.gba");
            // cpu.LoadRom("../../Tests/Krom/BIOSHUFFMAN.gba");
            // cpu.LoadRom("../../Tests/Tonc/brin_demo.gba");
            // cpu.LoadRom("../../Tests/GBASuiteNew/thumb.gba");
            cpu.SkipBios();

            while (!this.ShutDown)
            {
                this.RunLine();
            }
        }
    }
}
