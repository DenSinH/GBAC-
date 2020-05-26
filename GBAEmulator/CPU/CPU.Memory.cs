using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        /* BIOS is defined in CPU.BIOS.cs */
        private byte[] eWRAM = new byte[0x40000];       // 256kB External Work RAM
        private byte[] iWRAM = new byte[0x8000];        // 32kB Internal Work RAM
        // 1kB IO RAM
        public byte[] PaletteRAM = new byte[0x400];    // 1kB Palette RAM
        public byte[] VRAM = new byte[0x18000];        // 96kB VRAM
        private byte[] OAM = new byte[0x400];           // 1kB OAM
        private byte[] GamePak = new byte[0x200_0000];   // Game Pak (up to) 32MB (0x0800_0000 - 0x0a00_0000, then mirrored)
        private byte[] GamePakSRAM = new byte[0x10000]; // Game Pak Flash ROM (for saving game data)

        private readonly byte[][] __MemoryRegions__;  // Lookup table for memory regions for instant access (instead of switch statement)
        private readonly uint[] __MemoryMasks__ =
        {
            0x3fff, 0x3fff, 0x3ffff, 0x7fff, 0, 0x3ff, 0, 0x3ff, // 0 because VRAM mirrors are different, and IORAM contains registers
            0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0xffff
        };
        
        private uint GetWordAt(uint address)
        {
            // trying to find the insane overflow bug in bigmap.gba
            //if (address == 0x0300_0000 + 32412)
            //{
            //    Console.WriteLine(this.state);
            //    Console.WriteLine((this.PC - 4).ToString("x8"));
            //    Console.WriteLine(__GetWordAt__(this.iWRAM, address & 0xffff));
            //}

            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
                return __GetWordAt__(this.__MemoryRegions__[Section], address & __MemoryMasks__[Section]);


            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetWordAt__(this.VRAM, address & 0xffff);
                    }
                    return __GetWordAt__(this.VRAM, 0x10000 | (address & 0x7fff));
                case 4: // IORAM
                    return this.IOGetWordAt(address & 0x3ff);
                default:
                    throw new Exception("This cannot happen");
            }
            
        }

        private void SetWordAt(uint address, uint value)
        {
            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
            {
                __SetWordAt__(this.__MemoryRegions__[Section], address & __MemoryMasks__[Section], value);
                return;
            }

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    //Console.WriteLine("PC: " + (this.PC - 8).ToString("x8"));
                    //this.ShowInfo();

                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        __SetWordAt__(this.VRAM, address & 0xffff, value);
                        return;
                    }
                    __SetWordAt__(this.VRAM, 0x10000 | (address & 0x7fff), value);
                    return;
                case 4: // IORAM
                    this.IOSetWordAt(address & 0x3ff, value);
                    return;
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private ushort GetHalfWordAt(uint address)
        {
            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
                return __GetHalfWordAt__(this.__MemoryRegions__[Section], address & __MemoryMasks__[Section]);

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetHalfWordAt__(this.VRAM, address & 0xffff);
                    }
                    return __GetHalfWordAt__(this.VRAM, 0x10000 | (address & 0x7fff));
                case 4: // IORAM
                    return (ushort)this.IOGetHalfWordAt(address & 0x3ff);
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private void SetHalfWordAt(uint address, ushort value)
        {
            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
            {
                __SetHalfWordAt__(this.__MemoryRegions__[Section], address & __MemoryMasks__[Section], value);
                return;
            }

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        __SetHalfWordAt__(this.VRAM, address & 0xffff, value);
                        return;
                    }
                    __SetHalfWordAt__(this.VRAM, 0x10000 | (address & 0x7fff), value);
                    return;
                case 4: // IORAM
                    this.IOSetHalfWordAt(address & 0x3ff, value);  // for now
                    return;
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private byte GetByteAt(uint address)
        {
            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
                return this.__MemoryRegions__[Section][address & __MemoryMasks__[Section]];

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return this.VRAM[address & 0xffff];
                    }
                    return this.VRAM[0x10000 | (address & 0x7fff)];
                case 4: // IORAM
                    return (byte)this.IOGetByteAt(address & 0x3ff);
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private void SetByteAt(uint address, byte value)
        {
            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
            {
                this.__MemoryRegions__[Section][address & __MemoryMasks__[Section]] = value;
                return;
            }

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        this.VRAM[address & 0xffff] = value;
                        return;
                    }
                    this.VRAM[0x10000 | (address & 0x7fff)] = value;
                    return;
                case 4: // IORAM
                    this.IOSetByteAt(address & 0x3ff, value);
                    return;
                default:
                    throw new Exception("This cannot happen");
            }
        }

        /* =====================================================================================================
         *                                          Helper functions
         * =====================================================================================================
         */

        private static ushort __GetHalfWordAt__(byte[] memory, uint address)
        {
            // assumes memory address does not wrap!
            return (ushort)((memory[address + 1] << 8) | memory[address]);
        }

        private static void __SetHalfWordAt__(byte[] memory, uint address, ushort value)
        {
            // assumes memory address does not wrap!
            memory[address + 1] = (byte)((value & 0xff00) >> 8);
            memory[address] = (byte)(value & 0x00ff);
        }

        private static uint __GetWordAt__(byte[] memory, uint address)
        {
            // assumes memory address does not wrap!
            return (uint)(
                    (memory[address + 3] << 24) |
                    (memory[address + 2] << 16) |
                    (memory[address + 1] << 8) |
                    (memory[address])
                    );
        }

        private static void __SetWordAt__(byte[] memory, uint address, uint value)
        {
            // assumes memory address does not wrap!
            memory[address + 3] = (byte)((value & 0xff00_0000) >> 24);
            memory[address + 2] = (byte)((value & 0x00ff_0000) >> 16);
            memory[address + 1] = (byte)((value & 0x0000_ff00) >> 8);
            memory[address] = (byte)(value & 0x0000_00ff);
        }

    }
}
