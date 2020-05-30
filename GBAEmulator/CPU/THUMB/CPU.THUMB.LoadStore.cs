using System;
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
                        this.SetByteAt(Address, (byte)this.Registers[Rd]);
                    else
                    {
                        Address &= 0xffff_fffc;  // Forced align for STR
                        this.SetWordAt(Address, this.Registers[Rd]);
                    }

                    // STR instructions take 2N incremental cycles to execute.
                    return NCycle << 1;
                }
                else
                {
                    if (ByteQuantity)
                        this.Registers[Rd] = this.GetByteAt(Address);
                    else
                    {
                        uint Result = this.GetWordAt(Address & 0xffff_fffc);
                        byte RotateAmount = (byte)((Address & 0x03) << 3);

                        // ROR result for misaligned addresses
                        if (RotateAmount != 0)
                            Result = this.ROR(Result, RotateAmount);

                        this.Registers[Rd] = Result;
                    }
                    // Normal LDR instructions take 1S + 1N + 1I (incremental)
                    return SCycle + NCycle + ICycle;
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
                        Address &= 0xffff_fffe;  // force align STRH
                        this.SetHalfWordAt(Address, (ushort)this.Registers[Rd]);

                        // STR instructions take 2N incremental cycles to execute.
                        return NCycle << 1;
                    }
                    else
                    {
                        if ((Address & 0x01) == 0)  // aligned
                            this.Registers[Rd] = this.GetHalfWordAt(Address);
                        else
                            this.Registers[Rd] = (uint)(this.GetByteAt(Address - 1) << 24) | this.GetByteAt(Address);

                        // Normal LDR instructions take 1S + 1N + 1I (incremental)
                        return SCycle + NCycle + ICycle;
                    }
                }
                else
                {
                    if (!HFlag)
                        this.Registers[Rd] = (uint)(sbyte)this.GetByteAt(Address);
                    else
                    {
                        if ((Address & 0x01) == 1)  // misaligned
                        {
                            this.Registers[Rd] = (uint)(sbyte)this.GetByteAt(Address);
                        }
                        else
                        {
                            this.Registers[Rd] = (uint)(short)this.GetHalfWordAt(Address);
                        }
                    }

                    // Normal LDR instructions take 1S + 1N + 1I (incremental)
                    return SCycle + NCycle + ICycle;
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
                    this.Registers[Rd] = this.GetByteAt(Address);
                else
                {
                    uint Result = this.GetWordAt(Address & 0xffff_fffc);
                    byte RotateAmount = (byte)((Address & 0x03) << 3);

                    // ROR result for misaligned adresses
                    if (RotateAmount != 0)
                        Result = this.ROR(Result, RotateAmount);

                    this.Registers[Rd] = Result;
                }

                // Normal LDR instructions take 1S + 1N + 1I (incremental)
                return SCycle + NCycle + ICycle;
            }
            else
            {
                if (ByteQuantity)
                    this.SetByteAt(Address, (byte)this.Registers[Rd]);
                else
                {
                    Address &= 0xffff_fffe;  // force align
                    this.SetWordAt(Address, this.Registers[Rd]);
                }

                // STR instructions take 2N incremental cycles to execute.
                return NCycle << 1;
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
                    this.Registers[Rd] = this.GetHalfWordAt(Address);
                else
                    this.Registers[Rd] = (uint)(this.GetByteAt(Address - 1) << 24) | this.GetByteAt(Address);

                // Normal LDR instructions take 1S + 1N + 1I (incremental)
                return SCycle + NCycle + ICycle;
            }
            else
            {
                Address &= 0xffff_fffe;  // force align
                this.SetHalfWordAt(Address, (ushort)this.Registers[Rd]);

                // STR instructions take 2N incremental cycles to execute.
                return NCycle << 1;
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
                uint Result = this.GetWordAt(Address & 0xffff_fffc);
                byte RotateAmount = (byte)((Address & 0x03) << 3);

                // ROR result for misaligned adresses
                if (RotateAmount != 0)
                    Result = this.ROR(Result, RotateAmount);

                this.Registers[Rd] = Result;

                // Normal LDR instructions take 1S + 1N + 1I (incremental)
                return SCycle + NCycle + ICycle;
            }
            else
            {
                Address &= 0xffff_fffc;  // force align
                this.SetWordAt(SP + Word8, this.Registers[Rd]);

                // STR instructions take 2N incremental cycles to execute.
                return NCycle << 1;
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
            return SCycle + NCycle + ICycle;
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
            byte Misalignment = (byte)(Address & 0x03);  // store misalignment for writeback
            Address = Address & 0xffff_fffc;  // force align

            if (RList == 0)
            {
                /*
                 Strange Effects on Invalid Rlist's
                 Empty Rlist: R15 loaded/stored (ARMv4 only), and Rb=Rb+40h (ARMv4-v5).
                 (GBATek)
                */
                if (LoadFromMemory)
                {
                    PC = this.GetWordAt(Address);
                    this.PipelineFlush();
                }
                else
                    this.SetWordAt(Address, PC + 2);  // My PC is 4 ahead, but it should be 6 in this case

                // Writeback
                this.Registers[Rb] += 0x40;

                return SCycle;  // todo: figure out actual timings
            }
            else if (LoadFromMemory)
            {
                byte RegisterCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                    {
                        this.Registers[i] = this.GetWordAt(Address);
                        this.Log(string.Format("{0:x8} -> R{1} from {2:x8}", this.Registers[i], i, Address));
                        Address += 4;

                        RegisterCount++;
                    }
                }
                this.Registers[Rb] = Address | Misalignment;  // return misalignment

                // Normal LDM instructions take nS + 1N + 1I
                return RegisterCount * SCycle + NCycle + ICycle;
            }
            else
            {
                // Writeback with Rb included in Rlist:
                // Store OLD base if Rb is FIRST entry in Rlist, otherwise store NEW base (STM/ARMv4)
                Queue<byte> RegisterQueue = new Queue<byte>(8);
                for (byte i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                        RegisterQueue.Enqueue(i);
                }

                // STM instructions take (n-1)S + 2N incremental cycles to execute
                int Cycles =(RegisterQueue.Count - 1) * SCycle + (NCycle << 1);

                // we know that the queue is not empty, because RList != 0
                if (RegisterQueue.Peek() == Rb)
                {
                    this.Log(string.Format("{0:x8} -> MEM${1:x8} from R{2}", this.Registers[Rb], Address, Rb));
                    this.SetWordAt(Address, this.Registers[Rb]);
                    Address += 4;
                    RegisterQueue.Dequeue();
                }

                // Writeback, we want to write Rb as the new value if it is not the first to be written
                this.Registers[Rb] = (Address + 4 * (uint)RegisterQueue.Count) | Misalignment;

                while (RegisterQueue.Count > 0)
                {
                    this.Log(string.Format("{0:x8} -> MEM${1:x8} from R{2}", this.Registers[RegisterQueue.Peek()], Address, RegisterQueue.Peek()));
                    this.SetWordAt(Address, this.Registers[RegisterQueue.Dequeue()]);
                    Address += 4;
                }

                return Cycles;
            }
        }
    }
}
