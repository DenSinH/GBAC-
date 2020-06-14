using GBAEmulator.CPU;
using GBAEmulator.Bus;
using System;

using GBAEmulator.Memory.IO;

namespace GBAEmulator.Memory.Sections
{
    public partial class IORAMSection : IMemorySection
    {
        private readonly IORegister[] Storage = new IORegister[0x400];  // 1kB IO RAM
        private readonly UnusedRegister MasterUnusedRegister;
        private readonly ZeroRegister MasterZeroRegister = new ZeroRegister();
        const int AddressMask = 0x00ff_ffff;

        public readonly cDISPCNT DISPCNT = new cDISPCNT();
        public readonly cDISPSTAT DISPSTAT;
        public readonly cVCOUNT VCOUNT;

        public readonly cBGControl[] BGCNT = new cBGControl[4] {
                new cBGControl(0xdfff), new cBGControl(0xdfff), new cBGControl(0xffff), new cBGControl(0xffff)
            };

        public readonly cBGScrolling[] BGHOFS;
        public readonly cBGScrolling[] BGVOFS;

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

        public readonly cWindowDimensions[] WINH;
        public readonly cWindowDimensions[] WINV;

        public readonly cWindowControl WININ = new cWindowControl();
        public readonly cWindowControl WINOUT = new cWindowControl();

        public readonly cMosaic MOSAIC = new cMosaic();

        public readonly cBLDCNT BLDCNT = new cBLDCNT();
        public readonly cBLDALPHA BLDALPHA = new cBLDALPHA();
        public readonly cBLDY BLDY = new cBLDY();

        public readonly cDMAAddress[] DMASAD;
        public readonly cDMAAddress[] DMADAD;
        public readonly cDMACNT_L[] DMACNT_L;
        public readonly cDMACNT_H[] DMACNT_H;

        public readonly cSIODATA32 SIODATA32 = new cSIODATA32();
        // SIOMULTIx
        public readonly cSIOCNT SIOCNT;
        // SIOMLT_SEND
        public readonly cSIODATA8 SIODATA8 = new cSIODATA8();

        public readonly cKeyInput KEYINPUT;
        public readonly cKeyInterruptControl KEYCNT;

        public readonly cRCNT RCNT = new cRCNT();
        // JOY_x registers

        public readonly cIME IME = new cIME();
        public readonly cIE IE = new cIE();
        public readonly cIF IF = new cIF();

        public readonly cWAITCNT WAITCNT = new cWAITCNT();

        public readonly cPOSTFLG_HALTCNT HALTCNT = new cPOSTFLG_HALTCNT();

        public IORAMSection(ARM7TDMI cpu)
        {
            BUS bus = cpu.bus;

            this.DISPSTAT = new cDISPSTAT(this.IF);
            this.VCOUNT = new cVCOUNT(this.IF, this.DISPSTAT);
            this.BGHOFS = new cBGScrolling[4] { new cBGScrolling(bus, true), new cBGScrolling(bus, false),
                                                new cBGScrolling(bus, true), new cBGScrolling(bus, false) };

            this.BGVOFS = new cBGScrolling[4] { new cBGScrolling(bus, true), new cBGScrolling(bus, false),
                                                new cBGScrolling(bus, true), new cBGScrolling(bus, false) };

            this.BG2X = new cReferencePoint(bus);
            this.BG2Y = new cReferencePoint(bus);
            this.BG3X = new cReferencePoint(bus);
            this.BG3Y = new cReferencePoint(bus);

            this.BG2PA = new cRotationScaling(bus, true);
            this.BG2PB = new cRotationScaling(bus, false);
            this.BG2PC = new cRotationScaling(bus, true);
            this.BG2PD = new cRotationScaling(bus, false);

            this.BG3PA = new cRotationScaling(bus, true);
            this.BG3PB = new cRotationScaling(bus, false);
            this.BG3PC = new cRotationScaling(bus, true);
            this.BG3PD = new cRotationScaling(bus, false);

            this.WINH = new cWindowDimensions[2] { new cWindowDimensions(bus, true), new cWindowDimensions(bus, false) };
            this.WINV = new cWindowDimensions[2] { new cWindowDimensions(bus, true), new cWindowDimensions(bus, false) };

            this.SIOCNT = new cSIOCNT(this.IF);

            this.KEYCNT = new cKeyInterruptControl(this);
            this.KEYINPUT = new cKeyInput(this.KEYCNT, this.IF);

            this.DMASAD = new cDMAAddress[4] { new cDMAAddress(bus, true),  new cDMAAddress(bus, false),
                                               new cDMAAddress(bus, false), new cDMAAddress(bus, false) };
            this.DMADAD = new cDMAAddress[4] { new cDMAAddress(bus, true), new cDMAAddress(bus, true),
                                               new cDMAAddress(bus, true), new cDMAAddress(bus, false) };

            this.DMACNT_L = new cDMACNT_L[4] { new cDMACNT_L(bus, 0x3fff), new cDMACNT_L(bus, 0x3fff),
                                               new cDMACNT_L(bus, 0x3fff), new cDMACNT_L(bus, 0xffff) };
            this.DMACNT_H = new cDMACNT_H[4] { new cDMACNT_H(DMASAD[0], DMADAD[0], DMACNT_L[0], 0),
                                               new cDMACNT_H(DMASAD[1], DMADAD[1], DMACNT_L[1], 1),
                                               new cDMACNT_H(DMASAD[2], DMADAD[2], DMACNT_L[2], 2),
                                               new cDMACNT_H(DMASAD[3], DMADAD[3], DMACNT_L[3], 3, true) };

            this.MasterUnusedRegister = new UnusedRegister(bus);

            this.Init(cpu);
        }

        private void Init(ARM7TDMI cpu)
        { 
            // LCD I/O Registers
            this.Storage[0x00] = this.Storage[0x01] = this.DISPCNT;
            this.Storage[0x02] = this.Storage[0x03] = new DefaultRegister();  // green swap
            this.Storage[0x04] = this.Storage[0x05] = this.DISPSTAT;
            this.Storage[0x06] = this.Storage[0x07] = this.VCOUNT;

            this.Storage[0x08] = this.Storage[0x09] = this.BGCNT[0];
            this.Storage[0x0a] = this.Storage[0x0b] = this.BGCNT[1];
            this.Storage[0x0c] = this.Storage[0x0d] = this.BGCNT[2];
            this.Storage[0x0e] = this.Storage[0x0f] = this.BGCNT[3];

            this.Storage[0x10] = this.Storage[0x11] = this.BGHOFS[0];
            this.Storage[0x12] = this.Storage[0x13] = this.BGVOFS[0];
            this.Storage[0x14] = this.Storage[0x15] = this.BGHOFS[1];
            this.Storage[0x16] = this.Storage[0x17] = this.BGVOFS[1];
            this.Storage[0x18] = this.Storage[0x19] = this.BGHOFS[2];
            this.Storage[0x1a] = this.Storage[0x1b] = this.BGVOFS[2];
            this.Storage[0x1c] = this.Storage[0x1d] = this.BGHOFS[3];
            this.Storage[0x1e] = this.Storage[0x1f] = this.BGVOFS[3];

            this.Storage[0x20] = this.Storage[0x21] = this.BG2PA;
            this.Storage[0x22] = this.Storage[0x23] = this.BG2PB;
            this.Storage[0x24] = this.Storage[0x25] = this.BG2PC;
            this.Storage[0x26] = this.Storage[0x27] = this.BG2PD;

            this.Storage[0x28] = this.Storage[0x29] = this.BG2X.lower;
            this.Storage[0x2a] = this.Storage[0x2b] = this.BG2X.upper;
            this.Storage[0x2c] = this.Storage[0x2d] = this.BG2Y.lower;
            this.Storage[0x2e] = this.Storage[0x2f] = this.BG2Y.upper;

            this.Storage[0x30] = this.Storage[0x31] = this.BG3PA;
            this.Storage[0x32] = this.Storage[0x33] = this.BG3PB;
            this.Storage[0x34] = this.Storage[0x35] = this.BG3PC;
            this.Storage[0x36] = this.Storage[0x37] = this.BG3PD;

            this.Storage[0x38] = this.Storage[0x39] = this.BG3X.lower;
            this.Storage[0x3a] = this.Storage[0x3b] = this.BG3X.upper;
            this.Storage[0x3c] = this.Storage[0x3d] = this.BG3Y.lower;
            this.Storage[0x3e] = this.Storage[0x3f] = this.BG3Y.upper;

            this.Storage[0x40] = this.Storage[0x41] = this.WINH[0];
            this.Storage[0x42] = this.Storage[0x43] = this.WINH[1];

            this.Storage[0x44] = this.Storage[0x45] = this.WINV[0];
            this.Storage[0x46] = this.Storage[0x47] = this.WINV[1];

            this.Storage[0x48] = this.Storage[0x49] = this.WININ;
            this.Storage[0x4a] = this.Storage[0x4b] = this.WINOUT;

            this.Storage[0x4c] = this.Storage[0x4d] = this.MOSAIC;
            this.Storage[0x4e] = this.Storage[0x4f] = this.MasterZeroRegister;  // unused MOSAIC bits, lower half readable

            this.Storage[0x50] = this.Storage[0x51] = this.BLDCNT;
            this.Storage[0x52] = this.Storage[0x53] = this.BLDALPHA;
            this.Storage[0x54] = this.Storage[0x55] = this.BLDY;
            this.Storage[0x56] = this.Storage[0x57] = this.MasterZeroRegister;  // lower half readable

            for (int i = 0x58; i < 0x60; i += 4)
            {
                this.Storage[i]     = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Sound Registers
            for (int i = 0x60; i <= 0xa6; i += 2)
            {
                // double length no registers
                this.Storage[i] = this.Storage[i + 1] = new DefaultRegister();
            }

            for (int i = 0xa8; i < 0xb0; i += 4)
            {
                this.Storage[i]     = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // DMA Transfer Channels
            this.Storage[0xb0] = this.Storage[0xb1] = this.DMASAD[0].lower;
            this.Storage[0xb2] = this.Storage[0xb3] = this.DMASAD[0].upper;
            this.Storage[0xb4] = this.Storage[0xb5] = this.DMADAD[0].lower;
            this.Storage[0xb6] = this.Storage[0xb7] = this.DMADAD[0].upper;
            this.Storage[0xb8] = this.Storage[0xb9] = this.DMACNT_L[0];
            this.Storage[0xba] = this.Storage[0xbb] = this.DMACNT_H[0];

            this.Storage[0xbc] = this.Storage[0xbd] = this.DMASAD[1].lower;
            this.Storage[0xbe] = this.Storage[0xbf] = this.DMASAD[1].upper;
            this.Storage[0xc0] = this.Storage[0xc1] = this.DMADAD[1].lower;
            this.Storage[0xc2] = this.Storage[0xc3] = this.DMADAD[1].upper;
            this.Storage[0xc4] = this.Storage[0xc5] = this.DMACNT_L[1];
            this.Storage[0xc6] = this.Storage[0xc7] = this.DMACNT_H[1];

            this.Storage[0xc8] = this.Storage[0xc9] = this.DMASAD[2].lower;
            this.Storage[0xca] = this.Storage[0xcb] = this.DMASAD[2].upper;
            this.Storage[0xcc] = this.Storage[0xcd] = this.DMADAD[2].lower;
            this.Storage[0xce] = this.Storage[0xcf] = this.DMADAD[2].upper;
            this.Storage[0xd0] = this.Storage[0xd1] = this.DMACNT_L[2];
            this.Storage[0xd2] = this.Storage[0xd3] = this.DMACNT_H[2];

            this.Storage[0xd4] = this.Storage[0xd5] = this.DMASAD[3].lower;
            this.Storage[0xd6] = this.Storage[0xd7] = this.DMASAD[3].upper;
            this.Storage[0xd8] = this.Storage[0xd9] = this.DMADAD[3].lower;
            this.Storage[0xda] = this.Storage[0xdb] = this.DMADAD[3].upper;
            this.Storage[0xdc] = this.Storage[0xdd] = this.DMACNT_L[3];
            this.Storage[0xde] = this.Storage[0xdf] = this.DMACNT_H[3];

            for (int i = 0xe0; i < 0x100; i += 4)
            {
                this.Storage[i]     = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Timer Registers
            this.Storage[0x100] = this.Storage[0x101] = cpu.Timers[0].Data;
            this.Storage[0x102] = this.Storage[0x103] = cpu.Timers[0].Control;

            this.Storage[0x104] = this.Storage[0x105] = cpu.Timers[1].Data;
            this.Storage[0x106] = this.Storage[0x107] = cpu.Timers[1].Control;

            this.Storage[0x108] = this.Storage[0x109] = cpu.Timers[2].Data;
            this.Storage[0x10a] = this.Storage[0x10b] = cpu.Timers[2].Control;

            this.Storage[0x10c] = this.Storage[0x10d] = cpu.Timers[3].Data;
            this.Storage[0x10e] = this.Storage[0x10f] = cpu.Timers[3].Control;
            
            for (int i = 0x110; i < 0x120; i += 4)
            {
                this.Storage[i]     = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Serial Communication (1)
            this.Storage[0x120] = this.Storage[0x121] = this.SIODATA32.lower;  // shared
            this.Storage[0x122] = this.Storage[0x123] = this.SIODATA32.upper;  // shared
            this.Storage[0x124] = this.Storage[0x125] = new DefaultRegister();
            this.Storage[0x126] = this.Storage[0x127] = new DefaultRegister();
            this.Storage[0x128] = this.Storage[0x129] = this.SIOCNT;
            this.Storage[0x12a] = this.Storage[0x12b] = this.SIODATA8;  // shared

            this.Storage[0x012c] = this.Storage[0x012d] = this.MasterUnusedRegister.lower;
            this.Storage[0x012e] = this.Storage[0x012f] = this.MasterUnusedRegister.upper;

            // Keypad Input
            this.Storage[0x0130] = this.Storage[0x0131] = this.KEYINPUT;
            this.Storage[0x0132] = this.Storage[0x0133] = this.KEYCNT;

            // Serial Communication (2)
            this.Storage[0x134] = this.Storage[0x135] = this.RCNT;

            this.Storage[0x136] = this.Storage[0x137] = this.MasterUnusedRegister.upper;  // note offset!
            for (int i = 0x0138; i < 0x140; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.Storage[0x140] = this.Storage[0x141] = new DefaultRegister();

            for (int i = 0x0142; i < 0x150; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.Storage[0x150] = this.Storage[0x151] = new DefaultRegister();
            this.Storage[0x152] = this.Storage[0x153] = new DefaultRegister();
            this.Storage[0x154] = this.Storage[0x155] = new DefaultRegister();
            this.Storage[0x156] = this.Storage[0x157] = new DefaultRegister();
            this.Storage[0x158] = this.Storage[0x159] = new DefaultRegister();

            this.Storage[0x15a] = this.Storage[0x15b] = this.MasterZeroRegister;
            for (int i = 0x015c; i < 0x200; i += 4)
            {
                this.Storage[i]     = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Interrupt, Waitstate and Power-Down Control
            this.Storage[0x0200] = this.Storage[0x0201] = this.IE;
            this.Storage[0x0202] = this.Storage[0x0203] = this.IF;
            this.Storage[0x0204] = this.Storage[0x0205] = this.WAITCNT;

            this.Storage[0x0206] = this.Storage[0x0207] = this.MasterZeroRegister;
            this.Storage[0x0208] = this.Storage[0x0209] = this.IME;
            this.Storage[0x020a] = this.Storage[0x020b] = this.MasterZeroRegister;

            for (int i = 0x20c; i < 0x300; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.Storage[0x0300] = this.Storage[0x0301] = this.HALTCNT;
            this.Storage[0x0302] = this.Storage[0x0303] = this.MasterZeroRegister;

            for (int i = 0x304; i < 0x400; i += 4)
            {
                this.Storage[i]     = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            for (int i = 0; i < 0x400; i++)
            {
                if (this.Storage[i] is null)
                {
                    this.Error(i.ToString("x4") + " in IORAM not initialized");
                }
            }
        }

        private void Error(string message)
        {

        }

        private void Log(string message)
        {

        }

        public byte? GetByteAt(uint address)
        {
            if ((address & 0xfffc) == 0x0800) { Console.WriteLine(address.ToString("x4")); return null; }
            else if ((address &= AddressMask) >= this.Storage.Length) return null;

            this.Log("Get register byte at address " + address.ToString("x3"));
            IORegister reg = this.Storage[address];
            bool offset = (address & 1) > 0;

            if (!offset)
                return (byte)reg.Get();
            else
                return (byte)((reg.Get() & 0xff00) >> 8);
        }

        public void SetByteAt(uint address, byte value)
        {
            if ((address & 0xfffc) == 0x0800) { Console.WriteLine(address.ToString("x4")); return; }
            else if ((address &= AddressMask) >= this.Storage.Length) return;

            this.Log("Set register byte at address " + address.ToString("x3") + " " + value.ToString("x"));
            IORegister reg = this.Storage[address];

            bool offset = (address & 1) > 0;
            if (!offset)
                reg.Set(value, true, false);
            else
                reg.Set((ushort)(value << 8), false, true);
        }
        
        public ushort? GetHalfWordAt(uint address)
        {
            if ((address & 0xfffc) == 0x0800) { Console.WriteLine(address.ToString("x4")); return null; }
            else if((address &= AddressMask) >= this.Storage.Length) return null;
            address &= 0x00ff_fffe;  // force align

            this.Log("Get register halfword at address " + address.ToString("x"));
            IORegister reg = this.Storage[address];

            // force alignment makes these accesses a lot simpler!
            return reg.Get();
        }

        public void SetHalfWordAt(uint address, ushort value)
        {
            if ((address & 0xfffc) == 0x0800) { Console.WriteLine(address.ToString("x4")); return; }
            else if((address &= AddressMask) >= this.Storage.Length) return;
            address &= 0x00ff_fffe;  // force align

            this.Log("Set register halfword at address " + address.ToString("x3") + " " + value.ToString("x"));
            IORegister reg = this.Storage[address];

            // force alignment makes these accesses a lot simpler!
            reg.Set(value, true, true);
        }

        public uint? GetWordAt(uint address)
        {
            if ((address & 0xfffc) == 0x0800) { Console.WriteLine(address.ToString("x4")); return null; }
            else if((address &= AddressMask) >= this.Storage.Length) return null;
            address &= 0x00ff_fffc;  // force align

            this.Log("Get register word at address " + address.ToString("x"));
            IORegister reg = this.Storage[address];

            // force alignment makes these accesses a lot simpler!
            return (uint)(reg.Get() | (this.Storage[address + 2].Get() << 16));

        }

        public void SetWordAt(uint address, uint value)
        {
            if ((address & 0xfffc) == 0x0800) { Console.WriteLine(address.ToString("x4")); return; }
            else if((address &= AddressMask) >= this.Storage.Length) return;
            address &= 0x00ff_fffc;  // force align

            this.Log("Set register word at address " + address.ToString("x3") + " " + value.ToString("x"));
            IORegister reg = this.Storage[address];

            // force alignment makes these accesses a lot simpler!
            reg.Set((ushort)value, true, true);
            this.Storage[address + 2].Set((ushort)(value >> 16), true, true);
        }
    }
}
