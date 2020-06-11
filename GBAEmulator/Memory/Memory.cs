using System;

using GBAEmulator.Memory.Sections;
using GBAEmulator.CPU;
using GBAEmulator.Bus;

namespace GBAEmulator.Memory
{
    public partial class MEM
    {
        private BIOSSection              BIOS;
        private NonMirroredMemorySection UnusedSection = new NonMirroredMemorySection(0);
        private MirroredMemorySection    eWRAM         = new MirroredMemorySection(0x40000);
        private MirroredMemorySection    iWRAM         = new MirroredMemorySection(0x8000);
        public  IORAMSection             IORAM;
        public  PALSection               PAL           = new PALSection();
        public  VRAMSection              VRAM;
        public  OAMSection               OAM           = new OAMSection();
        private cROMSection              GamePak_L;
        private cROMSection              GamePak_H;
        public  BackupSection            Backup;

        private IMemorySection[] MemorySections;

        // todo: waitstates / N & S cycles
        // Byte access cycles are equal to halfword access cycles

        private readonly int[] ByteAccessSCycles = new int[16]
        {
            1, 1, 3, 1, 1, 1, 1, 1,
            5, 5, 5, 5, 5, 5, 5, 5
        };

        private readonly int[] WordAccessSCycles = new int[16]
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
            this.IORAM      = new IORAMSection(cpu, this, cpu.bus);
            this.VRAM       = new VRAMSection(this.IORAM.DISPCNT);
            this.GamePak_L  = new cROMSection(this, false);
            this.GamePak_H  = new cROMSection(this, true);
            this.Backup = new BackupSection(this);

            this.MemorySections = new IMemorySection[16]
            {
                this.BIOS,
                this.UnusedSection,
                this.eWRAM,
                this.iWRAM,
                this.IORAM,
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

    }
}
