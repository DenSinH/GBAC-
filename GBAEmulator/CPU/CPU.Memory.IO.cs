using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private IORegister[] IORAM = new IORegister[0x400];           // 1kB IO RAM

        private void InitRegisters()
        {
            for (int i = 0; i < IORAM.Length >> 1; i++)
            {
                // double length no registers
                this.IORAM[2 * i] = this.IORAM[2 * i + 1] = new NORegister();
            }
        }

        private byte IOGetByteAt(uint address)
        {
            IORegister reg = this.IORAM[address];
            byte offset = (byte)(address & (reg.Length - 1));

            if (offset == 0)
                return (byte)reg.Get();

            return (byte)((reg.Get() & (0xff << (8 * offset))) >> offset);
        }

        private void IOSetByteAt(uint address, byte value)
        {
            IORegister reg = this.IORAM[address];
            byte offset = (byte)(address & (reg.Length - 1));

            if (offset == 0)
            {
                reg.Set(value, 0xff);
                return;
            }
            reg.Set((uint)(value << (8 * offset)), (uint)(0xff << (8 * offset)));
        }
        
        private ushort IOGetHalfWordAt(uint address)
        {
            IORegister reg = this.IORAM[address];
            byte offset = (byte)(address & (reg.Length - 1));

            if (offset == 0)
                return (ushort)reg.Get();
            else if (offset == reg.Length - 1)
            {
                ushort result;
                result = (ushort)((reg.Get() & (0xff << (8 * offset))) >> offset);
                result |= (ushort)((this.IORAM[address + 1].Get() & 0x00ff) << 8);
                return result;
            }
            else  // only possible with a 1 or 2 offset in a length 4 register
            {
                return (ushort)((reg.Get() & (0xffff << (8 * offset))) >> offset);
            }
        }

        private void IOSetHalfWordAt(uint address, ushort value)
        {
            IORegister reg = this.IORAM[address];
            byte offset = (byte)(address & (reg.Length - 1));

            if (offset == 0)
            {
                reg.Set(value, 0xffff);
                return;
            }
            else if (offset == reg.Length - 1)
            {
                reg.Set((uint)(value << (8 * offset)), (uint)(0xff << (8 * offset)));
                this.IORAM[address + 1].Set((uint)value >> 8, 0xff);
                return;
            }
            else  // only possible with a 1 or 2 offset in a length 4 register
            {
                reg.Set((uint)(value << (8 * offset)), (uint)(0xffff << (8 * offset)));
            }
        }

        private uint IOGetWordAt(uint address)
        {
            IORegister reg = this.IORAM[address];
            byte offset = (byte)(address & (reg.Length - 1));

            if (offset == 0)
            {
                if (reg.Length == 4)
                    return reg.Get();
                return reg.Get() | (this.IORAM[address + 1].Get() << 16);
            }
            else
            {
                uint result = reg.Get() >> (8 * offset);
                if (reg.Length == 4)
                {
                    if (offset != 3 || this.IORAM[address + 1].Length == 4)
                        // we don't need a third register
                        return result | (this.IORAM[address + 1].Get() << (8 * (4 - offset)));
                    else
                        // only possible if offset == 3 and the next is of length 2
                        return result | (this.IORAM[address + 1].Get() << 8) | (this.IORAM[address + 2].Get() << 24);
                }
                else
                {
                    // now offset must be 1, as it is not 0 and the register is of length 2
                    if (this.IORAM[address + 1].Length == 4)
                        // don't need a third register
                        return result | (this.IORAM[address + 1].Get() << 8);
                    else
                        return result | (this.IORAM[address + 1].Get() << 8) | (this.IORAM[address + 2].Get() << 24);
                }
            }
        }

        private void IOSetWordAt(uint address, uint value)
        {
            IORegister reg = this.IORAM[address];
            byte offset = (byte)(address & (reg.Length - 1));

            if (offset == 0)
            {
                if (reg.Length == 4)
                {
                    reg.Set(value, 0xffff_ffff);
                    return;
                }
                reg.Set(value & 0xffff, 0xffff);
                this.IORAM[address + 1].Set(value >> 16, 0xffff);
                return;
            }
            else
            {
                if (reg.Length == 4)
                {
                    if (offset != 3 || this.IORAM[address + 1].Length == 4)
                    {
                        // we don't need a third register
                        reg.Set(value << (8 * (4 - offset)), (uint)(0xff_ffff << (8 * offset)));
                        this.IORAM[address + 1].Set(value >> (8 * offset), 0xffff_ffff >> (8 * offset));
                    }
                    else
                    {
                        // only possible if offset == 3 and the next is of length 2
                        reg.Set(value << 24, 0xff00_0000);
                        this.IORAM[address + 1].Set(value >> 8, 0xffff);
                        this.IORAM[address + 2].Set(value >> 24, 0x00ff);

                    }
                }
                else
                {
                    // now offset must be 1, as it is not 0 and the register is of length 2
                    if (this.IORAM[address + 1].Length == 4)
                    {
                        // don't need a third register
                        reg.Set(value << 8, 0xff00);
                        this.IORAM[address + 1].Set(value >> 8, 0xff_ffff);
                    }
                    else
                    {
                        reg.Set(value << 8, 0xff00);
                        this.IORAM[address + 1].Set(value >> 8, 0xffff);
                        this.IORAM[address + 2].Set(value >> 24, 0x00ff);
                    }
                }
            }
        }

    }
}
