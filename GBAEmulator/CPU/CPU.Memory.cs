﻿using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
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
            // Byte: 6; UInt16: 8; UInt32: 10 (worked out pretty well!)
            byte GetDataIndex = (byte)((byte)(Type.GetTypeCode(typeof(T)) - 6) >> 1);
            switch ((address & 0x0f00_0000) >> 6)
            {
                case 0:
                case 1:
                    // BIOS region
                    return (T)GetData[GetDataIndex](this.BIOS, address & 0x3fff);
                case 2:
                    // eWRAM mirrors
                    return (T)GetData[GetDataIndex](this.eWRAM, address & 0x3ffff);
                case 3:
                    // iWRAM mirrors
                    return (T)GetData[GetDataIndex](this.iWRAM, address & 0x7fff);
                case 4:
                    // IORAM mirrors
                    return (T)GetData[GetDataIndex](this.IORAM, address & 0x3ff);
                case 5:
                    // PaletteRAM mirrors
                    return (T)GetData[GetDataIndex](this.PaletteRAM, address & 0x3ff);
                case 6:
                    // VRAM mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        return (T)GetData[GetDataIndex](this.VRAM, address & 0xffff);
                    }
                    return (T)GetData[GetDataIndex](this.VRAM, 0x10000 | (address & 0x7fff));
                case 7:
                    // OAM mirrors
                    return (T)GetData[GetDataIndex](this.OAM, address & 0x3ff);
                case 0xe:
                    // SRAM is not mirrored (?)
                    return (T)GetData[GetDataIndex](this.GamePakSRAM, address & 0xffff);
                case 0xf:
                    throw new IndexOutOfRangeException(string.Format("Index {0:x8} out of bounds for getting CPU Memory", address));
                default:
                    // Other regions are Game Pak at different speeds
                    return (T)GetData[GetDataIndex](this.GamePak, address & 0x1ff_ffff);
            }
        }

        private void SetAt<T>(uint address, T value) where T : IConvertible
        {
            // Byte: 6; UInt16: 8; UInt32: 10 (worked out pretty well!)
            byte SetDataIndex = (byte)((byte)(Type.GetTypeCode(typeof(T)) - 6) >> 1);
            switch ((address & 0x0f00_0000) >> 6)
            {
                case 0:
                case 1:
                    // BIOS region
                    SetData[SetDataIndex](this.BIOS, address & 0x3fff, value);
                    return;
                case 2:
                    // eWRAM mirrors
                    SetData[SetDataIndex](this.eWRAM, address & 0x3ffff, value);
                    return;
                case 3:
                    // iWRAM mirrors
                    SetData[SetDataIndex](this.iWRAM, address & 0x7fff, value);
                    return;
                case 4:
                    // IORAM mirrors
                    SetData[SetDataIndex](this.IORAM, address & 0x3ff, value);
                    return;
                case 5:
                    // PaletteRAM mirrors
                    SetData[SetDataIndex](this.PaletteRAM, address & 0x3ff, value);
                    return;
                case 6:
                    // VRAM mirrors
                    if ((address & 0x1ffff) < 0x10000)
                    {
                        // first bit is already 0
                        SetData[SetDataIndex](this.VRAM, address & 0xffff, value);
                    }
                    else
                    {
                        SetData[SetDataIndex](this.VRAM, 0x10000 | (address & 0x7fff), value);
                    }
                    return;
                case 7:
                    // OAM mirrors
                    SetData[SetDataIndex](this.OAM, address & 0x3ff, value);
                    return;
                case 0xe:
                    // SRAM is not mirrored (?)
                    SetData[SetDataIndex](this.GamePakSRAM, address & 0xffff, value);
                    return;
                case 0xf:
                    throw new IndexOutOfRangeException(string.Format("Index {0:x8} out of bounds for setting CPU Memory", address));
                default:
                    // Other regions are Game Pak at different speeds
                    SetData[SetDataIndex](this.GamePak, address & 0x1ff_ffff, value);
                    return;
            }
        }

        /* Storing the functions that get / set specific data length values from memory */
        private static Action<byte[], uint, IConvertible>[] SetData =
        {
                (byte[] memory, uint address, IConvertible value) => { memory[address] = (byte)value; },
                (byte[] memory, uint address, IConvertible value) => SetHalfWordAt(memory, address, (ushort)value),
                (byte[] memory, uint address, IConvertible value) => SetWordAt(memory, address, (uint)value)
        };

        private static Func<byte[], uint, IConvertible>[] GetData =
        {
                (byte[] memory, uint address) => memory[address],
                (byte[] memory, uint address) => GetHalfWordAt(memory, address),
                (byte[] memory, uint address) => GetWordAt(memory, address)
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