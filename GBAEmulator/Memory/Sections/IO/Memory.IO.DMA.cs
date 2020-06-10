using System;

using GBAEmulator.Bus;

namespace GBAEmulator.Memory.IO
{
    #region DMA Transfers
    public class cDMAAddressHalf : WriteOnlyRegister2
    {
        private ushort BitMask;
        public ushort InternalRegister;

        public cDMAAddressHalf(BUS bus, bool IsLower, ushort BitMask) : base(bus, IsLower)
        {
            this.BitMask = BitMask;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & this.BitMask), setlow, sethigh);
        }

        public void Reload()
        {
            this.InternalRegister = this._raw;
        }
    }

    public class cDMAAddress : IORegister4<cDMAAddressHalf>
    {
        private bool InternalMemory;

        public cDMAAddress(BUS bus, bool InternalMemory) : base(
            new cDMAAddressHalf(bus, true, 0xffff),
            new cDMAAddressHalf(bus, false, (ushort)(InternalMemory ? 0x07ff : 0xffff)))
        {
            this.InternalMemory = InternalMemory;
        }

        public uint Address
        {
            // todo: separate based on memory section
            get => (uint)(this.upper.InternalRegister << 16 | this.lower.InternalRegister);
            set
            {
                // todo: can upper overflow (upper 4/5 bits unused)
                this.upper.InternalRegister = (ushort)(value >> 16);
                this.lower.InternalRegister = (ushort)(value & 0xffff);
            }
        }

        public void Reload()
        {
            this.lower.Reload();
            this.upper.Reload();
        }
    }

    public class cDMACNT_L : WriteOnlyRegister2
    {
        private ushort BitMask;
        private ushort InternalRegister;

        public cDMACNT_L(BUS bus, ushort BitMask) : base(bus, true)
        {
            this.BitMask = BitMask;
        }

        public ushort UnitCount
        {
            // a value of zero is treated as max length (ie. 4000h, or 10000h for DMA3).
            // the bitmask is simply   max length - 1
            get => (ushort)((this.InternalRegister == 0) ? (this.BitMask + 1) : this.InternalRegister);
            set => this.InternalRegister = value;
        }

        public bool Empty
        {
            get => this.InternalRegister == 0;
        }

        public void Reload()
        {
            this.InternalRegister = this._raw;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & this.BitMask), setlow, sethigh);
        }
    }

    public class cDMACNT_H : IORegister2
    {
        private bool AllowGamePakDRQ;
        public bool Active;

        private MEM mem;
        public readonly int index;

        public cDMACNT_H(MEM mem, int index) : base()
        {
            this.index = index;
            this.mem = mem;
        }

        public cDMACNT_H(MEM mem, int index, bool AllowGamePakDRQ) : this(mem, index)
        {
            this.AllowGamePakDRQ = AllowGamePakDRQ;
        }

        public AddrControl DestAddrControl
        {
            get => (AddrControl)((this._raw & 0x0060) >> 5);
        }

        public AddrControl SourceAddrControl
        {
            get => (AddrControl)((this._raw & 0x0180) >> 7);
        }

        public bool DMARepeat
        {
            get => (this._raw & 0x0200) > 0;
        }

        public bool DMATransferType
        {
            // (0=16bit, 1=32bit)
            get => (this._raw & 0x0400) > 0;
        }

        public bool GamePakDRQ
        {
            // (0=Normal, 1=DRQ <from> Game Pak, DMA3 only)
            get => this.AllowGamePakDRQ && (this._raw & 0x0800) > 0;
        }

        public DMAStartTiming StartTiming
        {
            get => (DMAStartTiming)((this._raw & 0x3000) >> 12);
        }

        public bool IRQOnEnd
        {
            get => (this._raw & 0x4000) > 0;
        }

        public bool DMAEnabled
        {
            get => (this._raw & 0x8000) > 0;
        }

        public void Disable()
        {
            this._raw &= 0x7fff;
        }

        public void Trigger(DMAStartTiming timing)
        {
            if ((this._raw & 0x8000) > 0)  // enabled
            {
                if (timing == this.StartTiming)
                {
                    this.Active = true;
                }
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            bool DoReload = !this.DMAEnabled;

            // bottom 5 bits unused
            base.Set((ushort)(value & 0xfff8), setlow, sethigh);
            if (DoReload && this.DMAEnabled)
            {
                this.mem.IORAM.DMADAD[this.index].Reload();
                this.mem.IORAM.DMASAD[this.index].Reload();
                this.mem.IORAM.DMACNT_L[this.index].Reload();
            }

            if ((this._raw & 0xb000) == 0x8000)  // DMA Enable set AND DMA start timing immediate
            {
                this.Active = true;
            }
        }
    }
    #endregion
}
