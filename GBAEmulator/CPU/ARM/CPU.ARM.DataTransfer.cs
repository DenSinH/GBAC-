﻿using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int Halfword_SignedDataTransfer(uint Instruction)
        {
            bool PreIndex, Up, WriteBack, LoadFromMemory;
            byte Rn, Rd, SH;
            uint Address, Offset;

            PreIndex = (Instruction & 0x0100_0000) > 0;
            Up = (Instruction & 0x0080_0000) > 0;
            WriteBack = (Instruction & 0x0020_0000) > 0;
            LoadFromMemory = (Instruction & 0x0010_0000) > 0;
            Rn = (byte)((Instruction & 0x000f_0000) >> 16);  // Base Register
            Rd = (byte)((Instruction & 0x0000_f000) >> 12);  // Source/Destination Register
            SH = (byte)((Instruction & 0x0000_0060) >> 5);  // Transfer type

            if ((Instruction & 0x0040_0000) == 0)
            {
                // Register offset
                byte Rm = (byte)(Instruction & 0x0000_000f);  // Offset Register
                Offset = this.Registers[Rm];
            }
            else
            {
                // Immediate offset
                Offset = (byte)(((Instruction & 0x0000_0f00) >> 4) | (Instruction & 0x0000_000f));
            }

            Address = this.Registers[Rn];

            if (PreIndex)
            {
                if (Up)
                {
                    Address += Offset;
                }
                else
                {
                    Address -= Offset;
                }
            }
            
            switch (SH)
            {
                case 0b00:  // SWP instruction, caught in single data swap, so we never get here
                    this.Error("SWP instruction not caught");
                    break;
                case 0b01:  // Unsigned halfwords
                    /*
                    The supplied address should always be on a
                    halfword boundary. If bit 0 of the supplied address is HIGH then the ARM7TDMI will
                    load an unpredictable value.

                    GBATek: LDRH Rd,[odd]   -->  LDRH Rd,[odd-1] ROR 8  ;read to bit0-7 and bit24-31
                    */
                    this.Log(string.Format("Halfword Data Transfer: Unsigned Halfword Rd = R{0} @{1:x8}", Rd, Address));
                    if (LoadFromMemory)
                    {
                        if ((Address & 0x01) == 0)  // aligned
                            this.Registers[Rd] = this.mem.GetHalfWordAt(Address);
                        else  // alignment happens in the memory handler
                            this.Registers[Rd] = ROR(this.mem.GetHalfWordAt(Address), 8);
                    }
                    else
                    {   
                        // force align happens in memory handler
                        this.mem.SetHalfWordAt(Address, (ushort)this.Registers[Rd]);
                    }
                    break;
                case 0b10:  // Signed byte
                    this.Log(string.Format("Halfword Data Transfer: Signed byte Rd = R{0} @{1:x8}", Rd, Address, Rd));
                    if (LoadFromMemory)
                    {
                        this.Registers[Rd] = (uint)(sbyte)this.mem.GetByteAt(Address);
                    }
                    else
                    {
                        this.Error("Cannot store signed byte");
                    }
                    break;
                case 0b11:  // Signed halfwords
                    /*
                    The supplied address should always be on a
                    halfword boundary. If bit 0 of the supplied address is HIGH then the ARM7TDMI will
                    load an unpredictable value.

                    GBATek: LDRSH Rd,[odd]  -->  LDRSB Rd,[odd]         ;sign-expand BYTE value
                    */
                    this.Log(string.Format("Halfword Data Transfer: Signed Halfword Rd = R{0} @{1:x8}", Rd, Address, Rd));
                    if (LoadFromMemory)
                    {
                        if ((Address & 0x01) == 1)  // misaligned
                        {
                            // equivalent to this.mem.GetByteAt(Address) for normal addresses
                            // memory alignment happens in memory handler
                            this.Registers[Rd] = (uint)(sbyte)(this.mem.GetHalfWordAt(Address) >> 8);
                        }
                        else
                        {
                            this.Registers[Rd] = (uint)(short)this.mem.GetHalfWordAt(Address);
                        }
                    }
                    else
                    {
                        this.Error("Cannot store signed halfword");
                    }
                    break;
            }

            if ((WriteBack || !PreIndex) && !(Rn == Rd && LoadFromMemory))
            {
                if (!PreIndex)
                {
                    if (Up)
                    {
                        Address += Offset;
                    }
                    else
                    {
                        Address -= Offset;
                    }
                }

                // Write-back must not be specified if R15 is specified as the base register (Rn). (Manual)
                this.Registers[Rn] = Address;
            }

            return LoadFromMemory ? ICycle : 0;
        }
    }
}
