﻿using System;

using GBAEmulator.Bus;

namespace GBAEmulator.Memory
{
    partial class MEM
    {

        #region DISPCNT
        public class cDISPCNT : IORegister2
        {
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
        }

        public readonly cDISPCNT DISPCNT = new cDISPCNT();
        #endregion

        #region DISPSTAT
        public class cDISPSTAT : IORegister2
        {
            MEM mem;

            public cDISPSTAT(MEM mem) : base()
            {
                this.mem = mem;
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
                        this.mem.IF.Request(Interrupt.LCDVBlank);
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
                    if (this.IsSet(DISPSTATFlags.HBlankIRQEnable))
                        this.mem.IF.Request(Interrupt.LCDHBlank);
                }
                else
                    this._raw &= 0xfffd;
            }
        }

        public readonly cDISPSTAT DISPSTAT;
        #endregion

        #region VCOUNT
        public class cVCOUNT : IORegister2
        {
            MEM mem;
            public cVCOUNT(MEM mem) : base()
            {
                this.mem = mem;
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
                    if (value == this.mem.DISPSTAT.VCountSetting)
                    {
                        this.mem.DISPSTAT.VCountMatch(true);
                        if (this.mem.DISPSTAT.IsSet(DISPSTATFlags.VCounterIRQEnable))
                        {
                            this.mem.IF.Request(Interrupt.LCDVCountMatch);
                        }
                    }
                    else
                    {
                        this.mem.DISPSTAT.VCountMatch(false);
                    }
                }
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                this.mem.Error("Cannot write to VCOUNT register");
            }
        }

        public readonly cVCOUNT VCOUNT;
        #endregion

        #region BGControl

        public class cBGControl : IORegister2
        {
            // BG0/1 have bit 13 unused
            private readonly ushort BitMask;

            public cBGControl(ushort BitMask) : base()
            {
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
                base.Set((ushort)(value & BitMask), setlow, sethigh);
            }
        }

        public readonly cBGControl[] BGCNT = new cBGControl[4] {
            new cBGControl(0xdfff), new cBGControl(0xdfff), new cBGControl(0xffff), new cBGControl(0xffff)
        };
        #endregion

        #region BGScrolling
        public class cBGScrolling : WriteOnlyRegister2
        {
            public cBGScrolling(BUS bus, bool IsLower) : base(bus, IsLower) { }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                base.Set((ushort)(value & 0x01ff), setlow, sethigh);
            }

            public ushort Offset
            {
                get => (ushort)(this._raw & 0x01ff);  // 9 bit value
            }
        }

        public readonly cBGScrolling[] BGHOFS;
        public readonly cBGScrolling[] BGVOFS;
        #endregion

        #region BG Rotation/Scaling
        public class cReferencePointHalf : WriteOnlyRegister2
        {
            private cReferencePoint parent;
            private ushort BitMask;

            public cReferencePointHalf(cReferencePoint parent, BUS bus, bool IsLower, ushort BitMask) : base(bus, IsLower)
            {
                this.parent = parent;
                this.BitMask = BitMask;
            }

            public ushort raw
            {
                get => this._raw;
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                base.Set((ushort)(value & this.BitMask), setlow, sethigh);
                this.parent.ResetInternal();
            }
        }

        public class cReferencePoint : IORegister4<cReferencePointHalf>
        {
            public cReferencePoint(BUS bus)
            {
                this.lower = new cReferencePointHalf(this, bus, true, 0xffff);
                // top 4 bits unused
                this.upper = new cReferencePointHalf(this, bus, false, 0x0fff);
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
        
        public readonly cReferencePoint BG2X;   
        public readonly cReferencePoint BG2Y;   
        public readonly cReferencePoint BG3X;
        public readonly cReferencePoint BG3Y;   

        public readonly cRotationScaling BG2PA;
        public readonly cRotationScaling BG2PB;
        public readonly cRotationScaling BG2PC;
        public readonly cRotationScaling BG2PD;

        public readonly cRotationScaling BG3PA;
        public readonly cRotationScaling BG3PB;
        public readonly cRotationScaling BG3PC;
        public readonly cRotationScaling BG3PD;
        #endregion

        #region Window Feature
        public class cWindowDimensions : WriteOnlyRegister2
        {
            public cWindowDimensions(BUS bus, bool IsLower) : base(bus, IsLower) { }

            public byte HighCoord
            {
                get => (byte)(this._raw & 0x00ff);
            }

            public byte LowCoord
            {
                get => (byte)(this._raw >> 8);
            }
        }

        public cWindowDimensions[] WINH;
        public cWindowDimensions[] WINV;

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

        public cWindowControl WININ = new cWindowControl();
        public cWindowControl WINOUT = new cWindowControl();
        #endregion

        #region Mosaic Function
        public class cMosaic : IORegister2
        {
            public byte BGMosaicHSize
            {
                get => (byte)((this._raw & 0x000f) + 1);
            }

            public byte BGMosaicVSize
            {
                get => (byte)(((this._raw & 0x00f0) >> 4) + 1);
            }

            public byte OBJMosaicHSize
            {
                get => (byte)(((this._raw & 0x0f00) >> 8) + 1);
            }

            public byte OBJMosaicVSize
            {
                get => (byte)(((this._raw & 0xf000) >> 12) + 1);
            }
        }

        public cMosaic MOSAIC = new cMosaic();
        #endregion

        #region Color Special Effects
        public class cBLDCNT : IORegister2
        {
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
                // top 2 bits unused
                base.Set((ushort)(value & 0x3fff), setlow, sethigh);
            }
        }

        public cBLDCNT BLDCNT = new cBLDCNT();

        public class cBLDALPHA : IORegister2
        {
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
                base.Set((ushort)(value & 0x1f1f), setlow, sethigh);
            }
        }

        public cBLDALPHA BLDALPHA = new cBLDALPHA();

        public class cBLDY : IORegister2
        {
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
                base.Set((ushort)(value & 0x001f), setlow, sethigh);
            }
        }

        public cBLDY BLDY = new cBLDY();
        #endregion
    }
}