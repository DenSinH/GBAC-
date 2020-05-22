using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private IORegister[] IORAM = new IORegister[0x400];           // 1kB IO RAM

        private void InitRegisters()
        {
            this.DISPSTAT = new cDISPSTAT(this);
            this.VCOUNT = new cVCOUNT(this);

            this.IORAM[0x00] = this.IORAM[0x01] = this.DISPCNT;
            this.IORAM[0x02] = this.IORAM[0x03] = new EmptyRegister();
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
            this.IORAM[0x26] = this.IORAM[0x27] = this.BG2PC;

            this.IORAM[0x28] = this.IORAM[0x29] = this.BG2X.lower;
            this.IORAM[0x2a] = this.IORAM[0x2b] = this.BG2X.upper;
            this.IORAM[0x2c] = this.IORAM[0x2d] = this.BG2Y.lower;
            this.IORAM[0x2e] = this.IORAM[0x2f] = this.BG2Y.upper;

            this.IORAM[0x30] = this.IORAM[0x31] = this.BG3PA;
            this.IORAM[0x32] = this.IORAM[0x33] = this.BG3PB;
            this.IORAM[0x34] = this.IORAM[0x35] = this.BG3PC;
            this.IORAM[0x36] = this.IORAM[0x37] = this.BG3PC;

            this.IORAM[0x38] = this.IORAM[0x39] = this.BG3X.lower;
            this.IORAM[0x3a] = this.IORAM[0x3b] = this.BG3X.upper;
            this.IORAM[0x3c] = this.IORAM[0x3d] = this.BG3Y.lower;
            this.IORAM[0x3e] = this.IORAM[0x3f] = this.BG3Y.upper;


            for (int i = 0x20; i < IORAM.Length >> 1; i++)
            {
                // double length no registers
                this.IORAM[2 * i] = this.IORAM[2 * i + 1] = new EmptyRegister();
            }

            cKeyInterruptControl KEYCNT = new cKeyInterruptControl();
            this.IORAM[0x0130] = this.IORAM[0x0131] = new cKeyInput(KEYCNT, this);
            this.IORAM[0x0132] = this.IORAM[0x0133] = KEYCNT;

            this.IORAM[0x0208] = this.IORAM[0x0209] = this.IME;
            this.IORAM[0x0200] = this.IORAM[0x0201] = this.IE;
            this.IORAM[0x0202] = this.IORAM[0x0203] = this.IF;
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
                reg.Set(value, !offset, offset);
            else
                reg.Set((ushort)(value << 8), !offset, offset);
        }
        
        private ushort IOGetHalfWordAt(uint address)
        {
            this.Log("Get register halfword at address " + address.ToString("x"));
            IORegister reg = this.IORAM[address];
            bool offset = (address & 1) > 0;

            if (!offset)
                return (ushort)reg.Get();

            return (ushort)(((reg.Get() & 0xff00) >> 8) | (this.IORAM[address + 1].Get() & 0x00ff));
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
                return (uint)(reg.Get() | (reg.Get() << 16));
            }
            uint result = (uint)(reg.Get() >> 8);
            result |= ((uint)this.IORAM[address + 2].Get() << 8);
            result |= ((uint)this.IORAM[address + 4].Get() << 24);
            return result;

        }

        private void IOSetWordAt(uint address, uint value)
        {
            this.Log("Set register word at address " + address.ToString("x3") + " " + value.ToString("x"));
            this.Log(this.PC.ToString("x"));
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
