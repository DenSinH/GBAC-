using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Sections
{
    class UnusedMemorySection : IMemorySection
    {

        public byte? GetByteAt(uint address)
        {
            return null;
        }

        public ushort? GetHalfWordAt(uint address)
        {
            return null;
        }

        public uint? GetWordAt(uint address)
        {
            return null;
        }

        public void SetByteAt(uint address, byte value)
        {
            
        }

        public void SetHalfWordAt(uint address, ushort value)
        {
            
        }

        public void SetWordAt(uint address, uint value)
        {
            
        }
    }
}
