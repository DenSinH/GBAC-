﻿using System;

using GBAEmulator.CPU;
using GBAEmulator.Bus;

namespace GBAEmulator.Memory
{
    public partial class MEM
    {
        ///* BIOS is defined in CPU.BIOS.cs */
        public byte[] eWRAM = new byte[0x40000];       // 256kB External Work RAM
        public byte[] iWRAM = new byte[0x8000];        // 32kB Internal Work RAM
        // 1kB IO RAM
        public byte[] PaletteRAM = new byte[0x400];    // 1kB Palette RAM
        public byte[] VRAM = new byte[0x18000];        // 96kB VRAM
        public byte[] OAM = new byte[0x400];           // 1kB OAM
        public byte[] GamePak = new byte[0x200_0000];   // Game Pak (up to) 32MB (0x0800_0000 - 0x0a00_0000, then mirrored)
        //// Backup Region

        //private readonly byte[][] __MemoryRegions__;  // Lookup table for memory regions for instant access (instead of switch statement)
        //private readonly uint[] __MemoryMasks__ = new uint[16]  // mirrored twice
        //{
        //    0x3fff, 0x3fff, 0x3ffff, 0x7fff, 0, 0x3ff, 0, 0x3ff, // 0 because VRAM mirrors are different, and IORAM contains registers
        //    0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0xffff, 0xffff
        //};

        // todo: waitstates / N & S cycles
        // Byte access cycles are equal to halfword access cycles

        private readonly int[] __ByteAccessSCycles__ = new int[16]
        {
            1, 1, 3, 1, 1, 1, 1, 1,
            5, 5, 5, 5, 5, 5, 5, 5
        };

        private readonly int[] __WordAccessSCycles__ = new int[16]
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
            
            this.InitBIOS();
            this.IORAMSection = new cIORAM(cpu, this, cpu.bus);
            this.BackupSection = new cBackupSection(this);
            this.GamePakSection_L = new cROMSection(this, false);
            this.GamePakSection_H = new cROMSection(this, true);

            //this.__MemoryRegions__ = new byte[16][]
            //{
            //    this.BIOS, this.BIOS, this.eWRAM, this.iWRAM, null, this.PaletteRAM, null, this.OAM,
            //    this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, null, null
            //};

            this.MemorySections = new IMemorySection[16]
            {
                this.BIOSSection,
                this.UnusedSection,
                this.eWRAMSection,
                this.iWRAMSection,
                this.IORAMSection,
                this.PaletteRAMSection,
                this.VRAMSection,
                this.OAMSection,

                this.GamePakSection_L,
                this.GamePakSection_H,
                this.GamePakSection_L,
                this.GamePakSection_H,
                this.GamePakSection_L,
                this.GamePakSection_H,
                this.BackupSection,
                this.BackupSection
            };
        }

    }
}
