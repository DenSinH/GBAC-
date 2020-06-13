using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private uint PreviousAddress;

        private void Update32bitAccessCPUCycles(uint section)
        {
            this.cpu.NCycle = WordAccessCycles[section];
            this.cpu.SCycle = WordAccessCycles[section];
        }
        private void Update8bitAccessCPUCycles(uint section)
        {
            this.cpu.NCycle = NonWordAccessCycles[section];
            this.cpu.SCycle = NonWordAccessCycles[section];
        }

        private int GetWordAccessCycles(uint section, uint address)
        {
            int cycles = 1;

            if (section < 8)
            {
                // no waitstates for other regions
                cycles = WordAccessCycles[section];
            }
            else if (section < 16)
            {
                // word access, so sequential would be offset by 4 bytes
                if (address - this.PreviousAddress == 4)
                {
                    // S access
                    cycles = 1 + this.IORAM.WAITCNT.GetWaitStateSCycles(section);
                }
                else
                {
                    // N access
                    cycles = 1 + this.IORAM.WAITCNT.GetWaitStateNCycles(section);
                }
                // GBATek:
                // GamePak uses 16bit data bus, so that a 32bit access is split into TWO 16bit accesses 
                // (of which, the second fragment is always sequential, even if the first fragment was non-sequential).
                cycles += this.IORAM.WAITCNT.GetWaitStateSCycles(section);
            }
            // else: out of bounds! (how many cycles?)

            this.PreviousAddress = address;
            return cycles;
        }

        private int GetNonWordAccessCycles(uint section, uint address)
        {
            int cycles = 1;

            if (section < 8)
            {
                // no waitstates for other regions
                cycles = WordAccessCycles[section];
            }
            else if (section < 16)
            {
                // word access, so sequential would be offset by 4 bytes
                if (address - this.PreviousAddress == 2 || address - this.PreviousAddress == 1)
                {
                    // S access
                    cycles = 1 + this.IORAM.WAITCNT.GetWaitStateSCycles(section);
                }
                else
                {
                    // N access
                    cycles = 1 + this.IORAM.WAITCNT.GetWaitStateNCycles(section);
                }
            }
            // else: out of bounds! (how many cycles?)

            this.PreviousAddress = address;
            return cycles;
        }

        public uint GetWordAt(uint address)
        {
            // offset is handled in individual sections (always force align, except SRAM
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetWordAccessCycles(Section, address);
            if (Section > 15) return this.bus.OpenBus();

            this.Update32bitAccessCPUCycles(Section);
            return this.bus.BusValue = (this.MemorySections[Section].GetWordAt(address) ?? this.bus.OpenBus());
        }

        public ushort GetHalfWordAt(uint address)
        {
            // offset is handled in individual sections (always force align, except SRAM
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15) return (ushort)this.bus.OpenBus();

            this.Update32bitAccessCPUCycles(Section);
            ushort value = this.MemorySections[Section].GetHalfWordAt(address) ?? (ushort)this.bus.OpenBus();
            this.bus.BusValue = value;
            return value;
        }

        public byte GetByteAt(uint address)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15) return (byte)this.bus.OpenBus();
            this.Update8bitAccessCPUCycles(Section);

            byte value = this.MemorySections[Section].GetByteAt(address) ?? (byte)this.bus.OpenBus();
            this.bus.BusValue = value;
            return value;
        }

        public void SetWordAt(uint address, uint value)
        {
            // offset is handled in individual sections (always force align, except SRAM
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetWordAccessCycles(Section, address);
            if (Section > 15) return;

            this.Update8bitAccessCPUCycles(Section);
            this.MemorySections[Section].SetWordAt(address, value);
        }

        public void SetHalfWordAt(uint address, ushort value)
        {
            // offset is handled in individual sections (always force align, except SRAM
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15) return;

            this.Update8bitAccessCPUCycles(Section);
            this.MemorySections[Section].SetHalfWordAt(address, value);
        }

        public void SetByteAt(uint address, byte value)
        {
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15) return;

            this.Update8bitAccessCPUCycles(Section);
            this.MemorySections[Section].SetByteAt(address, value);
        }
    }
}
