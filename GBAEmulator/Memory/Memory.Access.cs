using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using GBAEmulator.CPU;

namespace GBAEmulator.Memory
{
    public partial class MEM
    {
        [Conditional("DEBUG")]
        private void MemoryAccess(uint Address)
        {
            Console.WriteLine("Memory Access: " + Address.ToString("x8"));
        }
        
        /*
         todo:
         When accessing OAM (7000000h) or OBJ VRAM (6010000h) by HBlank Timing,
         then the "H-Blank Interval Free" bit in DISPCNT register must be set.
        */

        public uint GetWordAt(uint address)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.cpu.NCycle = __WordAccessSCycles__[Section];
                this.cpu.SCycle = __WordAccessSCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                    if (address <= __MemoryMasks__[Section])  // address is already < 0x0100_0000
                    {
                        if (this.cpu.PC < 0x0100_0000)
                        {
                            // normal BIOS fetch
                            return this.bus.BusValue = __GetWordAt__(this.BIOS, address & __MemoryMasks__[Section]);
                        }
                        return this.bus.BusValue = __GetWordAt__(this.BIOS, (uint)this.CurrentBIOSReadState);
                    }
                    return this.bus.OpenBus();
                case 1:
                    return this.bus.OpenBus();
                case 2:
                    return this.bus.BusValue = __GetWordAt__(this.eWRAM, address & __MemoryMasks__[Section]);
                case 3:
                    return this.bus.BusValue = __GetWordAt__(this.iWRAM, address & __MemoryMasks__[Section]);
                case 4:   // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) return this.IOGetWordAt(address & 0x3ff);
                    return this.bus.OpenBus();
                case 5:
                    return this.bus.BusValue = __GetWordAt__(this.PaletteRAM, address & __MemoryMasks__[Section]);
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return this.bus.BusValue = __GetWordAt__(this.VRAM, address & 0xffff);
                    }
                    return this.bus.BusValue = __GetWordAt__(this.VRAM, 0x10000 | (address & 0x7fff));
                case 7:
                    return this.bus.BusValue = __GetWordAt__(this.OAM, address & __MemoryMasks__[Section]);
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    uint _address = address & 0x01ff_ffff;

                    /*
                     the eeprom can be then addressed at DFFFF00h..DFFFFFFh.
                    Respectively, with eeprom, ROM is restricted to 8000000h-9FFFeFFh (max. 1FFFF00h bytes = 32MB minus 256 bytes).
                    On carts with 16MB or smaller ROM, eeprom can be alternately accessed anywhere at D000000h-DFFFFFFh.
                    (Tonc)
                     */
                    if (this.ROMBackupType == Backup.EEPROM)
                    {
                        if ((_address > 0x00ff_feff) ||
                            (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                        {
                            // EEPROM access, might as well call a read directly
                            return this.bus.BusValue = this.EEPROMRead();
                        }
                    }

                    if (_address < ROMSize)
                    {
                        return this.bus.BusValue = __GetWordAt__(this.GamePak, _address);
                    }
                    return this.bus.BusValue = ((address >> 1) & 0xffff) | ((((address >> 1) + 1) & 0xffff) << 16);  // seems to be what mGBA is doing...
                case 14:
                case 15:  // SRAM
                    byte value = this.BackupRead(address & 0xffff);
                    return (uint)(value | (value << 8) | (value << 16) | (value << 24));
                default:
                    return this.bus.OpenBus();
            }
        }

        public void SetWordAt(uint address, uint value, uint offset = 0)
        {
            this.MemoryAccess(address);
            this.bus.BusValue = value;

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.cpu.NCycle = __WordAccessSCycles__[Section];
                this.cpu.SCycle = __WordAccessSCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                case 1:
                    this.Error($"BIOS Word Write Attempted at {address.ToString("x8")} with PC = {this.cpu.PC.ToString("x8")}");
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
                    uint _address = address & 0x01ff_ffff;
                    // See GetWordAt for info on EEPROM
                    if (this.ROMBackupType == Backup.EEPROM)
                    {
                        if ((_address > 0x00ff_feff) ||
                            (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                        {
                            // EEPROM access, might as well call a write directly
                            this.EEPROMWrite((byte)value);
                            return;
                        }
                    }

                    this.Error($"ROM Word Write Attempted at {address.ToString("x8")} with PC = {this.cpu.PC.ToString("x8")}");
                    return;
                case 14:
                case 15:  // SRAM
                    /* 
                     * Writing changes the 8bit value at the specified address only,
                     * being set to LSB of (source_data ROR (address*8)).
                     */
                    byte RORValue = (byte)(value >> (8 * (int)offset));
                    this.BackupWrite((address & 0xffff) + offset, RORValue);
                    return;
                default:
                    return;
            }
        }

        public ushort GetHalfWordAt(uint address)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.cpu.NCycle = __ByteAccessSCycles__[Section];
                this.cpu.SCycle = __ByteAccessSCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                    if (address <= __MemoryMasks__[Section])
                    {
                        if (this.cpu.PC < 0x0100_0000)
                        {
                            // normal BIOS fetch
                            return __GetHalfWordAt__(this.BIOS, address & __MemoryMasks__[Section]);
                        }
                        return __GetHalfWordAt__(this.BIOS, (uint)this.CurrentBIOSReadState);
                    }
                    return (ushort)this.bus.OpenBus();
                case 1:
                    return (ushort)this.bus.OpenBus();
                case 2:
                    return __GetHalfWordAt__(this.eWRAM, address & __MemoryMasks__[Section]);
                case 3:
                    return __GetHalfWordAt__(this.iWRAM, address & __MemoryMasks__[Section]);
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) return this.IOGetHalfWordAt(address & 0x3ff);
                    return (ushort)this.bus.OpenBus();
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

                    // See GetWordAt for info on EEPROM
                    if (this.ROMBackupType == Backup.EEPROM)
                    {
                        if ((_address > 0x00ff_feff) ||
                            (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                        {
                            // EEPROM access, might as well call a read directly
                            return this.EEPROMRead();
                        }
                    }

                    if (_address < ROMSize)
                    {
                        return __GetHalfWordAt__(this.GamePak, _address);
                    }
                    else if (address >= 0x0d00_0000)
                    {
                        return (ushort)(this.bus.BusValue = 1);  // this is what mGBA seems to do...
                    }
                    return (ushort)(this.bus.BusValue = (address >> 1) & 0xffff);
                case 14:
                case 15:  // SRAM
                    byte value = this.BackupRead(address & 0xffff);
                    return (ushort)(this.bus.BusValue = (this.bus.BusValue & 0xffff_0000) | (uint)(value | (value << 8)));
                default:
                    return (ushort)this.bus.OpenBus();
            }
        }

        public void SetHalfWordAt(uint address, ushort value, uint offset = 0)
        {
            this.MemoryAccess(address);
            this.bus.BusValue = (this.bus.BusValue & 0xffff_0000) | value;

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.cpu.NCycle = __ByteAccessSCycles__[Section];
                this.cpu.SCycle = __ByteAccessSCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                case 1:
                    // this.Error($"BIOS Halfword Write Attempted at {address.ToString("x8")} with PC = {this.cpu.PC.ToString("x8")}");
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
                    uint _address = address & 0x01ff_ffff;

                    // See GetWordAt for info on EEPROM
                    if (this.ROMBackupType == Backup.EEPROM)
                    {
                        if ((_address > 0x00ff_feff) ||
                            (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                        {
                            // EEPROM access, might as well call a write directly
                            this.EEPROMWrite((byte)value);
                            return;
                        }
                    }

                    this.Error($"ROM Halfword Write Attempted at {address.ToString("x8")} with PC = {this.cpu.PC.ToString("x8")}");
                    return;
                case 14:
                case 15:  // SRAM
                    /* 
                     * Writing changes the 8bit value at the specified address only,
                     * being set to LSB of (source_data ROR (address*8)).
                     */
                    byte RORValue = (byte)(value >> (8 * (int)offset));
                    this.BackupWrite((address & 0xffff) + offset, RORValue);
                    return;
                default:
                    return;
            }
        }

        public byte GetByteAt(uint address)
        {
            this.MemoryAccess(address);

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.cpu.NCycle = __ByteAccessSCycles__[Section];
                this.cpu.SCycle = __ByteAccessSCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                    if (address <= __MemoryMasks__[Section])
                    {
                        if (this.cpu.PC < 0x0100_0000)
                        {
                            // normal BIOS fetch
                            return __GetByteAt__(this.BIOS, address & __MemoryMasks__[Section]);
                        }
                        return __GetByteAt__(this.BIOS, (uint)this.CurrentBIOSReadState);
                    }
                    return (byte)this.bus.OpenBus();
                case 1:
                    return (byte)this.bus.OpenBus();
                case 2:
                    return __GetByteAt__(this.eWRAM, address & __MemoryMasks__[Section]);
                case 3:
                    return __GetByteAt__(this.iWRAM, address & __MemoryMasks__[Section]);
                case 4: // IORAM
                    if ((address & 0x00ff_ffff) < 0x400) return this.IOGetByteAt(address & 0x3ff);
                    return (byte)this.bus.OpenBus();
                case 5:
                    return __GetByteAt__(this.PaletteRAM, address & __MemoryMasks__[Section]);
                case 6:  // VRAM Mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return __GetByteAt__(this.VRAM, address & 0xffff);
                    }
                    return __GetByteAt__(this.VRAM, 0x10000 | (address & 0x7fff));
                case 7:
                    return __GetByteAt__(this.OAM, address & __MemoryMasks__[Section]);
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:  // GamePak
                    uint _address = address & 0x01ff_ffff;

                    // See GetWordAt for info on EEPROM
                    if (this.ROMBackupType == Backup.EEPROM)
                    {
                        if ((_address > 0x00ff_feff) ||
                            (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                        {
                            // EEPROM access, might as well call a read directly
                            return this.EEPROMRead();
                        }
                    }

                    if (_address < ROMSize)
                    {
                        return __GetByteAt__(this.GamePak, _address);
                    }
                    return (byte)(this.bus.BusValue = (_address >> 1) & 0xff);
                case 14:
                case 15:  // SRAM
                    return this.BackupRead(address & 0xffff);
                default:
                    return (byte)this.bus.OpenBus();
            }
        }

        public void SetByteAt(uint address, byte value)
        {
            this.MemoryAccess(address);
            this.bus.BusValue = (this.bus.BusValue & 0xffff_ff00) | value;

            byte Section = (byte)((address & 0xff00_0000) >> 24);
            if (Section < 0x10)
            {
                this.cpu.NCycle = __ByteAccessSCycles__[Section];
                this.cpu.SCycle = __ByteAccessSCycles__[Section];
            }

            switch (Section)
            {
                case 0:
                case 1:
                    this.Error($"BIOS Byte Write Attempted at {address.ToString("x8")} with PC = {this.cpu.PC.ToString("x8")}");
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
                case 5:
                    this.PaletteRAM[address & 0x3fe] = value;
                    this.PaletteRAM[(address & 0x3fe) | 1] = value;
                    return;
                case 6:
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
                    uint _address = address & 0x01ff_ffff;

                    // See GetWordAt for info on EEPROM
                    if (this.ROMBackupType == Backup.EEPROM)
                    {
                        if ((_address > 0x00ff_feff) ||
                            (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                        {
                            // EEPROM access, might as well call a write directly
                            this.EEPROMWrite(value);
                            return;
                        }
                    }

                    this.Error($"ROM Byte Write Attempted at {address.ToString("x8")} with PC = {this.cpu.PC.ToString("x8")}");
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte __GetByteAt__(byte[] memory, uint address)
        {
            uint value = ARM7TDMI.ROR(__GetWordAt__(memory, address & 0xffff_fffc), (int)(address & 3) * 8);
            this.bus.BusValue = value;

            return (byte)value;
            return memory[address];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort __GetHalfWordAt__(byte[] memory, uint address)
        {
            // assumes memory address does not wrap!
            uint value = ARM7TDMI.ROR(__GetWordAt__(memory, address & 0xffff_fffc), (int)(address & 3) * 8);
            this.bus.BusValue = value;

            return (ushort)value;
            return (ushort)((memory[address + 1] << 8) | memory[address]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
