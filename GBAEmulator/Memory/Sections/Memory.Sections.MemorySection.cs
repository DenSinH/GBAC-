using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Sections
{
    public abstract class MemorySection : IMemorySection
    {
        // Base class for a non-register memory section
        // assumes all accesses are in bounds
        protected readonly byte[] Storage;

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

        public virtual byte? GetByteAt(uint address)
        {
            return Storage[address];
        }

        public virtual ushort? GetHalfWordAt(uint address)
        {
            return (ushort)((Storage[address + 1] << 8) | Storage[address]);
        }

        public virtual uint? GetWordAt(uint address)
        {
            return (uint)(
                    (Storage[address + 3] << 24) |
                    (Storage[address + 2] << 16) |
                    (Storage[address + 1] << 8) |
                    (Storage[address])
                    );
        }

        public virtual void SetByteAt(uint address, byte value)
        {
            Storage[address] = value;
        }

        public virtual void SetHalfWordAt(uint address, ushort value)
        {
            Storage[address + 1] = (byte)((value & 0xff00) >> 8);
            Storage[address] = (byte)(value & 0x00ff);
        }

        public virtual void SetWordAt(uint address, uint value)
        {
            Storage[address + 3] = (byte)((value & 0xff00_0000) >> 24);
            Storage[address + 2] = (byte)((value & 0x00ff_0000) >> 16);
            Storage[address + 1] = (byte)((value & 0x0000_ff00) >> 8);
            Storage[address] = (byte)(value & 0x0000_00ff);
        }
    }
}
