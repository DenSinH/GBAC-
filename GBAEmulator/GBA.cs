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
                this.mem.IORAM.DISPSTAT.SetVBlank(true);
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
                this.mem.IORAM.DISPSTAT.SetVBlank(false);
            }

            if (this.ppu.IsVBlank)
            {
                this.mem.IORAM.BG2X.ResetInternal();
                this.mem.IORAM.BG2Y.ResetInternal();
                this.mem.IORAM.BG3X.ResetInternal();
                this.mem.IORAM.BG3Y.ResetInternal();
            }

            /* NON-HBLANK */
            this.mem.IORAM.DISPSTAT.SetHBlank(false);
            this.mem.IORAM.VCOUNT.CurrentScanline = this.ppu.scanline;  // we also check for IRQ's this way
            if (this.ppu.scanline >= 2 && this.ppu.scanline < 162)
            {
                // DMA 3 video capture mode (special DMA trigger)
                this.cpu.TriggerDMASpecial(3);
            }

            this.cycle += NonHBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            /* HBLANK */
            this.mem.IORAM.DISPSTAT.SetHBlank(true);
            if (!ppu.IsVBlank) this.cpu.TriggerDMA(DMAStartTiming.HBlank);
            this.ppu.DrawScanline();

            this.cycle += HBlankCycles;
            while (this.cycle > 0)
                this.cycle -= this.cpu.Step();

            this.mem.IORAM.BG2X.UpdateInternal((uint)this.mem.IORAM.BG2PB.Full);
            this.mem.IORAM.BG2Y.UpdateInternal((uint)this.mem.IORAM.BG2PD.Full);
            this.mem.IORAM.BG3X.UpdateInternal((uint)this.mem.IORAM.BG3PB.Full);
            this.mem.IORAM.BG3Y.UpdateInternal((uint)this.mem.IORAM.BG3PD.Full);
        }

        public void Run()
        {
            // this.mem.LoadRom("../../../roms/PokemonEmerald.gba");
            // this.mem.LoadRom("../../../Tests/Krom/BIOSARCTAN.gba");
            // this.mem.LoadRom("../../../Tests/Marie/openbus-test_easy.gba");
            // this.mem.LoadRom("../../../Tests/Organharvester/joypad.gba");
            // this.mem.LoadRom("../../../Tests/flero/openbuster.gba");
            // this.mem.LoadRom("../../../Tests/GBASuiteNew/bios.gba");
            // this.mem.LoadRom("../../../Tests/Tonc/obj_aff.gba");
            this.mem.LoadRom("../../../Tests/AgingCard.gba");

            // this.cpu.mem.BIOS.UseNormattsBios();
            cpu.SkipBios();

            while (!this.ShutDown)
            {
                if (!this.Pause) this.RunLine();
            }
        }
    }
}
