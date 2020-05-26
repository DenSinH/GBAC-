using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private class EmptyRegister : IORegister2 { }  // basically default register (name might be a bit misleading)

        #region DISPCNT
        [Flags]
        public enum DISPCNTFlags : ushort
        {
            CGBMode = 0x0008,
            DPFrameSelect = 0x0010,
            HBLankIntervalFree = 0x0020,
            OBJVRamMapping = 0x0040,
            ForcedBlank = 0x0080,

            DisplayOBJ = 0x1000,
            WindowDisplay0 = 0x2000,
            WindowDisplay1 = 0x4000,
            OBJWindowDisplay = 0x8000,
        }

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
        }

        public readonly cDISPCNT DISPCNT = new cDISPCNT();  // 0x0400_0004
        #endregion

        #region DISPSTAT

        [Flags]
        public enum DISPSTATFlags : ushort
        {
            VBlankFlag = 0x0001,
            HBlankFlag = 0x0002,
            VCounterFlag = 0x0004,
            VBlankIRQEnable = 0x0008,
            HBlankIRQEnable = 0x0010,
            VCounterIRQEnable = 0x0020,
        }

        public class cDISPSTAT : IORegister2
        {
            ARM7TDMI cpu;

            public cDISPSTAT(ARM7TDMI cpu) : base()
            {
                this.cpu = cpu;
            }
            
            public byte VCountSetting
            {
                get => (byte)((this._raw & 0xff00) >> 8);
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
                        this.cpu.IF.Request(Interrupt.LCDVBlank);
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
                        this.cpu.IF.Request(Interrupt.LCDHBlank);
                }
                else
                    this._raw &= 0xfffd;
            }
        }

        public cDISPSTAT DISPSTAT;
        #endregion

        #region VCOUNT
        public class cVCOUNT : IORegister2
        {
            ARM7TDMI cpu;
            public cVCOUNT(ARM7TDMI cpu) : base()
            {
                this.cpu = cpu;
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
                    if (this.cpu.DISPSTAT.IsSet(DISPSTATFlags.VCounterIRQEnable) && (value == this.cpu.DISPSTAT.VCountSetting))
                        this.cpu.IF.Request(Interrupt.LCDVCountMatch);
                }
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                this.cpu.Error("Cannot write to VCOUNT register");
            }
        }

        public cVCOUNT VCOUNT;
        #endregion

        #region BGControl

        public class cBGControl : IORegister2
        {
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
        }

        public readonly cBGControl[] BGCNT = new cBGControl[4] { new cBGControl(), new cBGControl(), new cBGControl(), new cBGControl() };
        #endregion

        #region BGScrolling
        public class cBGScrolling : IORegister2
        {
            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                if (setlow)
                {
                    base.Set((ushort)(value & 0x00ff), setlow, sethigh);
                }
            }

            public ushort Offset
            {
                get => (ushort)(this._raw & 0x01ff);  // 9 bit value
            }
        }

        public readonly cBGScrolling[] BGHOFS = 
            new cBGScrolling[4] { new cBGScrolling(), new cBGScrolling(), new cBGScrolling(), new cBGScrolling() };
        public readonly cBGScrolling[] BGVOFS = 
            new cBGScrolling[4] { new cBGScrolling(), new cBGScrolling(), new cBGScrolling(), new cBGScrolling() };
        #endregion

        #region LCD I/O BG Rotation/Scaling
        public class cReferencePointHalf : IORegister2
        {
            cReferencePoint parent;
            
            public cReferencePointHalf(cReferencePoint parent)
            {
                this.parent = parent;
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                base.Set(value, setlow, sethigh);
                this.parent.ResetInternal();
            }
        }

        public class cReferencePoint : IORegister4<cReferencePointHalf>
        {
            public cReferencePoint()
            {
                this.lower = new cReferencePointHalf(this);
                this.upper = new cReferencePointHalf(this);
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

            public byte FractionalPortion
            {
                get => (byte)(this.lower.Get() & 0x00ff);
            }

            public uint IntegerPortion
            {
                get => (uint)(((this.lower.Get() & 0xff00) >> 8) | ((this.upper.Get() & 0x07ff) << 8));
            }

            public bool Sign
            {
                get => (this.upper.Get() & 0x0800) > 0;
            }

            public int Full
            {
                get
                {
                    if (this.Sign)  // negative
                        return (int)(this.lower.Get() | ((this.upper.Get() & 0x07ff) << 16) | 0xf800_0000);
                    return (this.lower.Get() | ((this.upper.Get() & 0x07ff) << 16));
                }
            }
        }

        public class cRotationScaling : IORegister2
        {
            public byte FractionalPortion
            {
                get => (byte)(this._raw & 0x00ff);
            }

            public byte IntegerPortion
            {
                get => (byte)((this._raw & 0x7f00) >> 8);
            }

            public bool Sign
            {
                get => (this._raw & 0x8000) > 0;
            }

            public short Full
            {
                get
                {
                    return (short)this._raw;
                }
            }
        }

        public readonly cReferencePoint BG2X = new cReferencePoint();
        public readonly cReferencePoint BG2Y = new cReferencePoint();
        public readonly cReferencePoint BG3X = new cReferencePoint();
        public readonly cReferencePoint BG3Y = new cReferencePoint();

        public readonly cRotationScaling BG2PA = new cRotationScaling();
        public readonly cRotationScaling BG2PB = new cRotationScaling();
        public readonly cRotationScaling BG2PC = new cRotationScaling();
        public readonly cRotationScaling BG2PD = new cRotationScaling();

        public readonly cRotationScaling BG3PA = new cRotationScaling();
        public readonly cRotationScaling BG3PB = new cRotationScaling();
        public readonly cRotationScaling BG3PC = new cRotationScaling();
        public readonly cRotationScaling BG3PD = new cRotationScaling();
        #endregion

        #region KEYINPUT
        private class cKeyInput : IORegister2
        {
            Controller controller = new XInputController();
            cKeyInterruptControl KEYCNT;
            ARM7TDMI cpu;

            public cKeyInput(cKeyInterruptControl KEYCNT, ARM7TDMI cpu)
            {
                this.KEYCNT = KEYCNT;
                this.cpu = cpu;

                try
                {
                    this.controller.PollKeysPressed();
                }
                catch (SharpDX.SharpDXException)
                {
                    this.controller = new KeyboardController();
                }
            }

            public override ushort Get()
            {
                ushort state = this.controller.PollKeysPressed();
                if (this.KEYCNT.IRQEnable)
                {
                    if (this.KEYCNT.IRQCondition)   // AND
                    {
                        if ((state & this.KEYCNT.Mask) == this.KEYCNT.Mask)
                            this.cpu.IF.Request(Interrupt.Keypad);
                    }
                    else                            // OR
                    {
                        if ((state & this.KEYCNT.Mask) > 0)
                            this.cpu.IF.Request(Interrupt.Keypad);
                    }
                }

                return (ushort)~state;
            }

            public override void Set(ushort value, bool setlow, bool sethigh) { }
        }

        private class cKeyInterruptControl : IORegister2
        {
            public ushort Mask
            {
                get => (ushort)(this._raw & 0x03ff);
            }

            public bool IRQEnable
            {
                get => (this._raw & 0x4000) > 0;
            }

            public bool IRQCondition
            {
                get => (this._raw & 0x8000) > 0;
            }
        }
        #endregion

        #region Interrupt Control
        public class cIME : IORegister2  // Interrupt Master Enable
        {
            public bool DisableAll
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

            public ushort raw
            {
                get => (ushort)(this._raw & 0x3fff);
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                // clear written bits
                if (setlow)
                    this._raw = (ushort)(this._raw & ~(value & 0x00ff));
                else if (sethigh)
                    this._raw = (ushort)(this._raw & ~(value & 0xff00));
            }
        }

        readonly cIME IME = new cIME();
        readonly cIE IE = new cIE();
        readonly cIF IF = new cIF();

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

        readonly cPOSTFLG_HALTCNT HALTCNT = new cPOSTFLG_HALTCNT();

        #endregion
    }
}
