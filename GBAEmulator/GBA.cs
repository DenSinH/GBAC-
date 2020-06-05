using System;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    public class GBA
    {
        public ARM7TDMI cpu;
        public PPU ppu;

        public Visual vis;
        public ushort[] display;

        public bool ShutDown;

        public GBA(ushort[] display)
        {
            this.cpu = new ARM7TDMI(this);
            this.ppu = new PPU(this, display);

            this.display = display;
        }

        const int NonHBlankCycles = 960;
        const int HBlankCycles = 272;

        long cycle;

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

            // set VBlank
            if (this.ppu.scanline == 160)
            {
                this.cpu.DISPSTAT.SetVBlank(true);
                this.cpu.TriggerDMA(DMAStartTiming.VBlank);

                // refresh screen, vis might have been destroyed because we ended the thread
                try
                {
                    this.vis?.Invoke(this.vis.Tick);
                }
                catch (ObjectDisposedException)
                {
                    // disgusting I know... 
                }
            }
            // no VBlank in 227
            else if (this.ppu.scanline == 227)
            {
                this.cpu.DISPSTAT.SetVBlank(false);
            }

            if (this.ppu.IsVBlank)
            {
                this.cpu.BG2X.ResetInternal();
                this.cpu.BG2Y.ResetInternal();
                this.cpu.BG3X.ResetInternal();
                this.cpu.BG3Y.ResetInternal();
            }

            /* NON-HBLANK */
            this.cpu.DISPSTAT.SetHBlank(false);
            this.cpu.VCOUNT.CurrentScanline = this.ppu.scanline;  // we also check for IRQ's this way
            if (this.ppu.scanline >= 2 && this.ppu.scanline < 162)
            {
                // DMA 3 video capture mode (special DMA trigger)
                this.cpu.TriggerDMASpecial(3);
            }

            this.cycle += NonHBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            /* HBLANK */
            this.cpu.DISPSTAT.SetHBlank(true);
            if (!ppu.IsVBlank) this.cpu.TriggerDMA(DMAStartTiming.HBlank);
            this.ppu.DrawScanline();

            this.cycle += HBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            this.cpu.BG2X.UpdateInternal((uint)this.cpu.BG2PB.Full);
            this.cpu.BG2Y.UpdateInternal((uint)this.cpu.BG2PD.Full);
            this.cpu.BG3X.UpdateInternal((uint)this.cpu.BG3PB.Full);
            this.cpu.BG3Y.UpdateInternal((uint)this.cpu.BG3PD.Full);
        }

        public void Run()
        {
            cpu.LoadRom("../../roms/PokemonEmerald.gba");
            // cpu.LoadRom("../../Tests/Krom/BIOSARCTAN.gba");
            // cpu.LoadRom("../../Tests/Marie/openbus-test_easy.gba");
            // cpu.LoadRom("../../Tests/Organharvester/joypad.gba");
            // cpu.LoadRom("../../Tests/GBASuiteNew/mem.gba");
            // cpu.LoadRom("../../Tests/Tonc/bld_demo.gba");
            // cpu.LoadRom("../../Tests/Armwrestler/armwrestler.gba");
            // cpu.LoadRom("../../Tests/EndriftSuite.gba");
            
            // this.cpu.UseNormattsBios();
            cpu.SkipBios();

            while (!this.ShutDown)
            {
                this.RunLine();
            }
        }
    }
}
