using System;

using GBAEmulator.Memory.Sections;
using GBAEmulator.IO;
using GBAEmulator.CPU;
using GBAEmulator.Bus;
using GBAEmulator.Video;

namespace GBAEmulator.Memory
{
    public partial class MEM
    {
        private readonly BIOSSection              BIOS;
        private readonly NonMirroredMemorySection UnusedSection = new NonMirroredMemorySection(0);
        private readonly MirroredMemorySection    eWRAM         = new MirroredMemorySection(0x40000);
        private readonly MirroredMemorySection    iWRAM         = new MirroredMemorySection(0x8000);
        public readonly  IORAMSection             IO;
        public readonly  PALSection               PAL           = new PALSection();
        public readonly  VRAMSection              VRAM;
        public readonly  OAMSection               OAM           = new OAMSection();
        private readonly cROMSection              GamePak_L;
        private readonly cROMSection              GamePak_H;
        public readonly  BackupSection            Backup;

        private readonly IMemorySection[] MemorySections;

        // Byte access cycles are equal to halfword access cycles
        private static readonly int[] NonWordAccessCycles = new int[16]
        {
            1, 1, 3, 1, 1, 1, 1, 1,
            5, 5, 5, 5, 5, 5, 5, 5
        };

        private static readonly int[] WordAccessCycles = new int[16]
        {
            1, 1, 6, 1, 1, 2, 2, 1,
            8, 8, 8, 8, 8, 8, 8, 8
        };

        public BIOSReadState CurrentBIOSReadState = BIOSReadState.StartUp;
        private ARM7TDMI cpu;
        private BUS bus;

        public MEM(ARM7TDMI cpu)
        {
            this.cpu = cpu;
            this.bus = cpu.bus;

            this.BIOS       = new BIOSSection(cpu);
            this.IO         = cpu.IO;
            this.VRAM       = new VRAMSection(cpu.IO);
            this.GamePak_L  = new cROMSection(this, false);
            this.GamePak_H  = new cROMSection(this, true);
            this.Backup     = new BackupSection(cpu.DMAChannels[3]);

            this.MemorySections = new IMemorySection[16]
            {
                this.BIOS,
                this.UnusedSection,
                this.eWRAM,
                this.iWRAM,
                this.IO,
                this.PAL,
                this.VRAM,
                this.OAM,

                this.GamePak_L,
                this.GamePak_H,
                this.GamePak_L,
                this.GamePak_H,
                this.GamePak_L,
                this.GamePak_H,
                this.Backup,
                this.Backup
            };
        }

        public void Init(PPU ppu)
        {
            this.OAM.Init(ppu);
            this.PAL.Init(ppu);
            this.VRAM.Init(ppu);
        }

        public void UseNormattsBIOS()
        {
            this.BIOS.UseNormattsBIOS();
        }
    }
}
