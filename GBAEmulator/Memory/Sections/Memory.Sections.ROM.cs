using System;
using System.Runtime.CompilerServices;

using GBAEmulator.Memory.GPIO;


namespace GBAEmulator.Memory.Sections
{
    public class cROMSection : MemorySection
    {
        private MEM mem;
        public GPIO.GPIO gpio;
        public uint ROMSize;
        private bool IsUpper;
        public cROMSection(MEM mem, bool IsUpper) : base(0x0100_0000)
        {
            this.mem = mem;
            this.IsUpper = IsUpper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte? TryEEPROMRead(uint address)
        {
            if (!this.IsUpper) return null;

            if (this.mem.Backup.ROMBackupType == BackupType.EEPROM)
            {
                if (((address & 0x00ff_ffff) > 0x00ff_feff) ||
                    (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                {
                    // EEPROM access, might as well call a read directly
                    // the interface we are using wants an argument, so we just pass it 0xffff_ffff to signify that it does not matter
                    return this.mem.Backup.GetByteAt(0xffff_ffff);
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryEEPROMWrite(uint address, byte value)
        {
            if (!this.IsUpper) return;

            if (this.mem.Backup.ROMBackupType == BackupType.EEPROM)
            {
                if (((address & 0x00ff_ffff) > 0x00ff_feff) ||
                    (this.ROMSize <= 0x0100_0000 && address >= 0x0d00_0000 && address < 0x0e00_0000))
                {
                    // EEPROM access, might as well call a read directly
                    // the interface we are using wants an argument, so we just pass it 0xffff_ffff to signify that it does not matter
                    this.mem.Backup.SetByteAt(0xffff_ffff, value);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte? TryGPIORead(uint address)
        {
            switch (address)
            {
                case 0x0800_00c4:
                    return this.gpio.GetData();
                case 0x0800_00c6:
                    return this.gpio.GetDirection();
                case 0x0800_00c8:
                    return this.gpio.GetControl();
                default:
                    return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryGPIOWrite(uint address, byte value)
        {
            switch (address)
            {
                case 0x0800_00c4:
                    this.gpio.SetData(value);
                    return;
                case 0x0800_00c6:
                    this.gpio.SetDirection(value);
                    return;
                case 0x0800_00c8:
                    this.gpio.SetControl(value);
                    return;
                default:
                    return;
            }
        }

        public override byte? GetByteAt(uint address)
        {
            byte? value;
            if (this.IsUpper)
            {
                value = this.TryEEPROMRead(address);
                if (value != null) return value;

                if ((address &= 0x01ff_ffff) > this.ROMSize)
                    return (byte)((address >> 1) & 0xff);
            }
            else if (address <= 0x0800_00c8 && address >= 0x0800_00c4)
            {
                value = this.TryGPIORead(address);
                if (value != null) return value;
            }
            return base.GetByteAt(address);
        }

        public override ushort? GetHalfWordAt(uint address)
        {
            ushort? value;
            if (this.IsUpper)
            {
                value = this.TryEEPROMRead(address);
                if (value != null) return value;

                if ((address &= 0x01ff_ffff) > this.ROMSize)
                {
                    return (ushort)((address >> 1) & 0xffff);
                }
            }
            else if (address <= 0x0800_00c8 && address >= 0x0800_00c4)
            {
                value = this.TryGPIORead(address);
                if (value != null) return value;
            }
            return base.GetHalfWordAt(address);
        }

        public override uint? GetWordAt(uint address)
        {
            byte? value;
            if (this.IsUpper)
            {
                value = this.TryEEPROMRead(address);
                if (value != null) return value;

                if ((address &= 0x01ff_ffff) > this.ROMSize)
                {
                    return ((address >> 1) & 0xfffe) | ((((address >> 1) & 0xfffe) + 1) << 16);
                }
            }
            else if (address <= 0x0800_00c8 && address >= 0x0800_00c4)  // more likely to be over 0x0800_00c8, so faster check this way
            {
                value = this.TryGPIORead(address);
                if (value != null) return value;
            }
            return base.GetWordAt(address);
        }

        public override void SetByteAt(uint address, byte value)
        {
            if (this.IsUpper)
                this.TryEEPROMWrite(address, value);
            else
                this.TryGPIOWrite(address, value);
        }

        public override void SetHalfWordAt(uint address, ushort value)
        {
            if (this.IsUpper)
                this.TryEEPROMWrite(address, (byte)value);
            else
                this.TryGPIOWrite(address, (byte)value);
        }

        public override void SetWordAt(uint address, uint value)
        {
            if (this.IsUpper)
                this.TryEEPROMWrite(address, (byte)value);
            else
                this.TryGPIOWrite(address, (byte)value);
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
