using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Sections
{
    public class MirroredMemorySection : MemorySection
    {
        uint BitMask;
        public MirroredMemorySection(uint Size) : base(Size)
        {
            BitMask = Size - 1;
        }

        public override byte? GetByteAt(uint address) => base.GetByteAt(address & BitMask);

        public override ushort? GetHalfWordAt(uint address) => base.GetHalfWordAt(address & BitMask);

        public override uint? GetWordAt(uint address) => base.GetWordAt(address & BitMask);

        public override void SetByteAt(uint address, byte value) => base.SetByteAt(address & BitMask, value);

        public override void SetHalfWordAt(uint address, ushort value) => base.SetHalfWordAt(address & BitMask, value);

        public override void SetWordAt(uint address, uint value) => base.SetWordAt(address & BitMask, value);
    }
}
