using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Memory.Sections
{
    public class MirroredMemorySection : MemorySection
    {
        uint BitMask;
        public MirroredMemorySection(uint Size) : base(Size)
        {
            BitMask = Size - 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte? GetByteAt(uint address) => base.GetByteAt(address & BitMask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort? GetHalfWordAt(uint address) => base.GetHalfWordAt(address & BitMask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint? GetWordAt(uint address) => base.GetWordAt(address & BitMask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetByteAt(uint address, byte value) => base.SetByteAt(address & BitMask, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetHalfWordAt(uint address, ushort value) => base.SetHalfWordAt(address & BitMask, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetWordAt(uint address, uint value) => base.SetWordAt(address & BitMask, value);
    }
}
