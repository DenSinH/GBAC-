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
        private byte[] PaletteRAM = new byte[0x400];    // 1kB Palette RAM
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

        public ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                PaletteRAM[Address] |
                (PaletteRAM[Address + 1] << 8)
                );
        }

        private uint GetWordAt(uint Address)
        {
            byte Section = (byte)((Address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
                return __GetWordAt__(this.__MemoryRegions__[Section], Address & __MemoryMasks__[Section]);


            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((Address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetWordAt__(this.VRAM, Address & 0xffff);
                    }
                    return __GetWordAt__(this.VRAM, 0x10000 | (Address & 0x7fff));
                case 4: // IORAM
                    return this.IOGetWordAt(Address & 0x3ff);
                default:
                    throw new Exception("This cannot happen");
            }
            
        }

        private void SetWordAt(uint Address, uint Value)
        {
            byte Section = (byte)((Address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
            {
                __SetWordAt__(this.__MemoryRegions__[Section], Address & __MemoryMasks__[Section], Value);
                return;
            }

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((Address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        __SetWordAt__(this.VRAM, Address & 0xffff, Value);
                        return;
                    }
                    __SetWordAt__(this.VRAM, 0x10000 | (Address & 0x7fff), Value);
                    return;
                case 4: // IORAM
                    this.IOSetWordAt(Address & 0x3ff, Value);
                    return;
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private ushort GetHalfWordAt(uint Address)
        {
            byte Section = (byte)((Address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
                return __GetHalfWordAt__(this.__MemoryRegions__[Section], Address & __MemoryMasks__[Section]);

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((Address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetHalfWordAt__(this.VRAM, Address & 0xffff);
                    }
                    return __GetHalfWordAt__(this.VRAM, 0x10000 | (Address & 0x7fff));
                case 4: // IORAM
                    return (ushort)this.IOGetHalfWordAt(Address & 0x3ff);
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private void SetHalfWordAt(uint Address, ushort Value)
        {
            byte Section = (byte)((Address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
            {
                __SetHalfWordAt__(this.__MemoryRegions__[Section], Address & __MemoryMasks__[Section], Value);
                return;
            }

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((Address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        __SetHalfWordAt__(this.VRAM, Address & 0xffff, Value);
                        return;
                    }
                    __SetHalfWordAt__(this.VRAM, 0x10000 | (Address & 0x7fff), Value);
                    return;
                case 4: // IORAM
                    this.IOSetHalfWordAt(Address & 0x3ff, Value);  // for now
                    return;
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private byte GetByteAt(uint Address)
        {
            byte Section = (byte)((Address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
                return this.__MemoryRegions__[Section][Address & __MemoryMasks__[Section]];

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((Address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return this.VRAM[Address & 0xffff];
                    }
                    return this.VRAM[0x10000 | (Address & 0x7fff)];
                case 4: // IORAM
                    return (byte)this.IOGetByteAt(Address & 0x3ff);
                default:
                    throw new Exception("This cannot happen");
            }
        }

        private void SetByteAt(uint Address, byte Value)
        {
            byte Section = (byte)((Address & 0x0f00_0000) >> 24);
            if (__MemoryRegions__[Section] != null)
            {
                this.__MemoryRegions__[Section][Address & __MemoryMasks__[Section]] = Value;
                return;
            }

            switch (Section)
            {
                case 6:  // VRAM Mirrors
                    if ((Address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        this.VRAM[Address & 0xffff] = Value;
                        return;
                    }
                    this.VRAM[0x10000 | (Address & 0x7fff)] = Value;
                    return;
                case 4: // IORAM
                    this.IOSetByteAt(Address & 0x3ff, Value);
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
