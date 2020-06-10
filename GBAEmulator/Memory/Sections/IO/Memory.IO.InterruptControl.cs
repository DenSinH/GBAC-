using System;

namespace GBAEmulator.Memory.IO
{
    #region Interrupt Control
    public class cIME : IORegister2  // Interrupt Master Enable
    {
        public bool Enabled
        {
            get => (this._raw & 0x01) > 0;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            if (setlow)
                this._raw = (ushort)(value & 1);
        }
    }

    public class cIE : IORegister2  // Interrupt Enable Register
    {
        public ushort raw
        {
            get => (ushort)(this._raw & 0x3fff);  // upper 2 bits are irrelevant
        }
    }

    public class cIF : IORegister2  // Interrupt Request / IRQ Acknowledge
    {
        public void Request(Interrupt request)
        {
            this._raw |= (ushort)request;
        }

        public void Request(ushort request)
        {
            this._raw |= request;
        }

        public ushort raw
        {
            get => (ushort)(this._raw & 0x3fff);
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // clear written bits
            if (setlow)
                this._raw = (ushort)(this._raw & ~(value & 0x00ff));
            if (sethigh)
                this._raw = (ushort)(this._raw & ~(value & 0xff00));
        }
    }
    #endregion

    #region HALTCNT
    public class cPOSTFLG_HALTCNT : IORegister2
    {
        // 2 1 byte registers combined
        public bool Halt;

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);
            if (sethigh)
            {
                // "games never enable stop mode" - EmuDev Discord
                Halt = true;
            }
        }
    }
    #endregion
}
