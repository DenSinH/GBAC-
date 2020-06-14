using System;

using GBAEmulator.CPU;
using GBAEmulator.Memory;
using GBAEmulator.Bus;
using GBAEmulator.IO;

namespace GBAEmulator
{
    public class GBA
    {
        public ARM7TDMI cpu;
        public PPU ppu;
        private IORAMSection IO;
        public MEM mem;
        public BUS bus;

        public Visual vis;
        public ushort[] display;

        public bool ShutDown;
        public bool Pause;

        public GBA(ushort[] display)
        {
            this.cpu = new ARM7TDMI(this);
            this.mem = this.cpu.mem;
            this.bus = this.cpu.bus;
            this.IO  = this.cpu.IO;

            this.ppu = new PPU(this, display, this.IO);

            this.display = display;
        }

        const int NonHBlankCycles = 960;
        const int HBlankNoFlagCycles = 46;
        const int HBlankWithFlagCycles = 226;

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
                this.mem.IO.DISPSTAT.SetVBlank(true);
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
                this.mem.IO.DISPSTAT.SetVBlank(false);
            }

            if (this.ppu.IsVBlank)
            {
                this.mem.IO.BG2X.ResetInternal();
                this.mem.IO.BG2Y.ResetInternal();
                this.mem.IO.BG3X.ResetInternal();
                this.mem.IO.BG3Y.ResetInternal();
            }

            /* NON-HBLANK */
            this.mem.IO.DISPSTAT.SetHBlank(false);
            this.mem.IO.VCOUNT.CurrentScanline = this.ppu.scanline;  // we also check for IRQ's this way
            if (this.ppu.scanline >= 2 && this.ppu.scanline < 162)
            {
                // DMA 3 video capture mode (special DMA trigger)
                this.cpu.TriggerDMASpecial(3);
            }
            else if (this.cpu.mem.IO.DMACNT_H[3].StartTiming == DMAStartTiming.Special && this.ppu.scanline == 162)
            {
                // this.cpu.mem.IORAM.DMACNT_H[3].Active = false;
                this.cpu.mem.IO.DMACNT_H[3].Disable();
            }

            this.cycle += NonHBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            /* HBLANK */
            if (this.mem.IO.DISPSTAT.IsSet(DISPSTATFlags.HBlankIRQEnable))
                this.mem.IO.IF.Request(Interrupt.LCDHBlank);

            if (!ppu.IsVBlank) this.cpu.TriggerDMA(DMAStartTiming.HBlank);
            this.ppu.DrawScanline();

            // Although the drawing time is only 960 cycles (240*4), the H-Blank flag is "0" for a total of 1006 cycles.
            // we split up the HBlank period into 2 smaller periods:
            //   - one where the HBlank flag is not set, but an HBlank IRQ has been requested
            //   - one where the HBlank flag is set
            this.cycle += HBlankNoFlagCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            this.mem.IO.DISPSTAT.SetHBlank(true);

            this.cycle += HBlankWithFlagCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            this.mem.IO.BG2X.UpdateInternal((uint)this.mem.IO.BG2PB.Full);
            this.mem.IO.BG2Y.UpdateInternal((uint)this.mem.IO.BG2PD.Full);
            this.mem.IO.BG3X.UpdateInternal((uint)this.mem.IO.BG3PB.Full);
            this.mem.IO.BG3Y.UpdateInternal((uint)this.mem.IO.BG3PD.Full);
        }

        public void Run()
        {
            // this.mem.LoadRom("../../../roms/MarioVSDonkeyKong.gba");
            // this.mem.LoadRom("../../../Tests/Krom/BIOSRLE.gba");
            // this.mem.LoadRom("../../../Tests/Marie/openbus-test_easy.gba");
            // this.mem.LoadRom("../../../Tests/Organharvester/joypad.gba");
            // this.mem.LoadRom("../../../Tests/flero/openbuster.gba");
            // this.mem.LoadRom("../../../Tests/GBASuiteNew/bios.gba");
            // this.mem.LoadRom("../../../Tests/Tonc/prio_demo.gba");
            this.mem.LoadRom("../../../Tests/AgingCard.gba");

            // this.cpu.mem.UseNormattsBios();
            cpu.SkipBios();

            while (!this.ShutDown)
            {
                if (!this.Pause) this.RunLine();
            }
        }
    }
}
