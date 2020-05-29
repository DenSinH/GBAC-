using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private class EmptyRegister : IORegister2 { }  // basically default register (name might be a bit misleading)

        private class UnusedRegister : IORegister
        {
            // Register full of unused bits (always returns 0)
            public ushort Get()
            {
                return 0;
            }

            public void Set(ushort value, bool setlow, bool sethigh) { }
        }

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
                base.Set((ushort)(value & 0x01ff), setlow, sethigh);
            }

            public override ushort Get()
            {
                // Write only
                return 0;
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
            private cReferencePoint parent;
            private ushort BitMask;
            
            public cReferencePointHalf(cReferencePoint parent, ushort BitMask)
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

            public override ushort Get()
            {
                // Write only
                return 0;
            }
        }

        public class cReferencePoint : IORegister4<cReferencePointHalf>
        {
            public cReferencePoint()
            {
                this.lower = new cReferencePointHalf(this, 0xffff);
                // top 4 bits unused
                this.upper = new cReferencePointHalf(this, 0x0fff);
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

        public class cRotationScaling : IORegister2
        {
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

        #region Window Feature
        public class cWindowDimensions : IORegister2
        {
            public byte HighCoord
            {
                get => (byte)(this._raw & 0x00ff);
            }

            public byte LowCoord
            {
                get => (byte)(this._raw >> 8);
            }

            public override ushort Get()
            {
                // Write only
                return 0;
            }
        }

        public cWindowDimensions[] WINH = new cWindowDimensions[2] { new cWindowDimensions(), new cWindowDimensions() };
        public cWindowDimensions[] WINV = new cWindowDimensions[2] { new cWindowDimensions(), new cWindowDimensions() };

        public class cWindowControl : IORegister2
        {
            public bool WindowBGEnable(byte Window, byte BG)
            {
                if (Window == 0)
                    return (this._raw & (1 << BG)) > 0;
                else
                    return (this._raw & (1 << BG)) > 0;
            }

            public bool WindowOBJEnable(byte Window)
            {
                if (Window == 0)
                    return (this._raw & 0x0010) > 0;
                else
                    return (this._raw & 0x1000) > 0;
            }

            public bool WindowSpecialEffects(byte Window)
            {
                if (Window == 0)
                    return (this._raw & 0x0020) > 0;
                else
                    return (this._raw & 0x2000) > 0;
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                // top 2 bits unused
                base.Set((ushort)(value & 0x3fff), setlow, sethigh);
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
                get => (this._raw & 0x01) == 0;
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

        private readonly cIME IME = new cIME();
        private readonly cIE IE = new cIE();
        private readonly cIF IF = new cIF();

        #endregion

        #region HALTCNT
        private class cPOSTFLG_HALTCNT : IORegister2
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

        private readonly cPOSTFLG_HALTCNT HALTCNT = new cPOSTFLG_HALTCNT();
        #endregion

        #region DMA Transfers
        private class cDMAAddressHalf : IORegister2
        {
            private ushort BitMask;
            public ushort InternalRegister;

            public cDMAAddressHalf(ushort BitMask) : base()
            {
                this.BitMask = BitMask;
            }

            public override ushort Get()
            {
                // Write only
                return 0;
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

        private class cDMAAddress : IORegister4<cDMAAddressHalf>
        {
            private bool InternalMemory;

            public cDMAAddress(bool InternalMemory) : base(new cDMAAddressHalf(0xffff), new cDMAAddressHalf((ushort)(InternalMemory ? 0x07ff : 0xffff)))
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

        private readonly cDMAAddress[] DMASAD = new cDMAAddress[4] { new cDMAAddress(true), new cDMAAddress(false),
            new cDMAAddress(false), new cDMAAddress(false) };
        private readonly cDMAAddress[] DMADAD = new cDMAAddress[4] { new cDMAAddress(true), new cDMAAddress(true),
            new cDMAAddress(true), new cDMAAddress(false) };

        private class cDMACNT_L : IORegister2
        {
            private ushort BitMask;
            private ushort InternalRegister;

            public cDMACNT_L(ushort BitMask) : base()
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

            public override ushort Get()
            {
                // Write only
                return 0;
            }
        }

        private readonly cDMACNT_L[] DMACNT_L = new cDMACNT_L[4] { new cDMACNT_L(0x3fff), new cDMACNT_L(0x3fff),
            new cDMACNT_L(0x3fff), new cDMACNT_L(0xffff) };

        private enum AddrControl : byte
        {
            Increment = 0,
            Decrement = 1,
            Fixed = 2,
            IncrementReload = 3
        }

        public enum DMAStartTiming : byte
        {
            Immediately = 0,
            VBlank = 1,
            HBlank = 2,
            Special = 3
        }

        private class cDMACNT_H : IORegister2
        {
            private bool AllowGamePakDRQ;
            public bool Active;

            private ARM7TDMI cpu;
            public readonly int index;

            public cDMACNT_H(ARM7TDMI cpu, int index) : base()
            {
                this.index = index;
                this.cpu = cpu;
            }

            public cDMACNT_H(ARM7TDMI cpu, int index, bool AllowGamePakDRQ) : this(cpu, index)
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

            public bool DMAEnable
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
                        this.Active = true;
                }
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                bool DoReload = !this.DMAEnable;

                base.Set(value, setlow, sethigh);
                if (DoReload && this.DMAEnable)
                {
                    this.cpu.DMADAD[this.index].Reload();
                    this.cpu.DMASAD[this.index].Reload();
                    this.cpu.DMACNT_L[this.index].Reload();
                }

                if ((this._raw & 0xb000) == 0x8000)  // DMA Enable set AND DMA start timing immediate
                    this.Active = true;
            }
        }

        private cDMACNT_H[] DMACNT_H;

        #endregion

        #region Timer Registers
        public class TimerRegister : IORegister2
        {
            public override ushort Get()
            {
                // Console.WriteLine("Timer read");
                return base.Get();
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                base.Set(value, setlow, sethigh);
                // Console.WriteLine($"Timer set to {value}");
            }
        }
        #endregion
    }
}
