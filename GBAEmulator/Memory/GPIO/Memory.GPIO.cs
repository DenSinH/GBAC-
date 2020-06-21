using System;

namespace GBAEmulator.Memory.GPIO
{
    public class GPIO
    {
        public enum Chip
        {
            Empty,
            RTC
        }

        private IGPIOChip GPIOChip;
        private bool AllowRead;
        private byte ReadMask;

        public GPIO(Chip chip)
        {
            switch (chip)
            {
                default:
                    this.GPIOChip = new GPIOEmpty();
                    break;
            }
        }

        public byte? GetData()
        {
            /*
             80000C4h - I/O Port Data (selectable W or R/W)
                  bit0-3  Data Bits 0..3 (0=Low, 1=High)
                  bit4-15 not used (0)
            */
            // Console.WriteLine("GPIO Get Data");
            if (!AllowRead)
                return null;
            return this.GPIOChip.Read();
        }

        public void SetData(byte value)
        {
            // Console.WriteLine("GPIO Set Data " + value.ToString("x4"));
            this.GPIOChip.Write(value);
        }

        public byte? GetDirection()
        {
            /*
             80000C6h - I/O Port Direction (for above Data Port) (selectable W or R/W)
                  bit0-3  Direction for Data Port Bits 0..3 (0=In, 1=Out)
                  bit4-15 not used (0)
            */
            // Console.WriteLine("GPIO Get Direction");
            if (!AllowRead)
                return null;
            return ReadMask;
        }

        public void SetDirection(byte value)
        {
            // Console.WriteLine("GPIO Set Direction " + value.ToString("x4"));
            ReadMask = (byte)(value & 0x000f);
        }

        public byte? GetControl()
        {
            /*
             80000C8h - I/O Port Control (selectable W or R/W)
                  bit0    Register 80000C4h..80000C8h Control (0=Write-Only, 1=Read/Write)
                  bit1-15 not used (0)
            */
            // Console.WriteLine("GPIO Get Control");
            if (!AllowRead)
                return null;
            return (byte)(AllowRead ? 1 : 0);
        }

        public void SetControl(ushort value)
        {
            // Console.WriteLine("GPIO Set Control " + value.ToString("x4"));
            AllowRead = (value & 1) > 0;
        }
    }
}
