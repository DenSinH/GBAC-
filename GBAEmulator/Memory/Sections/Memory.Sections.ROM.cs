using System;
using System.Runtime.CompilerServices;


namespace GBAEmulator.Memory.Sections
{
    public class cROMSection : ReadOnlyMemorySection
    {
        private MEM mem;
        private bool IsUpper;
        public cROMSection(MEM mem, bool IsUpper) : base(0x0100_0000)
        {
            this.mem = mem;
            this.IsUpper = IsUpper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte? TryEEPROMAccess(uint address)
        {
            if (!this.IsUpper) return null;

            if (this.mem.Backup.ROMBackupType == BackupType.EEPROM)
            {
                if ((address > 0x00ff_feff) ||
                    (this.mem.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                {
                    // EEPROM access, might as well call a read directly
                    // the interface we are using wants an argument, so we just pass it 0xffff_ffff to signify that it does not matter
                    return this.mem.Backup.GetByteAt(0xffff_ffff);
                }
            }
            return null;
        }

        public override byte? GetByteAt(uint address)
        {
            if (this.IsUpper) return this.TryEEPROMAccess(address) ?? base.GetByteAt(address);
            return base.GetByteAt(address);
        }

        public override ushort? GetHalfWordAt(uint address)
        {
            if (this.IsUpper) return this.TryEEPROMAccess(address) ?? base.GetHalfWordAt(address);
            return base.GetHalfWordAt(address);
        }

        public override uint? GetWordAt(uint address)
        {
            if (this.IsUpper) return this.TryEEPROMAccess(address) ?? base.GetWordAt(address);
            return base.GetWordAt(address);
        }

        public void Load(byte[] data, uint offset)
        {
            for (uint i = 0; i < Storage.Length; i++)
            {
                if (offset + i >= data.Length) return;
                this.Storage[i] = data[offset + i];
            }
        }
    }

}
