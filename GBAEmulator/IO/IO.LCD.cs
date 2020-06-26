using System;

using GBAEmulator.Bus;
using GBAEmulator.Video;

namespace GBAEmulator.IO
{

    #region DISPCNT
    public class cDISPCNT : IORegister2
    {
        private readonly PPU ppu;
        public cDISPCNT(PPU ppu)
        {
            this.ppu = ppu;
        }

        public byte BGMode
        {
            get => (byte)(this._raw & 0x07);
        }

        public bool IsSet(DISPCNTFlags flag) => (this._raw & (ushort)flag) > 0;

        public bool DisplayBG(byte BG)
        {
            return (this._raw & (0x0100 << BG)) > 0;
        }

        public bool DisplayBGWindow(byte Window)
        {
            if (Window == 0) return (this._raw & 0x2000) > 0;
            return (this._raw & 0x4000) > 0;
        }

        public bool DisplayOBJWindow()
        {
            return (this._raw & 0x8000) > 0;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            this.ppu.Wait();
            base.Set(value, setlow, sethigh);
        }
    }
    #endregion

    #region DISPSTAT
    public class cDISPSTAT : IORegister2
    {
        private readonly cIF IF;
        private readonly PPU ppu;

        public cDISPSTAT(cIF IF, PPU ppu) : base()
        {
            this.IF = IF;
            this.ppu = ppu;
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

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            this.ppu.Wait();
            base.Set(value, setlow, sethigh);
        }
    }
    #endregion

    #region VCOUNT
    public class cVCOUNT : IORegister2
    {
        private readonly cIF IF;
        private readonly cDISPSTAT DISPSTAT;

        public cVCOUNT(cIF IF, cDISPSTAT DISPSTAT) : base()
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
            // this.mem.Error("Cannot write to VCOUNT register");
        }
    }
    #endregion

    #region BGControl
    public class cBGControl : IORegister2
    {
        // BG0/1 have bit 13 unused
        private readonly PPU ppu;
        private readonly ushort BitMask;

        public cBGControl(PPU ppu, ushort BitMask) : base()
        {
            this.ppu = ppu;
            this.BitMask = BitMask;
        }

        public byte BGPriority
        {
            get => (byte)(this._raw & 0x03);
        }

        public byte CharBaseBlock
        {
            get => (byte)((this._raw & 0x0c) >> 2);
        }

        public bool Mosaic
        {
            get => (this._raw & 0x40) > 0;
        }

        public bool ColorMode
        {
            get => (this._raw & 0x80) > 0;
        }

        public byte ScreenBaseBlock
        {
            get => (byte)((this._raw & 0x1f00) >> 8);
        }

        public bool DisplayAreaOverflow
        {
            // not used for BG0 and BG1
            get => (this._raw & 0x2000) > 0;
        }

        public byte ScreenSize
        {
            get => (byte)((this._raw & 0xc000) >> 14);
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // todo: only when necessary
            this.ppu.Wait();
            base.Set((ushort)(value & BitMask), setlow, sethigh);
        }
    }
    #endregion

    #region BGScrolling
    public class cBGScrolling : WriteOnlyRegister2
    {
        private readonly PPU ppu;
        public cBGScrolling(PPU ppu, BUS bus, bool IsLower) : base(bus, IsLower)
        {
            this.ppu = ppu;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // todo: only when necessary
            this.ppu.Wait();
            base.Set((ushort)(value & 0x01ff), setlow, sethigh);
        }

        public ushort Offset
        {
            get => (ushort)(this._raw & 0x01ff);  // 9 bit value
        }
    }
    #endregion

    #region BG Rotation/Scaling
    public class cReferencePointHalf : WriteOnlyRegister2
    {
        private readonly PPU ppu;
        private cReferencePoint parent;
        private ushort BitMask;

        public cReferencePointHalf(cReferencePoint parent, PPU ppu, BUS bus, bool IsLower, ushort BitMask) : base(bus, IsLower)
        {
            this.ppu = ppu;
            this.parent = parent;
            this.BitMask = BitMask;
        }

        public ushort raw
        {
            get => this._raw;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // todo: only when necessary
            this.ppu.Wait();
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
            get => (this.upper.raw & 0x0800) > 0;
        }

        public int Full
        {
            get
            {
                if (this.Sign)  // negative
                    return (int)(this.lower.raw | ((this.upper.raw & 0x07ff) << 16) | 0xf800_0000);
                return (this.lower.raw | ((this.upper.raw & 0x07ff) << 16));
            }
        }
    }

    public class cRotationScaling : WriteOnlyRegister2
    {
        public cRotationScaling(BUS bus, bool IsLower) : base(bus, IsLower) { }

        public short Full
        {
            get
            {
                return (short)this._raw;
            }
        }
    }
    #endregion

    #region Window Feature
    public class cWindowDimensions : WriteOnlyRegister2
    {
        private readonly PPU ppu;
        public cWindowDimensions(PPU ppu, BUS bus, bool IsLower) : base(bus, IsLower)
        {
            this.ppu = ppu;
        }

        public byte HighCoord
        {
            get => (byte)(this._raw & 0x00ff);
        }

        public byte LowCoord
        {
            get => (byte)(this._raw >> 8);
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            this.ppu.Wait();
            base.Set(value, setlow, sethigh);
        }
    }

    public class cWindowControl : IORegister2
    {
        public bool WindowBGEnable(Window window, byte BG)
        {
            if ((byte)window == 0)
                return (this._raw & (0x0001 << BG)) > 0;
            else
                return (this._raw & (0x0100 << BG)) > 0;
        }

        public bool WindowOBJEnable(Window window)
        {
            if ((byte)window == 0)
                return (this._raw & 0x0010) > 0;
            else
                return (this._raw & 0x1000) > 0;
        }

        public bool WindowSpecialEffects(Window window)
        {
            if ((byte)window == 0)
                return (this._raw & 0x0020) > 0;
            else
                return (this._raw & 0x2000) > 0;
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
    public class cMosaic : IORegister2
    {
        private readonly PPU ppu;
        public cMosaic(PPU ppu)
        {
            this.ppu = ppu;
        }

        public byte BGMosaicHStretch
        {
            get => (byte)((this._raw & 0x000f) + 1);
        }

        public byte BGMosaicVStretch
        {
            get => (byte)(((this._raw & 0x00f0) >> 4) + 1);
        }

        public byte OBJMosaicHStretch
        {
            get => (byte)(((this._raw & 0x0f00) >> 8) + 1);
        }

        public byte OBJMosaicVStretch
        {
            get => (byte)(((this._raw & 0xf000) >> 12) + 1);
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            this.ppu.Wait();
            base.Set(value, setlow, sethigh);
        }
    }
    #endregion

    #region Color Special Effects
    public class cBLDCNT : IORegister2
    {
        private readonly PPU ppu;

        public cBLDCNT(PPU ppu)
        {
            this.ppu = ppu;
        }

        public bool BGIsTop(byte BG)
        {
            return (this._raw & (1 << BG)) > 0;
        }

        public bool OBJIsTop() => (this._raw & 0x10) > 0;

        public bool BDIsTop() => (this._raw & 0x20) > 0;

        public BlendMode BlendMode
        {
            get => (BlendMode)((this._raw & 0xc0) >> 6);
        }

        public bool BGIsBottom(byte BG)
        {
            return (this._raw & (0x100 << BG)) > 0;
        }

        public bool OBJIsBottom() => (this._raw & 0x1000) > 0;

        public bool BDIsBottom() => (this._raw & 0x2000) > 0;

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // todo: only when necessary
            this.ppu.Wait();
            // top 2 bits unused
            base.Set((ushort)(value & 0x3fff), setlow, sethigh);
        }
    }

    public class cBLDALPHA : IORegister2
    {
        private readonly PPU ppu;

        public cBLDALPHA(PPU ppu)
        {
            this.ppu = ppu;
        }

        public byte EVA
        {
            // allow up to 0x10 (1.4 fixed point)
            get
            {
                byte value = (byte)(this._raw & 0x001f);
                return (byte)(value > 0x10 ? 0x10 : value);
            }
        }

        public byte EVB
        {
            // allow up to 0x10 (1.4 fixed point)
            get
            {
                byte value = (byte)((this._raw & 0x1f00) >> 8);
                return (byte)(value > 0x10 ? 0x10 : value);
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // todo: only when necessary
            this.ppu.Wait();
            base.Set((ushort)(value & 0x1f1f), setlow, sethigh);
        }
    }

    public class cBLDY : IORegister2
    {
        private readonly PPU ppu;
        public cBLDY(PPU ppu)
        {
            this.ppu = ppu;
        }

        public byte EY
        {
            // allow up to 0x10 (1.4 fixed point)
            get
            {
                byte value = (byte)(this._raw & 0x1f);
                return (byte)(value > 0x10 ? 0x10 : value);
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            // todo: only when necessary
            this.ppu.Wait();
            base.Set((ushort)(value & 0x001f), setlow, sethigh);
        }
    }
    #endregion
}
