using System;

using GBAEmulator.Bus;
using GBAEmulator.Video;

namespace GBAEmulator.IO
{
    #region base
    public class LCDRegister2 : IORegister2
    {
        private readonly PPU ppu;
        protected ushort _PPUraw;

        public LCDRegister2(PPU ppu)
        {
            this.ppu = ppu;
        }

#if !THREADED_RENDERING
        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);
            this._PPUraw = this._raw;
        }
#endif

        public void UpdatePPU()
        {
            this._PPUraw = this._raw;
        }
    }

    public class WriteOnlyLCDRegister2 : LCDRegister2
    {
        private readonly BUS bus;
        private readonly bool IsLower;

        public WriteOnlyLCDRegister2(PPU ppu, BUS bus, bool IsLower) : base(ppu)
        {
            this.bus = bus;
            this.IsLower = IsLower;
        }

        public override ushort Get()
        {
            return (ushort)this.bus.OpenBus();
        }
    }
#endregion

#region DISPCNT
    public class cDISPCNT : LCDRegister2
    {
        public cDISPCNT(PPU ppu) : base(ppu)
        {

        }

        public byte BGMode
        {
            get => (byte)(this._PPUraw & 0x07);
        }

        public bool IsSet(DISPCNTFlags flag) => (this._PPUraw & (ushort)flag) > 0;

        public bool DisplayBG(byte BG)
        {
            return (this._PPUraw & (0x0100 << BG)) > 0;
        }

        public bool DisplayBGWindow(byte Window)
        {
            if (Window == 0) return (this._PPUraw & 0x2000) > 0;
            return (this._PPUraw & 0x4000) > 0;
        }

        public bool DisplayOBJWindow()
        {
            return (this._PPUraw & 0x8000) > 0;
        }
    }
#endregion

#region DISPSTAT
    public class cDISPSTAT : LCDRegister2
    {
        private readonly cIF IF;

        public cDISPSTAT(cIF IF, PPU ppu) : base(ppu)
        {
            this.IF = IF;
        }

        public byte VCountSetting
        {
            get => (byte)((this._raw & 0xff00) >> 8);
        }

        public void VCountMatch(bool match)
        {
            if (match) this._raw |= 0x0004;
            else this._raw &= 0xfffb;
        }

        public bool IsSet(DISPSTATFlags flag)
        {
            return (this._raw & (ushort)flag) == (ushort)flag;
        }

        public void SetVBlank(bool on)
        {
            if (on)
            {
                this._raw |= 1;
                if (this.IsSet(DISPSTATFlags.VBlankIRQEnable))
                {
                    this.IF.Request(Interrupt.LCDVBlank);
                }
            }
            else
                this._raw &= 0xfffe;
        }

        public void SetHBlank(bool on)
        {
            if (on)
            {
                this._raw |= 2;
            }
            else
                this._raw &= 0xfffd;
        }
    }
#endregion

#region VCOUNT
    public class cVCOUNT : LCDRegister2
    {
        private readonly cIF IF;
        private readonly cDISPSTAT DISPSTAT;

        public cVCOUNT(cIF IF, cDISPSTAT DISPSTAT) : base(null)
        {
            this.IF = IF;
            this.DISPSTAT = DISPSTAT;
        }

        public byte CurrentScanline
        {
            get
            {
                return (byte)(this._raw & 0x00ff);
            }
            set
            {
                this._raw = (ushort)((this._raw & 0xff00) | value);
                if (value == this.DISPSTAT.VCountSetting)
                {
                    this.DISPSTAT.VCountMatch(true);
                    if (this.DISPSTAT.IsSet(DISPSTATFlags.VCounterIRQEnable))
                    {
                        this.IF.Request(Interrupt.LCDVCountMatch);
                    }
                }
                else
                {
                    this.DISPSTAT.VCountMatch(false);
                }
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            
        }
    }
#endregion

#region BGControl
    public class cBGControl : LCDRegister2
    {
        // BG0/1 have bit 13 unused
        private readonly ushort BitMask;

        public cBGControl(PPU ppu, ushort BitMask) : base(ppu)
        {
            this.BitMask = BitMask;
        }

        public byte BGPriority
        {
            get => (byte)(this._PPUraw & 0x03);
        }

        public byte CharBaseBlock
        {
            get => (byte)((this._PPUraw & 0x0c) >> 2);
        }

        public bool Mosaic
        {
            get => (this._PPUraw & 0x40) > 0;
        }

        public bool ColorMode
        {
            get => (this._PPUraw & 0x80) > 0;
        }

        public byte ScreenBaseBlock
        {
            get => (byte)((this._PPUraw & 0x1f00) >> 8);
        }

        public bool DisplayAreaOverflow
        {
            // not used for BG0 and BG1
            get => (this._PPUraw & 0x2000) > 0;
        }

        public byte ScreenSize
        {
            get => (byte)((this._PPUraw & 0xc000) >> 14);
        }
    }
#endregion

#region BGScrolling
    public class cBGScrolling : WriteOnlyLCDRegister2
    {
        public cBGScrolling(PPU ppu, BUS bus, bool IsLower) : base(ppu, bus, IsLower)
        {

        }

        public ushort Offset
        {
            get => (ushort)(this._PPUraw & 0x01ff);  // 9 bit value
        }
    }
#endregion

#region BG Rotation/Scaling
    public class cReferencePointHalf : WriteOnlyLCDRegister2
    {
        private cReferencePoint parent;
        private ushort BitMask;

        public cReferencePointHalf(cReferencePoint parent, PPU ppu, BUS bus, bool IsLower, ushort BitMask) : base(ppu, bus, IsLower)
        {
            this.parent = parent;
            this.BitMask = BitMask;
        }

        public ushort PPUraw
        {
            get => this._PPUraw;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & this.BitMask), setlow, sethigh);
            this.parent.ResetInternal();
        }
    }

    public class cReferencePoint : IORegister4<cReferencePointHalf>
    {
        public cReferencePoint(PPU ppu, BUS bus)
        {
            this.lower = new cReferencePointHalf(this, ppu, bus, true, 0xffff);
            // top 4 bits unused
            this.upper = new cReferencePointHalf(this, ppu, bus, false, 0x0fff);
        }

        public uint InternalRegister { get; private set; }

        public void ResetInternal()
        {
            this.InternalRegister = (uint)this.Full;
        }

        public void UpdateInternal(uint dm_)  // dmx/dmy
        {
            this.InternalRegister += dm_;
        }

        public bool Sign
        {
            get => (this.upper.PPUraw & 0x0800) > 0;
        }

        public int Full
        {
            get
            {
                if (this.Sign)  // negative
                    return (int)(this.lower.PPUraw | ((this.upper.PPUraw & 0x07ff) << 16) | 0xf800_0000);
                return (this.lower.PPUraw | ((this.upper.PPUraw & 0x07ff) << 16));
            }
        }
    }

    public class cRotationScaling : LCDRegister2  // + write only
    {
        private readonly BUS bus;
        private readonly bool IsLower;
        public cRotationScaling(BUS bus, bool IsLower) : base(null)
        {
            this.bus = bus;
            this.IsLower = IsLower;
        }

        public short Full
        {
            get
            {
                return (short)this._PPUraw;
            }
        }

        public override ushort Get()
        {
            return (ushort)this.bus.OpenBus();
        }
    }
#endregion

#region Window Feature
    public class cWindowDimensions : WriteOnlyLCDRegister2
    {
        private readonly PPU ppu;
        public cWindowDimensions(PPU ppu, BUS bus, bool IsLower) : base(ppu, bus, IsLower)
        {
            this.ppu = ppu;
        }

        public byte HighCoord
        {
            get => (byte)(this._PPUraw & 0x00ff);
        }

        public byte LowCoord
        {
            get => (byte)(this._PPUraw >> 8);
        }
    }

    public class cWindowControl : LCDRegister2
    {
        public cWindowControl() : base(null)
        {

        }

        public bool WindowBGEnable(Window window, byte BG)
        {
            if ((byte)window == 0)
                return (this._PPUraw & (0x0001 << BG)) > 0;
            else
                return (this._PPUraw & (0x0100 << BG)) > 0;
        }

        public bool WindowOBJEnable(Window window)
        {
            if ((byte)window == 0)
                return (this._PPUraw & 0x0010) > 0;
            else
                return (this._PPUraw & 0x1000) > 0;
        }

        public bool WindowSpecialEffects(Window window)
        {
            if ((byte)window == 0)
                return (this._PPUraw & 0x0020) > 0;
            else
                return (this._PPUraw & 0x2000) > 0;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // top 2 bits unused
            base.Set((ushort)(value & 0x3fff), setlow, sethigh);
        }

        public override ushort Get()
        {
            // bits 5,6, 14, 15 unused
            return (ushort)(0x3f3f & this._raw);
        }
    }
#endregion

#region Mosaic Function
    public class cMosaic : LCDRegister2
    {
        public cMosaic(PPU ppu) : base(ppu)
        {

        }

        public byte BGMosaicHStretch
        {
            get => (byte)((this._PPUraw & 0x000f) + 1);
        }

        public byte BGMosaicVStretch
        {
            get => (byte)(((this._PPUraw & 0x00f0) >> 4) + 1);
        }

        public byte OBJMosaicHStretch
        {
            get => (byte)(((this._PPUraw & 0x0f00) >> 8) + 1);
        }

        public byte OBJMosaicVStretch
        {
            get => (byte)(((this._PPUraw & 0xf000) >> 12) + 1);
        }
    }
#endregion

#region Color Special Effects
    public class cBLDCNT : LCDRegister2
    {
        public cBLDCNT(PPU ppu) : base(ppu)
        {

        }

        public bool BGIsTop(byte BG)
        {
            return (this._PPUraw & (1 << BG)) > 0;
        }

        public bool OBJIsTop() => (this._PPUraw & 0x10) > 0;

        public bool BDIsTop() => (this._PPUraw & 0x20) > 0;

        public BlendMode BlendMode
        {
            get => (BlendMode)((this._PPUraw & 0xc0) >> 6);
        }

        public bool BGIsBottom(byte BG)
        {
            return (this._PPUraw & (0x100 << BG)) > 0;
        }

        public bool OBJIsBottom() => (this._PPUraw & 0x1000) > 0;

        public bool BDIsBottom() => (this._PPUraw & 0x2000) > 0;

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // top 2 bits unused
            base.Set((ushort)(value & 0x3fff), setlow, sethigh);
        }
    }

    public class cBLDALPHA : LCDRegister2
    {
        public cBLDALPHA(PPU ppu) : base(ppu)
        {

        }

        public byte EVA
        {
            // allow up to 0x10 (1.4 fixed point)
            get
            {
                byte value = (byte)(this._PPUraw & 0x001f);
                return (byte)(value > 0x10 ? 0x10 : value);
            }
        }

        public byte EVB
        {
            // allow up to 0x10 (1.4 fixed point)
            get
            {
                byte value = (byte)((this._PPUraw & 0x1f00) >> 8);
                return (byte)(value > 0x10 ? 0x10 : value);
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x1f1f), setlow, sethigh);
        }
    }

    public class cBLDY : LCDRegister2
    {
        public cBLDY(PPU ppu) : base(ppu)
        {

        }

        public byte EY
        {
            // allow up to 0x10 (1.4 fixed point)
            get
            {
                byte value = (byte)(this._PPUraw & 0x1f);
                return (byte)(value > 0x10 ? 0x10 : value);
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0x001f), setlow, sethigh);
        }
    }
#endregion
}
