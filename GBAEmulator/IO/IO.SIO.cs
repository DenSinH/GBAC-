using System;

namespace GBAEmulator.IO
{
    #region RCNT
    public class cRCNT : IORegister2
    {
        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xc0ff), setlow, sethigh);
        }
    }
    #endregion

    #region SIOCNT
    public class cSIOCNT : IORegister2
    {
        private readonly cIF IF;

        public cSIOCNT(cIF IF)
        {
            this.IF = IF;
        }

        public bool ShiftClock => (this._raw & 0x0001) > 0;

        public bool InternalShiftClock => (this._raw & 0x0002) > 0;

        public bool SIState => (this._raw & 0x0004) > 0;

        public bool InactiveSOState => (this._raw & 0x0008) > 0;

        public bool Active => (this._raw & 0x0100) > 0;

        public bool TransferLength => (this._raw & 0x1000) > 0;  // 32bit (true) / 8bit (false)

        public bool IRQEnable => (this._raw & 0x4000) > 0;

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x7f0f), setlow, sethigh);

            // cheese it
            if (this.IRQEnable)
                this.IF.Request(Interrupt.SerialCommunication);
        }
    }
    #endregion

    #region SIODATA
    public class cSIODATA8 : IORegister2
    {
        public byte Data => (byte)(this._raw & 0x00ff);

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x00ff), setlow, sethigh);
        }
    }

    public class cSIODATA32Half : IORegister2
    {
        
    }

    public class cSIODATA32 : IORegister4<cSIODATA32Half>
    {
        public cSIODATA32() : base(new cSIODATA32Half(), new cSIODATA32Half()) { }

        public uint Data => (uint)((this.upper.Get() << 16) | this.lower.Get());
    }
    #endregion
}
