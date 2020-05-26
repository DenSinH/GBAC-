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

        private void RunLine()
        {
            // Console.Write("y_lo: "); this.cpu.ShowIWRAMAt(32412);
            // Console.Write("y_hi: "); this.cpu.ShowIWRAMAt(32448);

            // Console.Write("x_lo: "); this.cpu.ShowIWRAMAt(32416);
            // Console.Write("x_hi: "); this.cpu.ShowIWRAMAt(32444);

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
            if (this.ppu.IsVBlank)
            {
                this.cpu.BG2X.ResetInternal();
                this.cpu.BG2Y.ResetInternal();
                this.cpu.BG3X.ResetInternal();
                this.cpu.BG3Y.ResetInternal();
            }

            this.cpu.DISPSTAT.SetHBlank(false);
            this.cpu.VCOUNT.CurrentScanline = this.ppu.scanline;  // we also check for IRQ's this way

            for (cycle = 0; cycle < 960; cycle++)
                this.cpu.Step();

            this.cpu.DISPSTAT.SetHBlank(true);
            this.ppu.DrawScanline();

            for (cycle = 0; cycle < 272; cycle++)
                this.cpu.Step();

            this.cpu.BG2X.UpdateInternal((uint)this.cpu.BG2PB.Full);
            this.cpu.BG2Y.UpdateInternal((uint)this.cpu.BG2PD.Full);
            this.cpu.BG3X.UpdateInternal((uint)this.cpu.BG3PB.Full);
            this.cpu.BG3Y.UpdateInternal((uint)this.cpu.BG3PD.Full);
        }

        public void Run()
        {
            // cpu.LoadRom("../../roms/KirbyNightmare.gba");
            // cpu.LoadRom("../../Tests/Krom/BIOSBIT1BPP.gba");
            cpu.LoadRom("../../Tests/Tonc/swi_demo.gba");
            // cpu.LoadRom("../../Tests/GBASuiteNew/thumb.gba");
            // cpu.LoadRom("../../Tests/AgingCard.gba");
            cpu.SkipBios();

            while (!this.ShutDown)
            {
                this.RunLine();
            }
        }
    }
}
