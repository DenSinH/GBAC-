using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Sections
{
    public class ReadOnlyMemorySection : NonMirroredMemorySection
    {
        // all readonly sections are not mirrored anyway
        public ReadOnlyMemorySection(uint Size) : base(Size) { }

        public override void SetByteAt(uint address, byte value) { }

        public override void SetHalfWordAt(uint address, ushort value) { }

        public override void SetWordAt(uint address, uint value) { }

        public virtual void Load(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                this.Storage[i] = data[i];
            }
        }
    }

}
