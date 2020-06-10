using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Memory.Sections
{
    public class cVRAMSection : MemorySection
    {
        public cVRAMSection() : base(0x18000) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint MaskAddress(uint address)
        {
            if ((address & 0x1ffff) < 0x10000)
                return address & 0xffff;
            return 0x10000 | (address & 0x7fff);
        }

        public override byte? GetByteAt(uint address) => base.GetByteAt(MaskAddress(address));

        public override ushort? GetHalfWordAt(uint address) => base.GetHalfWordAt(MaskAddress(address));

        public override uint? GetWordAt(uint address) => base.GetWordAt(MaskAddress(address));

        public override void SetByteAt(uint address, byte value) => base.SetByteAt(MaskAddress(address), value);

        public override void SetHalfWordAt(uint address, ushort value) => base.SetHalfWordAt(MaskAddress(address), value);

        public override void SetWordAt(uint address, uint value) => base.SetWordAt(MaskAddress(address), value);
    }

}
