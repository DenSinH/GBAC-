namespace GBAEmulator.Memory
{
    partial class MEM
    {
        public uint GetWordAt(uint address, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return this.bus.OpenBus();
            return this.MemorySections[Section].GetWordAt(address & 0x00ff_ffff) ?? this.bus.OpenBus();
        }

        public ushort GetHalfWordAt(uint address, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return (ushort)this.bus.OpenBus();
            return this.MemorySections[Section].GetHalfWordAt(address & 0x00ff_ffff) ?? (ushort)this.bus.OpenBus();
        }

        public byte GetByteAt(uint address)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return (byte)this.bus.OpenBus();
            return this.MemorySections[Section].GetByteAt(address & 0x00ff_ffff) ?? (byte)this.bus.OpenBus();
        }

        public void SetWordAt(uint address, uint value, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.MemorySections[Section].SetWordAt(address & 0x00ff_ffff, value);
        }

        public void SetHalfWordAt(uint address, ushort value, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.MemorySections[Section].SetHalfWordAt(address & 0x00ff_ffff, value);
        }

        public void SetByteAt(uint address, byte value)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.MemorySections[Section].SetByteAt(address & 0x00ff_ffff, value);
        }
    }
}
