using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private class EmptyRegister : IORegister2 { }

        #region DISPCNT
        public class cDISPCNT : IORegister2
        {
            [Flags]
            public enum DISPCNTFlags : ushort
            {
                CGBMode = 0x0008,
                DPFrameSelect = 0x0010,
                HBLankIntervalFree = 0x0020,
                OBJVRamMapping = 0x0040,
                ForcedBlank = 0x0080,
                DisplayBG0 = 0x0100,
                DisplayBG1 = 0x0200,
                DisplayBG2 = 0x0400,
                DisplayBG3 = 0x0800,
                DisplayOBJ = 0x1000,
                WindowDisplay0 = 0x2000,
                WindowDisplay1 = 0x4000,
                OBJWindowDisplay = 0x8000,
            }

            public byte BGMode
            {
                get => (byte)(this.raw & 0x07);
            }

            public bool IsSet(DISPCNTFlags flag)
            {
                return (this.raw & (ushort)flag) == (ushort)flag;
            }
        }

        public cDISPCNT DISPCNT = new cDISPCNT();  // 0x0400_0004
        #endregion

        #region DISPSTAT
        public class cDISPSTAT : IORegister2
        {
            [Flags]
            public enum DISPSTATFlags : ushort
            {
                VBlankFlag = 0x0001,
                DPFrameSelect = 0x0002,
                HBLankIntervalFree = 0x0004,
                OBJVRamMapping = 0x0008,
                ForcedBlank = 0x0010,
                DisplayBG0 = 0x0020,
                DisplayBG1 = 0x0040,
                DisplayBG2 = 0x0080,
                DisplayBG3 = 0x0800
            }
            
            public byte VCountSetting
            {
                get => (byte)((this.raw & 0xff00) >> 8);
            }

            public bool IsSet(DISPSTATFlags flag)
            {
                return (this.raw & (ushort)flag) == (ushort)flag;
            }

            public void SetVBlank(bool on)
            {
                if (on)
                    this.raw |= 1;
                else
                    this.raw &= 0xfffe;
            }

            public void SetHBlank(bool on)
            {
                if (on)
                    this.raw |= 2;
                else
                    this.raw &= 0xfffd;
            }

        }

        public cDISPSTAT DISPSTAT = new cDISPSTAT();
        #endregion

        #region VCOUNT
        public class cVCOUNT : IORegister2
        {
            public byte CurrentScanline
            {
                get
                {
                    return (byte)(this.raw & 0x00ff);
                }
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                throw new Exception("Cannot write to VCOUNT register");
            }
        }

        public cVCOUNT VCOUNT = new cVCOUNT();
        #endregion

        #region BGControl

        public class cBGControl : IORegister2
        {
            public byte BGPriority
            {
                get => (byte)(this.raw & 0x03);
            }

            public byte CharBaseBlock
            {
                get => (byte)((this.raw & 0x0c) >> 2);
            }

            public bool Mosaic
            {
                get => (this.raw & 0x40) > 0;
            }

            public bool Colors
            {
                get => (this.raw & 0x80) > 0;
            }

            public byte ScreenBaseBlock
            {
                get => (byte)((this.raw & 0x1f00) >> 8);
            }

            public bool DisplayAreaOverflow
            {
                // not used for BG0 and BG1
                get => (this.raw & 0x2000) > 0;
            }

            public byte ScreenSize
            {
                get => (byte)((this.raw & 0xc00) >> 14);
            }
        }

        cBGControl[] BGCNT = new cBGControl[4] { new cBGControl(), new cBGControl(), new cBGControl(), new cBGControl() };
        #endregion

        #region BGScrolling
        public class cBGScrolling : IORegister2
        {
            public byte Offset
            {
                get => (byte)(this.raw & 0x00ff);
            }
        }

        cBGScrolling[] BGHOFS = new cBGScrolling[4] { new cBGScrolling(), new cBGScrolling(), new cBGScrolling(), new cBGScrolling() };
        cBGScrolling[] BGVOFS = new cBGScrolling[4] { new cBGScrolling(), new cBGScrolling(), new cBGScrolling(), new cBGScrolling() };
        #endregion

        #region LCD I/O BG Rotation/Scaling
        public class cReferencePoint : IORegister4
        {
            public byte FractionalPortion
            {
                get => (byte)(this.lower.Get() & 0x00ff);
            }

            public uint IntegerPortion
            {
                get => (uint)(((this.lower.Get() & 0xff00) >> 8) | ((this.upper.Get() & 0x07ff)));
            }

            public bool Sign
            {
                get => (this.upper.Get() & 0x0800) > 0;
            }
        }

        public class cRotationScaling : IORegister2
        {
            public byte FractionalPortion
            {
                get => (byte)(this.raw & 0x00ff);
            }

            public byte IntegerPortion
            {
                get => (byte)((this.raw & 0x7f00) >> 8);
            }

            public bool Sign
            {
                get => (this.raw & 0x8000) > 0;
            }
        }

        cReferencePoint BG2X = new cReferencePoint();
        cReferencePoint BG2Y = new cReferencePoint();
        cReferencePoint BG3X = new cReferencePoint();
        cReferencePoint BG3Y = new cReferencePoint();

        cRotationScaling BG2PA = new cRotationScaling();
        cRotationScaling BG2PB = new cRotationScaling();
        cRotationScaling BG2PC = new cRotationScaling();
        cRotationScaling BG2PD = new cRotationScaling();
        cRotationScaling BG3PA = new cRotationScaling();
        cRotationScaling BG3PB = new cRotationScaling();
        cRotationScaling BG3PC = new cRotationScaling();
        cRotationScaling BG3PD = new cRotationScaling();
        #endregion

        #region KEYINPUT
        private class cKeyInput : IORegister2
        {
            public override ushort Get()
            {
                return 0xffff;
            }

            public override void Set(ushort value, bool setlow, bool sethigh) { }
        }
        #endregion
    }
}
