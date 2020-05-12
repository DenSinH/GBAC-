using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class CPU
    {
        /* BIOS is defined in CPU.BIOS.cs */
        private byte[] eWRAM = new byte[0x40000];       // 256kB External Work RAM
        private byte[] iWRAM = new byte[0x8000];        // 32kB Internal Work RAM
        private byte[] IORAM = new byte[400];           // 1kB IO RAM
        private byte[] PaletteRAM = new byte[0x400];    // 1kB Palette RAM
        private byte[] VRAM = new byte[0x18000];        // 96kB VRAM
        private byte[] OAM = new byte[0x400];           // 1kB OAM
        private byte[] GamePak = new byte[0x200_0000];   // Game Pak (up to) 32MB (0x0800_0000 - 0x0a00_0000, then mirrored)
        private byte[] GamePakSRAM = new byte[0x10000]; // Game Pak Flash ROM (for saving game data)

        private T GetAt<T>(uint address) where T : IConvertible
        {
            switch ((address & 0x0f00_0000) >> 6)
            {
                case 0:
                case 1:
                    // BIOS region
                    return (T)GetData[typeof(T)](this.BIOS, address & 0x3fff);
                case 2:
                    // eWRAM mirrors
                    return (T)GetData[typeof(T)](this.eWRAM, address & 0x3ffff);
                case 3:
                    // iWRAM mirrors
                    return (T)GetData[typeof(T)](this.iWRAM, address & 0x7fff);
                case 4:
                    // IORAM mirrors
                    return (T)GetData[typeof(T)](this.IORAM, address & 0x3ff);
                case 5:
                    // PaletteRAM mirrors
                    return (T)GetData[typeof(T)](this.PaletteRAM, address & 0x3ff);
                case 6:
                    // VRAM mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return (T)GetData[typeof(T)](this.VRAM, address & 0xffff);
                    }
                    return (T)GetData[typeof(T)](this.VRAM, 0x10000 | (address & 0x7fff));
                case 7:
                    // OAM mirrors
                    return (T)GetData[typeof(T)](this.OAM, address & 0x3ff);
                case 0xe:
                    // SRAM is not mirrored (?)
                    return (T)GetData[typeof(T)](this.GamePakSRAM, address & 0xffff);
                case 0xf:
                    throw new IndexOutOfRangeException(string.Format("Index {0:x8} out of bounds for getting CPU Memory", address));
                default:
                    // Other regions are Game Pak at different speeds
                    return (T)GetData[typeof(T)](this.GamePak, address & 0x1ff_ffff);
            }
        }

        private void SetAt<T>(uint address, T value) where T : IConvertible
        {
            switch ((address & 0x0f00_0000) >> 6)
            {
                case 0:
                case 1:
                    // BIOS region
                    SetData[typeof(T)](this.BIOS, address & 0x3fff, value);
                    return;
                case 2:
                    // eWRAM mirrors
                    SetData[typeof(T)](this.eWRAM, address & 0x3ffff, value);
                    return;
                case 3:
                    // iWRAM mirrors
                    SetData[typeof(T)](this.iWRAM, address & 0x7fff, value);
                    return;
                case 4:
                    // IORAM mirrors
                    SetData[typeof(T)](this.IORAM, address & 0x3ff, value);
                    return;
                case 5:
                    // PaletteRAM mirrors
                    SetData[typeof(T)](this.PaletteRAM, address & 0x3ff, value);
                    return;
                case 6:
                    // VRAM mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        SetData[typeof(T)](this.VRAM, address & 0xffff, value);
                    }
                    else
                    {
                        SetData[typeof(T)](this.VRAM, 0x10000 | (address & 0x7fff), value);
                    }
                    return;
                case 7:
                    // OAM mirrors
                    SetData[typeof(T)](this.OAM, address & 0x3ff, value);
                    return;
                case 0xe:
                    // SRAM is not mirrored (?)
                    SetData[typeof(T)](this.GamePakSRAM, address & 0xffff, value);
                    return;
                case 0xf:
                    throw new IndexOutOfRangeException(string.Format("Index {0:x8} out of bounds for setting CPU Memory", address));
                default:
                    // Other regions are Game Pak at different speeds
                    SetData[typeof(T)](this.GamePak, address & 0x1ff_ffff, value);
                    return;
            }
        }

        /* Storing the functions that get / set specific data length values from memory */
        private static readonly Dictionary<Type, Action<byte[], uint, IConvertible>> SetData = 
            new Dictionary<Type, Action<byte[], uint, IConvertible>>
        {
            { typeof(byte), (byte[] memory, uint address, IConvertible value) => {memory[address] = (byte)value; } },
            { typeof(ushort), (byte[] memory, uint address, IConvertible value) => SetHalfWordAt(memory, address, (ushort)value) },
            { typeof(uint), (byte[] memory, uint address, IConvertible value) => SetWordAt(memory, address, (uint)value) }
        };

        private static readonly Dictionary<Type, Func<byte[], uint, IConvertible>> GetData =
            new Dictionary<Type, Func<byte[], uint, IConvertible>>
        {
            { typeof(byte), (byte[] memory, uint address) => memory[address] },
            { typeof(ushort), (byte[] memory, uint address) => GetHalfWordAt(memory, address) },
            { typeof(uint), (byte[] memory, uint address) => GetWordAt(memory, address) }
        };

        private static ushort GetHalfWordAt(byte[] memory, uint address)
        {
            // assumes memory address does not wrap!
            return (ushort)((memory[address + 1] << 8) | memory[address]);
        }

        private static void SetHalfWordAt(byte[] memory, uint address, ushort value)
        {
            // assumes memory address does not wrap!
            memory[address + 1] = (byte)((value & 0xff00) >> 8);
            memory[address] = (byte)(value & 0x00ff);
        }

        private static uint GetWordAt(byte[] memory, uint address)
        {
            // assumes memory address does not wrap!
            return (uint)(
                    (memory[address + 3] << 24) |
                    (memory[address + 2] << 16) |
                    (memory[address + 1] << 8) |
                    (memory[address])
                    );
        }

        private static void SetWordAt(byte[] memory, uint address, uint value)
        {
            // assumes memory address does not wrap!
            memory[address + 3] = (byte)((value & 0xff00_0000) >> 24);
            memory[address + 2] = (byte)((value & 0x00ff_0000) >> 16);
            memory[address + 1] = (byte)((value & 0x0000_ff00) >> 8);
            memory[address] = (byte)(value & 0x0000_00ff);
        }

    }
}
