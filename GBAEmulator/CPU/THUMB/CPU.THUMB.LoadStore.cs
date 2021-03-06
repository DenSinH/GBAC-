﻿using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int LoadStoreRegOffset_SignExtended(ushort Instruction)
        {
            // Contains both the instructions for Load/Store with Register offset, and for Load/Store sign-extended

            byte Ro, Rb, Rd;

            Ro = (byte)((Instruction & 0x01c0) >> 6);  // Offset Register
            Rb = (byte)((Instruction & 0x0038) >> 3);  // Base Register
            Rd = (byte)(Instruction & 0x0007);         // Source/Destination Register

            if ((Instruction & 0x0200) == 0)
            {
                // Load/Store with register offset
                this.Log(string.Format("Load/Store with register offset, Mem[R{0} + R{1}] <-> R{2}", Rb, Ro, Rd));
                bool LoadFromMemory, ByteQuantity;

                LoadFromMemory = (Instruction & 0x0800) > 0;
                ByteQuantity = (Instruction & 0x0400) > 0;

                uint Address = this.Registers[Rb] + this.Registers[Ro];

                if (!LoadFromMemory)
                {
                    if (ByteQuantity)
                        this.mem.SetByteAt(Address, (byte)this.Registers[Rd]);
                    else
                    {
                        // Forced align for STR is handled i memory read handler
                        this.mem.SetWordAt(Address, this.Registers[Rd]);
                    }

                    // STR instructions take 2N incremental cycles to execute. (no I cycles)
                    return 0;
                }
                else
                {
                    if (ByteQuantity)
                        this.Registers[Rd] = this.mem.GetByteAt(Address);
                    else
                    {
                        // memory alignment happens in memory access handler
                        uint Result = this.mem.GetWordAt(Address);
                        byte RotateAmount = (byte)((Address & 0x03) << 3);

                        // ROR result for misaligned addresses
                        if (RotateAmount != 0)
                            Result = ROR(Result, RotateAmount);

                        this.Registers[Rd] = Result;
                    }
                    // Normal LDR instructions take 1S + 1N + 1I (incremental)
                    return ICycle;
                }
            }
            else
            {
                // Load/Store sign-extended byte/halfword
                this.Log(string.Format("Load/Store sign-extended, Mem[R{0} + R{1}] <-> R{2}", Rb, Ro, Rd));
                bool HFlag, SignExtended;

                HFlag = (Instruction & 0x0800) > 0;
                SignExtended = (Instruction & 0x0400) > 0;

                uint Address = this.Registers[Rb] + this.Registers[Ro];

                if (!SignExtended)
                {
                    if (!HFlag)
                    {
                        // force align STRH is handled in memory read handler
                        this.mem.SetHalfWordAt(Address, (ushort)this.Registers[Rd]);

                        // STR instructions take 2N incremental cycles to execute. (No I cycles)
                        return 0;
                    }
                    else
                    {
                        if ((Address & 0x01) == 0)  // aligned
                            this.Registers[Rd] = this.mem.GetHalfWordAt(Address);
                        else
                        {
                            ushort value = this.mem.GetHalfWordAt(Address);
                            this.Registers[Rd] = (uint)(value << 24) | (uint)(value >> 8);
                        }
                            

                        // Normal LDR instructions take 1S + 1N + 1I (incremental)
                        return ICycle;
                    }
                }
                else
                {
                    if (!HFlag)
                        this.Registers[Rd] = (uint)(sbyte)this.mem.GetByteAt(Address);
                    else
                    {
                        if ((Address & 0x01) == 1)  // misaligned
                        {
                            this.Registers[Rd] = (uint)(sbyte)this.mem.GetByteAt(Address);
                        }
                        else
                        {
                            this.Registers[Rd] = (uint)(short)this.mem.GetHalfWordAt(Address);
                        }
                    }

                    // Normal LDR instructions take 1S + 1N + 1I (incremental)
                    return ICycle;
                }

            }
        }

        private int LoadStoreImmediate(ushort Instruction)
        {
            bool ByteQuantity, LoadFromMemory;
            byte Offset5, Rb, Rd;

            ByteQuantity = (Instruction & 0x1000) > 0;
            LoadFromMemory = (Instruction & 0x0800) > 0;
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);  // Offset value
            Rb = (byte)((Instruction & 0x0038) >> 3);       // Base Register
            Rd = (byte)(Instruction & 0x0007);              // Source/Destination Register

            this.Log(string.Format("Load/Store with immediate offset, Mem[R{0} + {1:x4} << 2] <-> R{2}", Rb, Offset5, Rd));

            /*
             For word accesses (B = 0), the value specified by #Imm is a full 7-bit address, but must
             be word-aligned (ie with bits 1:0 set to 0), since the assembler places #Imm >> 2 in
             the Offset5 field.
             (manual)
            */
            if (!ByteQuantity)
                Offset5 <<= 2;

            uint Address = this.Registers[Rb] + Offset5;

            if (LoadFromMemory)
            {
                if (ByteQuantity)
                    this.Registers[Rd] = this.mem.GetByteAt(Address);
                else
                {
                    // alignment happens in memory handler
                    uint Result = this.mem.GetWordAt(Address);
                    byte RotateAmount = (byte)((Address & 0x03) << 3);

                    // ROR result for misaligned adresses
                    if (RotateAmount != 0)
                        Result = ROR(Result, RotateAmount);

                    this.Registers[Rd] = Result;
                }

                // Normal LDR instructions take 1S + 1N + 1I (incremental)
                return ICycle;
            }
            else
            {
                if (ByteQuantity)
                    this.mem.SetByteAt(Address, (byte)this.Registers[Rd]);
                else
                {
                    // force align is handled in memory read handler
                    this.mem.SetWordAt(Address, this.Registers[Rd]);
                }

                // STR instructions take 2N incremental cycles to execute. (No I cycles)
                return 0;
            }
        }

        private int LoadStoreHalfword(ushort Instruction)
        {
            bool LoadFromMemory;
            byte Offset5, Rb, Rd;
            
            LoadFromMemory = (Instruction & 0x0800) > 0;
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);  // Offset value
            Rb = (byte)((Instruction & 0x0038) >> 3);       // Base Register
            Rd = (byte)(Instruction & 0x0007);              // Source/Destination Register

            this.Log(string.Format("Load/Store halfword, Mem[R{0} + {1:x4} << 1] <-> R{2}", Rb, Offset5, Rd));

            /*
             #Imm is a full 6-bit address but must be halfword-aligned (ie with bit 0 set to 0) since
             the assembler places #Imm >> 1 in the Offset5 field.
            */
            Offset5 <<= 1;
            uint Address = this.Registers[Rb] + Offset5;

            if (LoadFromMemory)
            {
                if ((Address & 0x01) == 0)  // aligned
                {
                    this.Registers[Rd] = this.mem.GetHalfWordAt(Address);
                }
                else
                {
                    ushort value = this.mem.GetHalfWordAt(Address);
                    this.Registers[Rd] = (uint)(value << 24) | (uint)(value >> 8);
                }

                // Normal LDR instructions take 1S + 1N + 1I (incremental)
                return ICycle;
            }
            else
            {
                // force align is handled in memory read handler
                this.mem.SetHalfWordAt(Address, (ushort)this.Registers[Rd]);

                // STR instructions take 2N incremental cycles to execute.
                return 0;
            }
        }

        private int LoadStoreSPRelative(ushort Instruction)
        {
            bool LoadFromMemory;
            byte Rd;
            uint Word8;

            LoadFromMemory = (Instruction & 0x0800) > 0;
            Rd = (byte)((Instruction & 0x0700) >> 8);
            /*
             The offset supplied in #Imm is a full 10-bit address, but must always be word-aligned
             (ie bits 1:0 set to 0), since the assembler places #Imm >> 2 in the Word8 field.
            */
            Word8 = (uint)(Instruction & 0x00ff) << 2;
            uint Address = SP + Word8;

            this.Log(string.Format("Load/Store SP-relative, Mem[SP + {0:x4} << 1] <-> R{1}", Word8, Rd));

            if (LoadFromMemory)
            {
                // If address is misaligned by a half-word amount, garbage is fetched into the upper 2 bits. (GBATek)
                // force align is handled in memory read handler
                uint Result = this.mem.GetWordAt(Address);
                byte RotateAmount = (byte)((Address & 0x03) << 3);

                // ROR result for misaligned adresses
                if (RotateAmount != 0)
                    Result = ROR(Result, RotateAmount);

                this.Registers[Rd] = Result;

                // Normal LDR instructions take 1S + 1N + 1I (incremental)
                return ICycle;
            }
            else
            {
                // force align happens in read handler
                this.mem.SetWordAt(Address, this.Registers[Rd]);

                // STR instructions take 2N incremental cycles to execute.
                return 0;
            }
        }

        private int LoadAddress(ushort Instruction)
        {
            bool Source;
            byte Rd;
            uint Word8;

            Source = (Instruction & 0x0800) > 0;
            Rd = (byte)((Instruction & 0x0700) >> 8);

            /*
             The value specified by #Imm is a full 10-bit value, but this must be word-aligned (ie
             with bits 1:0 set to 0) since the assembler places #Imm >> 2 in field Word8.
            */
            Word8 = (uint)(Instruction & 0x00ff) << 2;

            this.Log(string.Format("Load Address, SP/PC + {0:x4} -> R{1}", Word8, Rd));
            if (Source)
            {
                // Use SP as source
                this.Registers[Rd] = SP + Word8;
            }
            else
            {
                /*
                 Where the PC is used as the source register (SP = 0), bit 1 of the PC is always read
                 as 0. The value of the PC will be 4 bytes greater than the address of the instruction
                 before bit 1 is forced to 0.  
                 
                 My PC is always 4 bytes ahead in THUMB mode, so I don't need to account for this
                */
                this.Registers[Rd] = (PC & 0xffff_fffd) + Word8;
            }

            // Normal LDR instructions take 1S + 1N + 1I (incremental)
            return ICycle;
        }

        private int MultipleLoadStore(ushort Instruction)
        {
            this.Log("Multiple Load/Store");
             
            bool LoadFromMemory;
            byte Rb, RList;

            LoadFromMemory = (Instruction & 0x0800) > 0;
            Rb = (byte)((Instruction & 0x0700) >> 8);
            RList = (byte)(Instruction & 0x00ff);

            uint Address = this.Registers[Rb];
            
            if (RList == 0)
            {
                /*
                 Strange Effects on Invalid Rlist's
                 Empty Rlist: R15 loaded/stored (ARMv4 only), and Rb=Rb+40h (ARMv4-v5).
                 (GBATek)
                */
                if (LoadFromMemory)
                {
                    PC = this.mem.GetWordAt(Address);
                    this.PipelineFlush();
                }
                else
                {
                    this.mem.SetWordAt(Address, PC + 2);  // My PC is 4 ahead, but it should be 6 in this case
                }
                    
                // Writeback
                this.Registers[Rb] += 0x40;
            }
            else if (LoadFromMemory)
            {
                byte RegisterCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                    {
                        this.Registers[i] = this.mem.GetWordAt(Address);
                        this.Log(string.Format("{0:x8} -> R{1} from {2:x8}", this.Registers[i], i, Address));
                        Address += 4;

                        RegisterCount++;
                    }
                }
                this.Registers[Rb] = Address;  // return misaligned
            }
            else
            {
                // Writeback with Rb included in Rlist:
                // Store OLD base if Rb is FIRST entry in Rlist, otherwise store NEW base (STM/ARMv4)
                sRegisterList RegisterQueue = new sRegisterList(8);
                for (byte i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                        RegisterQueue.Enqueue(i);
                }

                // we know that the queue is not empty, because RList != 0
                if (RegisterQueue.Peek() == Rb)
                {
                    this.Log(string.Format("{0:x8} -> MEM${1:x8} from R{2}", this.Registers[Rb], Address, Rb));
                    this.mem.SetWordAt(Address, this.Registers[Rb]);
                    Address += 4;
                    RegisterQueue.Dequeue();
                }

                // Writeback, we want to write Rb as the new value if it is not the first to be written
                this.Registers[Rb] = (Address + 4 * (uint)RegisterQueue.Count);

                while (RegisterQueue.Count > 0)
                {
                    this.Log(string.Format("{0:x8} -> MEM${1:x8} from R{2}", this.Registers[RegisterQueue.Peek()], Address, RegisterQueue.Peek()));
                    this.mem.SetWordAt(Address, this.Registers[RegisterQueue.Dequeue()]);
                    Address += 4;
                }
            }

            return LoadFromMemory ? ICycle : 0;
        }
    }
}
