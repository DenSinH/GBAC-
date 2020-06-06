using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        [Conditional("DEBUG")]
        private void MemoryAccess(uint Address)
        {
            // Console.WriteLine("Memory Access: " + Address.ToString("x8"));
        }

        /* BIOS is defined in CPU.BIOS.cs */
        private byte[] eWRAM = new byte[0x40000];       // 256kB External Work RAM
        private byte[] iWRAM = new byte[0x8000];        // 32kB Internal Work RAM
        // 1kB IO RAM
        public byte[] PaletteRAM = new byte[0x400];    // 1kB Palette RAM
        public byte[] VRAM = new byte[0x18000];        // 96kB VRAM
        public byte[] OAM = new byte[0x400];           // 1kB OAM
        private byte[] GamePak = new byte[0x200_0000];   // Game Pak (up to) 32MB (0x0800_0000 - 0x0a00_0000, then mirrored)
        // Backup Region

        private readonly byte[][] __MemoryRegions__;  // Lookup table for memory regions for instant access (instead of switch statement)
        private readonly uint[] __MemoryMasks__ = new uint[16]  // mirrored twice
        {
            0x3fff, 0x3fff, 0x3ffff, 0x7fff, 0, 0x3ff, 0, 0x3ff, // 0 because VRAM mirrors are different, and IORAM contains registers
            0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0x01ff_ffff, 0xffff, 0xffff
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

        /*
         todo:
         When accessing OAM (7000000h) or OBJ VRAM (6010000h) by HBlank Timing,
         then the "H-Blank Interval Free" bit in DISPCNT register must be set.
        */

        private uint GetWordAt(uint address)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.NCycle = __WordAccessCycles__[Section];
                this.SCycle = __WordAccessCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                    if ((address & 0x00ff_ffff) <= __MemoryMasks__[Section]) return __GetWordAt__(this.BIOS, address & __MemoryMasks__[Section]);
                    return this.Pipeline.Peek();
                case 1:
                    return this.Pipeline.Peek();
                case 2:
                    return __GetWordAt__(this.eWRAM, address & __MemoryMasks__[Section]);
                case 3:
                    return __GetWordAt__(this.iWRAM, address & __MemoryMasks__[Section]);
                case 4:   // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) return this.IOGetWordAt(address & 0x3ff);
                    return this.Pipeline.Peek();
                case 5:
                    return __GetWordAt__(this.PaletteRAM, address & __MemoryMasks__[Section]);
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetWordAt__(this.VRAM, address & 0xffff);
                    }
                    return __GetWordAt__(this.VRAM, 0x10000 | (address & 0x7fff));
                case 7:
                    return __GetWordAt__(this.OAM, address & __MemoryMasks__[Section]);
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    uint _address = address & 0x01ff_ffff;
                    if (_address < ROMSize)
                    {
                        return __GetWordAt__(this.GamePak, _address);
                    }
                    return ((address >> 1) & 0xffff) | ((((address >> 1) + 1) & 0xffff) << 16);  // seems to be what mGBA is doing...
                case 14:
                case 15:  // SRAM
                    byte value = this.BackupRead(address & 0xffff);
                    return (uint)(value | (value << 8) | (value << 16) | (value << 24));
                default:
                    return this.Pipeline.Peek();
            }
        }

        private void SetWordAt(uint address, uint value)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.NCycle = __WordAccessCycles__[Section];
                this.SCycle = __WordAccessCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                case 1:
                    this.Error($"BIOS Word Write Attempted at {address.ToString("x8")} with PC = {this.PC.ToString("x8")}");
                    return;
                case 2:
                    __SetWordAt__(this.eWRAM, address & __MemoryMasks__[Section], value);
                    return;
                case 3:
                    __SetWordAt__(this.iWRAM, address & __MemoryMasks__[Section], value);
                    return;
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) this.IOSetWordAt(address & 0x3ff, value);
                    return;
                case 5:
                    __SetWordAt__(this.PaletteRAM, address & __MemoryMasks__[Section], value);
                    return;
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        __SetWordAt__(this.VRAM, address & 0xffff, value);
                        return;
                    }
                    __SetWordAt__(this.VRAM, 0x10000 | (address & 0x7fff), value);
                    return;
                case 7:
                    __SetWordAt__(this.OAM, address & __MemoryMasks__[Section], value);
                    return;
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    this.Error($"ROM Word Write Attempted at PC = {this.PC.ToString("x8")}");
                    return;
                case 14:
                case 15:  // SRAM
                    /* 
                     * Writing changes the 8bit value at the specified address only,
                     * being set to LSB of (source_data ROR (address*8)).
                     */
                    byte RORValue = (byte)this.ROR(value, (byte)(8 * (address & 3)));
                    this.BackupWrite(address & 0xffff, RORValue);
                    return;
                default:
                    return;
            }
        }

        private ushort GetHalfWordAt(uint address)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.NCycle = __ByteAccessCycles__[Section];
                this.SCycle = __ByteAccessCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                    if ((address & 0x00ff_ffff) <= __MemoryMasks__[Section]) return __GetHalfWordAt__(this.BIOS, address & __MemoryMasks__[Section]);
                    return (ushort)this.Pipeline.Peek();
                case 1:
                    return (ushort)this.Pipeline.Peek();
                case 2:
                    return __GetHalfWordAt__(this.eWRAM, address & __MemoryMasks__[Section]);
                case 3:
                    return __GetHalfWordAt__(this.iWRAM, address & __MemoryMasks__[Section]);
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) return this.IOGetHalfWordAt(address & 0x3ff);
                    return (ushort)this.Pipeline.Peek();
                case 5:
                    return __GetHalfWordAt__(this.PaletteRAM, address & __MemoryMasks__[Section]);
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetHalfWordAt__(this.VRAM, address & 0xffff);
                    }
                    return __GetHalfWordAt__(this.VRAM, 0x10000 | (address & 0x7fff));
                case 7:
                    return __GetHalfWordAt__(this.OAM, address & __MemoryMasks__[Section]);
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    uint _address = address & 0x01ff_ffff;
                    if (_address < ROMSize)
                    {
                        return __GetHalfWordAt__(this.GamePak, _address);
                    }
                    else if (address >= 0x0d00_0000) return 1;  // this is what mGBA seems to do...
                    return (ushort)((address >> 1) & 0xffff);
                case 14:
                case 15:  // SRAM
                    byte value = this.BackupRead(address & 0xffff);
                    return (ushort)(value | (value << 8));
                default:
                    return (ushort)this.Pipeline.Peek();
            }
        }

        private void SetHalfWordAt(uint address, ushort value)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.NCycle = __ByteAccessCycles__[Section];
                this.SCycle = __ByteAccessCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                case 1:
                    this.Error($"BIOS Halfword Write Attempted at {address.ToString("x8")} with PC = {this.PC.ToString("x8")}");
                    return;
                case 2:
                    __SetHalfWordAt__(this.eWRAM, address & __MemoryMasks__[Section], value);
                    return;
                case 3:
                    __SetHalfWordAt__(this.iWRAM, address & __MemoryMasks__[Section], value);
                    return;
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) this.IOSetHalfWordAt(address & 0x3ff, value);  // for now
                    return;
                case 5:
                    __SetHalfWordAt__(this.PaletteRAM, address & __MemoryMasks__[Section], value);
                    return;
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        __SetHalfWordAt__(this.VRAM, address & 0xffff, value);
                        return;
                    }
                    __SetHalfWordAt__(this.VRAM, 0x10000 | (address & 0x7fff), value);
                    return;
                case 7:
                    __SetHalfWordAt__(this.OAM, address & __MemoryMasks__[Section], value);
                    return;
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    this.Error($"ROM Halfword Write Attempted at PC = {this.PC.ToString("x8")}");
                    return;
                case 14:
                case 15:  // SRAM
                    /* 
                     * Writing changes the 8bit value at the specified address only,
                     * being set to LSB of (source_data ROR (address*8)).
                     */
                    byte RORValue = (byte)this.ROR(value, (byte)(8 * (address & 3)));
                    this.BackupWrite(address & 0xffff, RORValue);
                    return;
                default:
                    return;
            }
        }

        private byte GetByteAt(uint address)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.NCycle = __ByteAccessCycles__[Section];
                this.SCycle = __ByteAccessCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                    if ((address & 0x00ff_ffff) <= __MemoryMasks__[Section]) return this.BIOS[address & __MemoryMasks__[Section]];
                    return (byte)this.Pipeline.Peek();
                case 1:
                    return (byte)this.Pipeline.Peek();
                case 2:
                    return this.eWRAM[address & __MemoryMasks__[Section]];
                case 3:
                    return this.iWRAM[address & __MemoryMasks__[Section]];
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) return (byte)this.IOGetByteAt(address & 0x3ff);
                    return (byte)this.Pipeline.Peek();
                case 5:
                    return this.PaletteRAM[address & __MemoryMasks__[Section]];
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return this.VRAM[address & 0xffff];
                    }
                    return this.VRAM[0x10000 | (address & 0x7fff)];
                case 7:
                    return this.OAM[address & __MemoryMasks__[Section]];
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    address &= 0x01ff_ffff;
                    if (address < ROMSize)
                    {
                        return this.GamePak[address];
                    }
                    return (byte)((address >> 1) & 0xff);
                case 14:
                case 15:  // SRAM
                    return this.BackupRead(address & 0xffff);
                default:
                    return (byte)this.Pipeline.Peek();
            }
        }

        private void SetByteAt(uint address, byte value)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.NCycle = __ByteAccessCycles__[Section];
                this.SCycle = __ByteAccessCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                case 1:
                    this.Error($"BIOS Byte Write Attempted at {address.ToString("x8")} with PC = {this.PC.ToString("x8")}");
                    return;
                case 2:
                    this.eWRAM[address & __MemoryMasks__[Section]] = value;
                    return;
                case 3:
                    this.iWRAM[address & __MemoryMasks__[Section]] = value;
                    return;
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) this.IOSetByteAt(address & 0x3ff, value);
                    return;
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
                case 5:  // ignore PaletteRAM byte stores
                    this.PaletteRAM[address & 0x3fe] = value;
                    this.PaletteRAM[(address & 0x3fe) | 1] = value;
                    return;
                case 6:   // ignore OAM byte stores
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
                case 7:  // ignore OAM byte writes
                    return;
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    this.Error($"ROM Byte Write Attempted at PC = {this.PC.ToString("x8")}");
                    return;
                case 14:
                case 15:  // SRAM
                    this.BackupWrite(address & 0xffff, value);
                    return;
                default:
                    return;
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
