using GBAEmulator.CPU;
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Memory
{
    interface IMemorySection
    {
        public byte? GetByteAt(uint address);

        public ushort? GetHalfWordAt(uint address);

        public uint? GetWordAt(uint address);

        public void SetByteAt(uint address, byte value);

        public void SetHalfWordAt(uint address, ushort value);

        public void SetWordAt(uint address, uint value);
    }

    public abstract class MemorySection : IMemorySection
    {
        // Base class for a non-register memory section
        // assumes all accesses are in bounds
        protected readonly byte[] Storage;

        public MemorySection(uint Size)
        {
            Storage = new byte[Size];
        }
        
        public virtual byte? GetByteAt(uint address)
        {
            return Storage[address];
        }

        public virtual ushort? GetHalfWordAt(uint address)
        {
            return (ushort)((Storage[address] << 8) | Storage[address + 1]); 
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
            Storage[address]     = (byte) (value & 0x0000_00ff);
        }
    }

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

    public class cBIOSSection : ReadOnlyMemorySection
    {
        private ARM7TDMI cpu;
        public cBIOSSection(ARM7TDMI cpu) : base(0x4000)
        {
            this.cpu = cpu;
        }

        public override byte? GetByteAt(uint address)
        {
            if (this.cpu.PC < 0x0100_0000)
                return base.GetByteAt(address);
            return Storage[(uint)this.cpu.mem.CurrentBIOSReadState];
        }

        public override ushort? GetHalfWordAt(uint address)
        {
            if (this.cpu.PC < 0x0100_0000)
                return base.GetHalfWordAt(address);
            return Storage[(uint)this.cpu.mem.CurrentBIOSReadState];
        }

        public override uint? GetWordAt(uint address)
        {
            if (this.cpu.PC < 0x0100_0000)
                return base.GetWordAt(address);
            return Storage[(uint)this.cpu.mem.CurrentBIOSReadState];
        }
    }

    public class cROMSection : ReadOnlyMemorySection
    {
        private MEM mem;
        private bool IsUpper;
        public cROMSection(MEM mem, bool IsUpper) : base(0x0100_0000)
        {
            this.mem = mem;
            this.IsUpper = IsUpper;
        }

        private byte? TryEEPROMAccess(uint address)
        {
            if (!this.IsUpper) return null;

            if (this.mem.ROMBackupType == MEM.BackupType.EEPROM)
            {
                if ((address > 0x00ff_feff) ||
                    (this.mem.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                {
                    // EEPROM access, might as well call a read directly
                    // the interface we are using wants an argument, so we just pass it 0xffff_ffff to signify that it does not matter
                    return this.mem.BackupRead(0xffff_ffff);
                }
            }
            return null;
        }

        public override byte? GetByteAt(uint address)
        {
            
            return this.TryEEPROMAccess(address) ?? base.GetByteAt(address);
        }

        public override ushort? GetHalfWordAt(uint address)
        {
            return this.TryEEPROMAccess(address) ?? base.GetHalfWordAt(address);
        }

        public override uint? GetWordAt(uint address)
        {
            return this.TryEEPROMAccess(address) ?? base.GetWordAt(address);
        }

        public void Load(byte[] data, uint offset)
        {
            for (uint i = 0; i < Storage.Length; i++)
            {
                if (offset + i > data.Length) return;
                this.Storage[i] = data[offset + i];
            }
        }
    }

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

    public class NonMirroredMemorySection : MemorySection
    {
        public NonMirroredMemorySection(uint Size) : base(Size)
        {

        }

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

    public class cBackupSection : IMemorySection
    {
        MEM mem;
        public cBackupSection(MEM mem)
        {
            this.mem = mem;
        }

        public byte? GetByteAt(uint address)
        {
            return this.mem.BackupRead(address & 0xffff);
        }

        public ushort? GetHalfWordAt(uint address)
        {
            return this.mem.BackupRead(address & 0xffff);
        }

        public uint? GetWordAt(uint address)
        {
            return this.mem.BackupRead(address & 0xffff);
        }

        public void SetByteAt(uint address, byte value)
        {
            this.mem.BackupWrite(address & 0xffff, value);
        }

        public void SetHalfWordAt(uint address, ushort value)
        {
            this.mem.BackupWrite(address & 0xffff, (byte)value);
        }

        public void SetWordAt(uint address, uint value)
        {
            this.mem.BackupWrite(address & 0xffff, (byte)value);
        }
    }

    partial class MEM
    {
        private cBIOSSection BIOSSection;
        private NonMirroredMemorySection UnusedSection = new NonMirroredMemorySection(0);
        private MirroredMemorySection eWRAMSection = new MirroredMemorySection(0x40000);
        private MirroredMemorySection iWRAMSection = new MirroredMemorySection(0x8000);
        public cIORAM IORAMSection;
        private MirroredMemorySection PaletteRAMSection = new MirroredMemorySection(0x8000);
        private cVRAMSection VRAMSection = new cVRAMSection();
        private MirroredMemorySection OAMSection = new MirroredMemorySection(0x400);
        private cROMSection GamePakSection_L;
        private cROMSection GamePakSection_H;
        private cBackupSection BackupSection;

        private IMemorySection[] MemorySections;

        public uint GetWordAt(uint address, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return this.bus.OpenBus();
            return this.MemorySections[Section].GetWordAt(address) ?? this.bus.OpenBus();
        }
        public ushort GetHalfWordAt(uint address, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return (ushort)this.bus.OpenBus();
            return this.MemorySections[Section].GetHalfWordAt(address) ?? (ushort)this.bus.OpenBus();
        }
        public byte GetByteAt(uint address)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return (byte)this.bus.OpenBus();
            return this.MemorySections[Section].GetByteAt(address) ?? (byte)this.bus.OpenBus();
        }

        public void SetWordAt(uint address, uint value, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.MemorySections[Section].SetWordAt(address, value);
        }
        public void SetHalfWordAt(uint address, ushort value, uint offset=0)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.MemorySections[Section].SetHalfWordAt(address, value);
        }
        public void SetByteAt(uint address, byte value)
        {
            uint Section = (address & 0xff00_0000) >> 24;
            if (Section > 15) return;
            this.MemorySections[Section].SetByteAt(address, value);
        }
    }
}
