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
        public byte[] OAM = new byte[0x400];           // 1kB OAM
        private byte[] GamePak = new byte[0x200_0000];   // Game Pak (up to) 32MB (0x0800_0000 - 0x0a00_0000, then mirrored)
        private byte[] GamePakSRAM = new byte[0x10000]; // Game Pak Flash ROM (for saving game data)

        private readonly byte[][] __MemoryRegions__;  // Lookup table for memory regions for instant access (instead of switch statement)
        private readonly uint[] __MemoryMasks__ = new uint[16]  // mirrored twice
        {
            0x3fff, 0x3fff, 0x3ffff, 0x7fff, 0, 0x3ff, 0, 0x3ff, // 0 because VRAM mirrors are different, and IORAM contains registers
            0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0x1ff_ffff, 0xffff, 0xffff
        };

        // todo: waitstates / N & S cycles
        // Byte access cycles are equal to halfword access cycles
        private readonly int[] __ByteAccessCycles__ = new int[16]
        {
            1, 1, 3, 1, 1, 1, 1, 1,
            5, 5, 5, 5, 5, 5, 5, 5
        };

        private readonly int[] __WordAccessCycles__ = new int[16]
        {
            1, 1, 6, 1, 1, 2, 2, 1,
            8, 8, 8, 8, 8, 8, 8, 8
        };

        enum MemorySection : byte
        {
            BIOS = 0,
            // BIOS Mirror
            eWRAM = 2,
            iWRAM = 3,
            IORAM = 4,
            PaletteRAM = 5,
            VRAM = 6,
            OAM = 7
            // otherwise GamePak
        }
        
        private uint GetWordAt(uint address)
        {
            // trying to find the insane overflow bug in bigmap.gba
            //if (address == 0x0300_0000 + 32412)  // y coord is stored at this address it seems
            //{
            //    Console.WriteLine(this.state);
            //    Console.WriteLine((this.PC - 4).ToString("x8"));
            //    Console.WriteLine(__GetWordAt__(this.iWRAM, address & 0xffff));
            //}

            byte Section = (byte)((address & 0x0f00_0000) >> 24);
            this.NCycle = __WordAccessCycles__[Section];
            this.SCycle = __WordAccessCycles__[Section];

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
            this.NCycle = __WordAccessCycles__[Section];
            this.SCycle = __WordAccessCycles__[Section];

            //if (address >= 0x0600_8000 && address < 0x0600_c000)
            //{
            //    //Console.WriteLine("word: " + address.ToString("x8"));
            //    //Console.WriteLine(this.PC.ToString("x8"));
            //    if (this.PC < 0x0800_0000)
            //        return;
            //}

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
            this.NCycle = __ByteAccessCycles__[Section];
            this.SCycle = __ByteAccessCycles__[Section];

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
            this.NCycle = __ByteAccessCycles__[Section];
            this.SCycle = __ByteAccessCycles__[Section];

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

            this.NCycle = __ByteAccessCycles__[Section];
            this.SCycle = __ByteAccessCycles__[Section];

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
            this.NCycle = __ByteAccessCycles__[Section];
            this.SCycle = __ByteAccessCycles__[Section];

            if (Section >= 5)
            {
                /*
                 Writing 8bit Data to Video Memory
                Video Memory (BG, OBJ, OAM, Palette) can be written to in 16bit and 32bit units only.
                Attempts to write 8bit data (by STRB opcode) won't work:

                Writes to OBJ (6010000h-6017FFFh) (or 6014000h-6017FFFh in Bitmap mode)
                and to OAM (7000000h-70003FFh) are ignored, the memory content remains unchanged.

                Writes to BG (6000000h-600FFFFh) (or 6000000h-6013FFFh in Bitmap mode)
                and to Palette (5000000h-50003FFh) are writing the new 8bit value to BOTH upper and
                lower 8bits of the addressed halfword, ie. "[addr AND NOT 1]=data*101h".
                 */
                switch ((MemorySection)Section)
                {
                    case MemorySection.VRAM:   // ignore OAM byte stores
                        if (this.DISPCNT.BGMode >= 3)
                        {
                            if (address >= 0x0601_4000)
                            {
                                return;
                            }
                            else
                            {
                                this.VRAM[address & 0x17ffe] = value;
                                this.VRAM[(address & 0x17ffe) | 1] = value;
                                return;
                            }
                        }
                        else if (address >= 0x0601_0000)
                        {
                            // non-bitmap modes
                            return;
                        }
                        else
                        {
                            this.VRAM[address & 0xfffe] = value;
                            this.VRAM[(address & 0xfffe) | 1] = value;
                            return;
                        }
                    case MemorySection.OAM:
                        return;
                    case MemorySection.PaletteRAM:  // ignore PaletteRAM byte stores
                        this.PaletteRAM[address & 0x3fe] = value;
                        this.PaletteRAM[(address & 0x3fe) | 1] = value;
                        return;
                }
            }

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
