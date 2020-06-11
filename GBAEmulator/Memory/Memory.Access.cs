namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private void Update32bitAccessCPUCycles(uint Section)
        {
            this.cpu.NCycle = WordAccessSCycles[Section];
            this.cpu.SCycle = WordAccessSCycles[Section];
        }

        private void Update8bitAccessCPUCycles(uint Section)
        {
            this.cpu.NCycle = ByteAccessSCycles[Section];
            this.cpu.SCycle = WordAccessSCycles[Section];
        }

        public uint GetWordAt(uint address, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return this.bus.OpenBus();
            this.Update32bitAccessCPUCycles(Section);
            return this.bus.BusValue = (this.MemorySections[Section].GetWordAt(address) ?? this.bus.OpenBus());
        }

        public ushort GetHalfWordAt(uint address, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return (ushort)this.bus.OpenBus();
            this.Update32bitAccessCPUCycles(Section);
            ushort value = this.MemorySections[Section].GetHalfWordAt(address) ?? (ushort)this.bus.OpenBus();
            this.bus.BusValue = value;
            return value;
        }

        public byte GetByteAt(uint address)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return (byte)this.bus.OpenBus();
            this.Update8bitAccessCPUCycles(Section);
            byte value = this.MemorySections[Section].GetByteAt(address) ?? (byte)this.bus.OpenBus();
            this.bus.BusValue = value;
            return value;
        }

        public void SetWordAt(uint address, uint value, uint offset=0)
        {
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.Update8bitAccessCPUCycles(Section);
            this.MemorySections[Section].SetWordAt(address, value);
        }

        public void SetHalfWordAt(uint address, ushort value, uint offset=0)
        {
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.Update8bitAccessCPUCycles(Section);
            this.MemorySections[Section].SetHalfWordAt(address, value);
        }

        public void SetByteAt(uint address, byte value)
        {
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.Update8bitAccessCPUCycles(Section);
            this.MemorySections[Section].SetByteAt(address, value);
        }
    }
}
