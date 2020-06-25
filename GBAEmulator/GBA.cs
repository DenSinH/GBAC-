using System;

using GBAEmulator.CPU;
using GBAEmulator.Video;
using GBAEmulator.Memory;
using GBAEmulator.Bus;
using GBAEmulator.IO;
using GBAEmulator.Audio;
using GBAEmulator.Scheduler;

namespace GBAEmulator
{
    public class GBA
    {
        public readonly ARM7TDMI cpu;
        public readonly APU apu;
        public readonly PPU ppu;
        public readonly IORAMSection IO;
        public readonly MEM mem;
        public readonly BUS bus;

        // how many events at once:
        // 1 for each channel, framecounter, Provide sample => 6 for APU
        // 1 for each timer                                 => 4 for CPU
        private readonly Scheduler.Scheduler EventQueue = new Scheduler.Scheduler();

        public Visual vis;
        public readonly ushort[] display;

        public bool ShutDown { private get; set; }
        public bool Pause;
        public bool Alive { get; private set; } = true;

        public GBA(ushort[] display)
        {
            this.cpu = new ARM7TDMI(this, this.EventQueue);
            this.IO  = this.cpu.IO;
            this.mem = this.cpu.mem;
            this.bus = this.cpu.bus;

            this.apu = new APU(this.cpu, this.EventQueue);
            this.ppu = new PPU(this, display, this.IO);

            this.IO.Init(this.cpu, this.apu);

            this.display = display;
        }

        private const int NonHBlankCycles = 960;
        private const int HBlankNoFlagCycles = 46;
        private const int HBlankWithFlagCycles = 226;

        private long cycle;

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

            // We don't have to normalize the event queue, since we are using ulongs to store the time, this makes it so we can play
            // games for up to 2 ** 63 / 2 ** 24 = 2 ** 39 seconds, or ~16 300 years
            // We are probably dead before the EventQueue messes up

            // set VBlank
            if (this.ppu.scanline == 160)
            {
                this.mem.IO.DISPSTAT.SetVBlank(true);
                this.cpu.TriggerDMA(DMAStartTiming.VBlank);

                // refresh screen, vis might have been destroyed because we ended the thread
                try
                {
                    this.vis?.BeginInvoke(this.vis.Tick);
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
                this.cpu.DMAChannels[3].Trigger(DMAStartTiming.Special);
            }
            else if (this.cpu.DMAChannels[3].DMACNT_H.StartTiming == DMAStartTiming.Special && this.ppu.scanline == 162)
            {
                // this.cpu.mem.IORAM.DMACNT_H[3].Active = false;
                this.cpu.DMAChannels[3].DMACNT_H.Disable();
            }

            this.cycle += NonHBlankCycles;
            while (this.cycle > 0)
            {
                this.cycle -= this.cpu.Step();
                this.EventQueue.Handle(this.cpu.GlobalCycleCount);
            }

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
            {
                this.cycle -= this.cpu.Step();
                this.EventQueue.Handle(this.cpu.GlobalCycleCount);
            }

            this.mem.IO.DISPSTAT.SetHBlank(true);

            this.cycle += HBlankWithFlagCycles;
            while (this.cycle > 0)
            {
                this.cycle -= this.cpu.Step();
                this.EventQueue.Handle(this.cpu.GlobalCycleCount);
            }

            this.mem.IO.BG2X.UpdateInternal((uint)this.mem.IO.BG2PB.Full);
            this.mem.IO.BG2Y.UpdateInternal((uint)this.mem.IO.BG2PD.Full);
            this.mem.IO.BG3X.UpdateInternal((uint)this.mem.IO.BG3PB.Full);
            this.mem.IO.BG3Y.UpdateInternal((uint)this.mem.IO.BG3PD.Full);
        }

        public void Reset()
        {
            this.IO.Reset();
            this.cpu.Reset();
        }

        public void Run(string ROMPath)
        {
            this.mem.LoadRom(ROMPath);

            // this.mem.UseNormattsBios();
            cpu.SkipBios();

            this.Alive = true;
            while (!this.ShutDown)
            {
                if (!this.Pause) this.RunLine();
            }
            this.Alive = false;
        }
    }
}
