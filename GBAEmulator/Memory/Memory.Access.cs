using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private uint PreviousAddress;
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
                // The GBA forcefully uses non-sequential timing at the beginning of each 128K-block of gamepak ROM,
                // eg. "LDMIA [801fff8h],r0-r7" will have non-sequential timing at 8020000h.
                if (address - this.PreviousAddress == 4 && (address & 0x1_ffff) != 0)
                {
                    // S access
                    cycles = 1 + this.IO.WAITCNT.GetWaitStateSCycles(section);
                }
                else
                {
                    // N access
                    cycles = 1 + this.IO.WAITCNT.GetWaitStateNCycles(section);
                }
                // GBATek:
                // GamePak uses 16bit data bus, so that a 32bit access is split into TWO 16bit accesses 
                // (of which, the second fragment is always sequential, even if the first fragment was non-sequential).
                cycles += 1 + this.IO.WAITCNT.GetWaitStateSCycles(section);
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
                cycles = NonWordAccessCycles[section];
            }
            else if (section < 16)
            {
                // non-word access, so sequential would be offset by 2 or 1 bytes
                if ((address - this.PreviousAddress == 2 || address - this.PreviousAddress == 1) && (address & 0x1_ffff) != 0)
                {
                    // S access
                    cycles = 1 + this.IO.WAITCNT.GetWaitStateSCycles(section);
                }
                else
                {
                    // N access
                    cycles = 1 + this.IO.WAITCNT.GetWaitStateNCycles(section);
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
            if (Section > 15)
            {
                return this.bus.OpenBus();
            }

            return this.bus.BusValue = (this.MemorySections[Section].GetWordAt(address) ?? this.bus.OpenBus());
        }

        public ushort GetHalfWordAt(uint address)
        {
            // offset is handled in individual sections (always force align, except SRAM
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15)
            {
                return (ushort)this.bus.OpenBus();
            }

            ushort value = this.MemorySections[Section].GetHalfWordAt(address) ?? (ushort)this.bus.OpenBus();

            uint BusMask = ((address & 3) > 1) ? 0x0000_ffff : 0xffff_0000;
            this.bus.BusValue = (this.bus.BusValue & BusMask) | (uint)(value << (((address & 3) > 1) ? 16 : 0));
            return value;
        }

        public byte GetByteAt(uint address)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15)
            {
                return (byte)this.bus.OpenBus();
            }

            byte value = this.MemorySections[Section].GetByteAt(address) ?? (byte)this.bus.OpenBus();

            uint BusMask = (uint)(0x0000_00ff << (int)(8*(address & 3)));
            this.bus.BusValue = (this.bus.BusValue & BusMask) | (uint)(value << (int)(8 * (address & 3)));
            return value;
        }

        public void SetWordAt(uint address, uint value)
        {
            // offset is handled in individual sections (always force align, except SRAM
            this.bus.BusValue = value;
            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetWordAccessCycles(Section, address);
            if (Section > 15) return;

            this.MemorySections[Section].SetWordAt(address, value);
        }

        public void SetHalfWordAt(uint address, ushort value)
        {
            // offset is handled in individual sections (always force align, except SRAM
            uint BusMask = ((address & 3) > 1) ? 0x0000_ffff : 0xffff_0000;
            this.bus.BusValue = (this.bus.BusValue & BusMask) | (uint)(value << (((address & 3) > 1) ? 16 : 0));

            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15) return;

            this.MemorySections[Section].SetHalfWordAt(address, value);
        }

        public void SetByteAt(uint address, byte value)
        {
            uint BusMask = (uint)(0x0000_00ff << (int)(8 * (address & 3)));
            this.bus.BusValue = (this.bus.BusValue & BusMask) | (uint)(value << (int)(8 * (address & 3)));

            uint Section = (address & 0xff00_0000) >> 24;
            this.cpu.InstructionCycles += this.GetNonWordAccessCycles(Section, address);
            if (Section > 15) return;

            this.MemorySections[Section].SetByteAt(address, value);
        }
    }
}
