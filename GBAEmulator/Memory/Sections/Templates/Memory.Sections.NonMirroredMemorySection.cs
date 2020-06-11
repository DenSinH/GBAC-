using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Sections
{
    public class NonMirroredMemorySection : MemorySection
    {
        public NonMirroredMemorySection(uint Size) : base(Size) { }

        public override byte? GetByteAt(uint address)
        {
            if (address < this.Storage.Length) return base.GetByteAt(address);
            return null;
        }

        public override ushort? GetHalfWordAt(uint address)
        {
            if (address < this.Storage.Length) return base.GetHalfWordAt(address);
            return null;
        }

        public override uint? GetWordAt(uint address)
        {
            if (address < this.Storage.Length) return base.GetWordAt(address);
            return null;
        }

        public override void SetByteAt(uint address, byte value)
        {
            if (address < this.Storage.Length) base.SetByteAt(address, value);
        }

        public override void SetHalfWordAt(uint address, ushort value)
        {
            if (address < this.Storage.Length) base.SetHalfWordAt(address, value);
        }

        public override void SetWordAt(uint address, uint value)
        {
            if (address < this.Storage.Length) base.SetWordAt(address, value);
        }
    }

}
