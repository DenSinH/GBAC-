﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GBAEmulator.Memory.Sections
{
    public abstract class MemorySection : IMemorySection
    {
        // Base class for a non-register memory section
        // assumes all accesses are in bounds
        protected readonly byte[] Storage;
        const uint AddressMask = 0x00ff_ffff;

        public MemorySection(uint Size)
        {
            Storage = new byte[Size];
        }

        // allow direct access
        public byte this[long address]
        {
            get => this.Storage[address];
            set => this.Storage[address] = value; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte? GetByteAt(uint address)
        {
            return Storage[address & AddressMask];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ushort? GetHalfWordAt(uint address)
        {
            address &= AddressMask & 0x00ff_fffe;  // force align
            return (ushort)((Storage[address + 1] << 8) | Storage[address]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual uint? GetWordAt(uint address)
        {
            address &= AddressMask & 0x00ff_fffc;  // force align
            return (uint)(
                    (Storage[address + 3] << 24) |
                    (Storage[address + 2] << 16) |
                    (Storage[address + 1] << 8) |
                    (Storage[address])
                    );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetByteAt(uint address, byte value)
        {
            address &= AddressMask;
            Storage[address] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetHalfWordAt(uint address, ushort value)
        {
            address &= AddressMask & 0x00ff_fffe;  // force align
            Storage[address + 1] = (byte)((value & 0xff00) >> 8);
            Storage[address] = (byte)(value & 0x00ff);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetWordAt(uint address, uint value)
        {
            address &= AddressMask & 0x00ff_fffc;  // force align
            Storage[address + 3] = (byte)((value & 0xff00_0000) >> 24);
            Storage[address + 2] = (byte)((value & 0x00ff_0000) >> 16);
            Storage[address + 1] = (byte)((value & 0x0000_ff00) >> 8);
            Storage[address] = (byte)(value & 0x0000_00ff);
        }
    }
}
