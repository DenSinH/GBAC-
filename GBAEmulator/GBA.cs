using System;

using GBAEmulator.CPU;
using GBAEmulator.Memory;
using GBAEmulator.Bus;

namespace GBAEmulator
{
    public class GBA
    {
        public ARM7TDMI cpu;
        public PPU ppu;
        public MEM mem;
        public BUS bus;

        public Visual vis;
        public ushort[] display;

        public bool ShutDown;
        public bool Pause;

        public GBA(ushort[] display)
        {
            this.cpu = new ARM7TDMI(this);
            this.ppu = new PPU(this, display);
            this.mem = this.cpu.mem;
            this.bus = this.cpu.bus;

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
                this.mem.DISPSTAT.SetVBlank(true);
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
                this.mem.DISPSTAT.SetVBlank(false);
            }

            if (this.ppu.IsVBlank)
            {
                this.mem.BG2X.ResetInternal();
                this.mem.BG2Y.ResetInternal();
                this.mem.BG3X.ResetInternal();
                this.mem.BG3Y.ResetInternal();
            }

            /* NON-HBLANK */
            this.mem.DISPSTAT.SetHBlank(false);
            this.mem.VCOUNT.CurrentScanline = this.ppu.scanline;  // we also check for IRQ's this way
            if (this.ppu.scanline >= 2 && this.ppu.scanline < 162)
            {
                // DMA 3 video capture mode (special DMA trigger)
                this.cpu.TriggerDMASpecial(3);
            }

            this.cycle += NonHBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            /* HBLANK */
            this.mem.DISPSTAT.SetHBlank(true);
            if (!ppu.IsVBlank) this.cpu.TriggerDMA(DMAStartTiming.HBlank);
            this.ppu.DrawScanline();

            this.cycle += HBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            this.mem.BG2X.UpdateInternal((uint)this.mem.BG2PB.Full);
            this.mem.BG2Y.UpdateInternal((uint)this.mem.BG2PD.Full);
            this.mem.BG3X.UpdateInternal((uint)this.mem.BG3PB.Full);
            this.mem.BG3Y.UpdateInternal((uint)this.mem.BG3PD.Full);
        }

        public void Run()
        {
            // this.mem.LoadRom("../../../roms/ZeldaMinishCap.gba");
            // this.mem.LoadRom("../../../Tests/Krom/BIOSARCTAN.gba");
            // this.mem.LoadRom("../../../Tests/Marie/openbus-test_easy.gba");
            // this.mem.LoadRom("../../../Tests/Organharvester/joypad.gba");
            this.mem.LoadRom("../../../Tests/flero/openbuster.gba");
            // this.mem.LoadRom("../../../Tests/GBASuiteNew/nes.gba");
            // this.mem.LoadRom("../../../Tests/Tonc/obj_aff.gba");
            // this.mem.LoadRom("../../../Tests/Armwrestler/armwrestler.gba");
            // this.mem.LoadRom("../../../Tests/EndriftSuite.gba");

            // this.cpu.mem.UseNormattsBios();
            cpu.SkipBios();

            while (!this.ShutDown)
            {
                if (!this.Pause) this.RunLine();
            }
        }
    }
}
