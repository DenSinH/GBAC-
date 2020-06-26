using GBAEmulator.CPU;
using GBAEmulator.Bus;
using System;

using GBAEmulator.Memory.Sections;
using System.Diagnostics;
using GBAEmulator.Audio.Channels;
using GBAEmulator.Audio;
using GBAEmulator.Video;

namespace GBAEmulator.IO
{
    public partial class IORAMSection : IMemorySection
    {
        private IORegister[] Storage = new IORegister[0x400];  // 1kB IO RAM
        private UnusedRegister MasterUnusedRegister;
        private ZeroRegister MasterZeroRegister = new ZeroRegister();
        const uint AddressMask = 0x00ff_ffff;

        public cDISPCNT DISPCNT;
        public cDISPSTAT DISPSTAT;
        public cVCOUNT VCOUNT;

        public cBGControl[] BGCNT;

        public cBGScrolling[] BGHOFS;
        public cBGScrolling[] BGVOFS;

        public cReferencePoint BG2X;
        public cReferencePoint BG2Y;
        public cReferencePoint BG3X;
        public cReferencePoint BG3Y;

        public cRotationScaling BG2PA;
        public cRotationScaling BG2PB;
        public cRotationScaling BG2PC;
        public cRotationScaling BG2PD;

        public cRotationScaling BG3PA;
        public cRotationScaling BG3PB;
        public cRotationScaling BG3PC;
        public cRotationScaling BG3PD;

        public cWindowDimensions[] WINH;
        public cWindowDimensions[] WINV;

        public cWindowControl WININ;
        public cWindowControl WINOUT;

        public cMosaic MOSAIC;

        public cBLDCNT BLDCNT;
        public cBLDALPHA BLDALPHA;
        public cBLDY BLDY;

        public cSIODATA32 SIODATA32 = new cSIODATA32();
        // SIO
        public cSIOCNT SIOCNT;
        // SIO
        public cSIODATA8 SIODATA8 = new cSIODATA8();

        public cKeyInput KEYINPUT;
        public cKeyInterruptControl KEYCNT;

        public cRCNT RCNT = new cRCNT();
        // JOYers

        public cIME IME = new cIME();
        public cIE IE = new cIE();
        public cIF IF = new cIF();

        public cWAITCNT WAITCNT = new cWAITCNT();

        public cPOSTFLG_HALTCNT HALTCNT = new cPOSTFLG_HALTCNT();

        public IORAMSection()
        {

        }

        public void Reset()
        {
            for (uint i = 0; i < 0x400; i += 2)
            {
                this.SetHalfWordAt(i, 0);
            }
            this.HALTCNT.Halt = false;
        }

        private void Error(string message)
        {
            Console.Error.WriteLine($"IO Error: {message}");
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            Console.WriteLine(message);
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
