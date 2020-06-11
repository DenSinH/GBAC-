using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Memory.Sections
{
    interface IMemorySection
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte? GetByteAt(uint address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort? GetHalfWordAt(uint address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint? GetWordAt(uint address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetByteAt(uint address, byte value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetHalfWordAt(uint address, ushort value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWordAt(uint address, uint value);
    }
}
