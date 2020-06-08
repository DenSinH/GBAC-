﻿using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        public IORegister[] IORAM = new IORegister[0x400];           // 1kB IO RAM
        private UnusedRegister MasterUnusedRegister;
        private ZeroRegister MasterZeroRegister = new ZeroRegister();

        public void InitRegisters()
        {
            // LCD I/O Registers
            this.IORAM[0x00] = this.IORAM[0x01] = this.DISPCNT;
            this.IORAM[0x02] = this.IORAM[0x03] = new DefaultRegister();  // green swap
            this.IORAM[0x04] = this.IORAM[0x05] = this.DISPSTAT;
            this.IORAM[0x06] = this.IORAM[0x07] = this.VCOUNT;

            this.IORAM[0x08] = this.IORAM[0x09] = this.BGCNT[0];
            this.IORAM[0x0a] = this.IORAM[0x0b] = this.BGCNT[1];
            this.IORAM[0x0c] = this.IORAM[0x0d] = this.BGCNT[2];
            this.IORAM[0x0e] = this.IORAM[0x0f] = this.BGCNT[3];

            this.IORAM[0x10] = this.IORAM[0x11] = this.BGHOFS[0];
            this.IORAM[0x12] = this.IORAM[0x13] = this.BGVOFS[0];
            this.IORAM[0x14] = this.IORAM[0x15] = this.BGHOFS[1];
            this.IORAM[0x16] = this.IORAM[0x17] = this.BGVOFS[1];
            this.IORAM[0x18] = this.IORAM[0x19] = this.BGHOFS[2];
            this.IORAM[0x1a] = this.IORAM[0x1b] = this.BGVOFS[2];
            this.IORAM[0x1c] = this.IORAM[0x1d] = this.BGHOFS[3];
            this.IORAM[0x1e] = this.IORAM[0x1f] = this.BGVOFS[3];

            this.IORAM[0x20] = this.IORAM[0x21] = this.BG2PA;
            this.IORAM[0x22] = this.IORAM[0x23] = this.BG2PB;
            this.IORAM[0x24] = this.IORAM[0x25] = this.BG2PC;
            this.IORAM[0x26] = this.IORAM[0x27] = this.BG2PD;

            this.IORAM[0x28] = this.IORAM[0x29] = this.BG2X.lower;
            this.IORAM[0x2a] = this.IORAM[0x2b] = this.BG2X.upper;
            this.IORAM[0x2c] = this.IORAM[0x2d] = this.BG2Y.lower;
            this.IORAM[0x2e] = this.IORAM[0x2f] = this.BG2Y.upper;

            this.IORAM[0x30] = this.IORAM[0x31] = this.BG3PA;
            this.IORAM[0x32] = this.IORAM[0x33] = this.BG3PB;
            this.IORAM[0x34] = this.IORAM[0x35] = this.BG3PC;
            this.IORAM[0x36] = this.IORAM[0x37] = this.BG3PD;

            this.IORAM[0x38] = this.IORAM[0x39] = this.BG3X.lower;
            this.IORAM[0x3a] = this.IORAM[0x3b] = this.BG3X.upper;
            this.IORAM[0x3c] = this.IORAM[0x3d] = this.BG3Y.lower;
            this.IORAM[0x3e] = this.IORAM[0x3f] = this.BG3Y.upper;

            this.IORAM[0x40] = this.IORAM[0x41] = this.WINH[0];
            this.IORAM[0x42] = this.IORAM[0x43] = this.WINH[1];

            this.IORAM[0x44] = this.IORAM[0x45] = this.WINV[0];
            this.IORAM[0x46] = this.IORAM[0x47] = this.WINV[1];

            this.IORAM[0x48] = this.IORAM[0x49] = this.WININ;
            this.IORAM[0x4a] = this.IORAM[0x4b] = this.WINOUT;

            this.IORAM[0x4c] = this.IORAM[0x4d] = this.MOSAIC;
            this.IORAM[0x4e] = this.IORAM[0x4f] = this.MasterZeroRegister;  // unused MOSAIC bits, lower half readable

            this.IORAM[0x50] = this.IORAM[0x51] = this.BLDCNT;
            this.IORAM[0x52] = this.IORAM[0x53] = this.BLDALPHA;
            this.IORAM[0x54] = this.IORAM[0x55] = this.BLDY;
            this.IORAM[0x56] = this.IORAM[0x57] = this.MasterZeroRegister;  // lower half readable

            for (int i = 0x58; i < 0x60; i += 4)
            {
                this.IORAM[i]     = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Sound Registers
            for (int i = 0x60; i <= 0xa6; i += 2)
            {
                // double length no registers
                this.IORAM[i] = this.IORAM[i + 1] = new DefaultRegister();
            }

            for (int i = 0xa8; i < 0xb0; i += 4)
            {
                this.IORAM[i]     = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            // DMA Transfer Channels
            this.IORAM[0xb0] = this.IORAM[0xb1] = this.DMASAD[0].lower;
            this.IORAM[0xb2] = this.IORAM[0xb3] = this.DMASAD[0].upper;
            this.IORAM[0xb4] = this.IORAM[0xb5] = this.DMADAD[0].lower;
            this.IORAM[0xb6] = this.IORAM[0xb7] = this.DMADAD[0].upper;
            this.IORAM[0xb8] = this.IORAM[0xb9] = this.DMACNT_L[0];
            this.IORAM[0xba] = this.IORAM[0xbb] = this.DMACNT_H[0];

            this.IORAM[0xbc] = this.IORAM[0xbd] = this.DMASAD[1].lower;
            this.IORAM[0xbe] = this.IORAM[0xbf] = this.DMASAD[1].upper;
            this.IORAM[0xc0] = this.IORAM[0xc1] = this.DMADAD[1].lower;
            this.IORAM[0xc2] = this.IORAM[0xc3] = this.DMADAD[1].upper;
            this.IORAM[0xc4] = this.IORAM[0xc5] = this.DMACNT_L[1];
            this.IORAM[0xc6] = this.IORAM[0xc7] = this.DMACNT_H[1];

            this.IORAM[0xc8] = this.IORAM[0xc9] = this.DMASAD[2].lower;
            this.IORAM[0xca] = this.IORAM[0xcb] = this.DMASAD[2].upper;
            this.IORAM[0xcc] = this.IORAM[0xcd] = this.DMADAD[2].lower;
            this.IORAM[0xce] = this.IORAM[0xcf] = this.DMADAD[2].upper;
            this.IORAM[0xd0] = this.IORAM[0xd1] = this.DMACNT_L[2];
            this.IORAM[0xd2] = this.IORAM[0xd3] = this.DMACNT_H[2];

            this.IORAM[0xd4] = this.IORAM[0xd5] = this.DMASAD[3].lower;
            this.IORAM[0xd6] = this.IORAM[0xd7] = this.DMASAD[3].upper;
            this.IORAM[0xd8] = this.IORAM[0xd9] = this.DMADAD[3].lower;
            this.IORAM[0xda] = this.IORAM[0xdb] = this.DMADAD[3].upper;
            this.IORAM[0xdc] = this.IORAM[0xdd] = this.DMACNT_L[3];
            this.IORAM[0xde] = this.IORAM[0xdf] = this.DMACNT_H[3];

            for (int i = 0xe0; i < 0x100; i += 4)
            {
                this.IORAM[i]     = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Timer Registers
            this.IORAM[0x100] = this.IORAM[0x101] = this.cpu.Timers[0].Data;
            this.IORAM[0x102] = this.IORAM[0x103] = this.cpu.Timers[0].Control;

            this.IORAM[0x104] = this.IORAM[0x105] = this.cpu.Timers[1].Data;
            this.IORAM[0x106] = this.IORAM[0x107] = this.cpu.Timers[1].Control;

            this.IORAM[0x108] = this.IORAM[0x109] = this.cpu.Timers[2].Data;
            this.IORAM[0x10a] = this.IORAM[0x10b] = this.cpu.Timers[2].Control;

            this.IORAM[0x10c] = this.IORAM[0x10d] = this.cpu.Timers[3].Data;
            this.IORAM[0x10e] = this.IORAM[0x10f] = this.cpu.Timers[3].Control;
            
            for (int i = 0x110; i < 0x120; i += 4)
            {
                this.IORAM[i]     = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Serial Communication (1)
            for (int i = 0x120; i <= 0x12a; i += 2)
            {
                // double length no registers
                this.IORAM[i] = this.IORAM[i + 1] = new DefaultRegister();
            }

            this.IORAM[0x012c] = this.IORAM[0x012d] = this.MasterUnusedRegister.lower;
            this.IORAM[0x012e] = this.IORAM[0x012f] = this.MasterUnusedRegister.upper;

            // Keypad Input
            this.IORAM[0x0130] = this.IORAM[0x0131] = new cKeyInput(this.KEYCNT, this);
            this.IORAM[0x0132] = this.IORAM[0x0133] = this.KEYCNT;

            // Serial Communication (2)
            for (int i = 0x134; i <= 0x158; i += 2)
            {
                // double length no registers
                this.IORAM[i] = this.IORAM[i + 1] = new DefaultRegister();
            }

            this.IORAM[0x15a] = this.IORAM[0x15b] = this.MasterZeroRegister;
            for (int i = 0x015c; i < 0x200; i += 4)
            {
                this.IORAM[i]     = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            // Interrupt, Waitstate and Power-Down Control
            this.IORAM[0x0200] = this.IORAM[0x0201] = this.IE;
            this.IORAM[0x0202] = this.IORAM[0x0203] = this.IF;
            this.IORAM[0x0204] = this.IORAM[0x0205] = new DefaultRegister(); // WAITCNT

            this.IORAM[0x0206] = this.IORAM[0x0207] = this.MasterZeroRegister;
            this.IORAM[0x0208] = this.IORAM[0x0209] = this.IME;
            this.IORAM[0x020a] = this.IORAM[0x020b] = this.MasterZeroRegister;

            for (int i = 0x20c; i < 0x300; i += 4)
            {
                this.IORAM[i] = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.IORAM[0x0300] = this.IORAM[0x0301] = this.HALTCNT;
            this.IORAM[0x0302] = this.IORAM[0x0303] = this.MasterZeroRegister;

            for (int i = 0x304; i < 0x400; i += 4)
            {
                this.IORAM[i]     = this.IORAM[i + 1] = this.MasterUnusedRegister.lower;
                this.IORAM[i + 2] = this.IORAM[i + 3] = this.MasterUnusedRegister.upper;
            }

            for (int i = 0; i < 0x400; i++)
            {
                if (this.IORAM[i] is null)
                {
                    this.Error(i.ToString("x4") + " in IORAM not initialized");
                }
            }
        }

        private byte IOGetByteAt(uint address)
        {
            this.Log("Get register byte at address " + address.ToString("x3"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;

            if (!offset)
                return (byte)reg.Get();

            return (byte)((reg.Get() & 0xff00) >> 8);
        }

        private void IOSetByteAt(uint address, byte value)
        {
            this.Log("Set register byte at address " + address.ToString("x3") + " " + value.ToString("x"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;
            if (!offset)
                reg.Set(value, true, false);
            else
                reg.Set((ushort)(value << 8), false, true);
        }
        
        private ushort IOGetHalfWordAt(uint address)
        {
            this.Log("Get register halfword at address " + address.ToString("x"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;

            if (!offset)
                return (ushort)reg.Get();

            return (ushort)(((reg.Get() & 0xff00) >> 8) | ((this.IORAM[address + 2].Get() & 0x00ff) << 8));
        }

        private void IOSetHalfWordAt(uint address, ushort value)
        {
            this.Log("Set register halfword at address " + address.ToString("x3") + " " + value.ToString("x"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;

            if (!offset)
            {
                reg.Set(value, true, true);
                return;
            }

            reg.Set((ushort)(value << 8), false, true);
            this.IORAM[address + 2].Set((ushort)(value & 0x00ff), true, false);
        }

        private uint IOGetWordAt(uint address)
        {
            this.Log("Get register word at address " + address.ToString("x"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;

            if (!offset)
            {
                return (uint)(reg.Get() | (this.IORAM[address + 2].Get() << 16));
            }
            uint result = (uint)(reg.Get() >> 8);
            result |= ((uint)this.IORAM[address + 2].Get() << 8);
            result |= ((uint)this.IORAM[address + 4].Get() << 24);
            return result;

        }

        private void IOSetWordAt(uint address, uint value)
        {
            this.Log("Set register word at address " + address.ToString("x3") + " " + value.ToString("x"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;

            if (!offset)
            {
                reg.Set((ushort)value, true, true);
                this.IORAM[address + 2].Set((ushort)(value >> 16), true, true);
            }
            else
            {
                reg.Set((ushort)(value << 8), false, true);
                this.IORAM[address + 2].Set((ushort)(value >> 8), true, true);
                this.IORAM[address + 4].Set((ushort)(value >> 24), true, false);
            }
        }
    }
}
