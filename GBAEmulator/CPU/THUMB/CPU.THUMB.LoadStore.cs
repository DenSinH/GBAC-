using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void LoadStoreRegOffset_SignExtended(ushort Instruction)
        {
            // Contains both the instructions for Load/Store with Register offset, and for Load/Store sign-extended

            byte Ro, Rb, Rd;

            Ro = (byte)((Instruction & 0x01c0) >> 6);  // Offset Register
            Rb = (byte)((Instruction & 0x0038) >> 3);  // Base Register
            Rd = (byte)(Instruction & 0x0007);         // Source/Destination Register

            if ((Instruction & 0x0200) > 0)
            {
                // Load/Store with register offset
                this.Log("THUMB Load/Store with register offset");
                bool LoadFromMemory, ByteQuantity;

                LoadFromMemory = (Instruction & 0x0800) > 0;
                ByteQuantity = (Instruction & 0x0400) > 0;

                uint Address = this.Registers[Rb] + this.Registers[Ro];
                if (LoadFromMemory)
                {
                    if (ByteQuantity)
                        this.SetAt<byte>(Address, (byte)this.Registers[Rd]);
                    else
                        this.SetAt<uint>(Address, this.Registers[Rd]);
                }
                else
                {
                    if (ByteQuantity)
                        this.Registers[Rd] = this.GetAt<byte>(Address);
                    else
                        this.Registers[Rd] = this.GetAt<uint>(Address);
                }
            }
            else
            {
                // Load/Store sign-extended byte/halfword
                this.Log("THUMB Load/Store sign-extended byte/halfword");
                bool HFlag, SignExtended;

                HFlag = (Instruction & 0x0800) > 0;
                SignExtended = (Instruction & 0x0400) > 0;

                uint Address = this.Registers[Rb] + this.Registers[Ro];

                if (!SignExtended)
                {
                    if (!HFlag)
                        this.SetAt<ushort>(Address, (ushort)this.Registers[Rd]);
                    else
                        this.Registers[Rd] = this.GetAt<ushort>(Address);
                }
                else
                {
                    if (!HFlag)
                        this.Registers[Rd] = (uint)(sbyte)this.GetAt<byte>(Address);
                    else
                        this.Registers[Rd] = (uint)(short)this.GetAt<ushort>(Address);
                }

            }
        }

        private void LoadStoreImmediate(ushort Instruction)
        {
            bool ByteQuantity, LoadFromMemory;
            byte Offset5, Rb, Rd;

            ByteQuantity = (Instruction & 0x1000) > 0;
            LoadFromMemory = (Instruction & 0x0800) > 0;
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);  // Offset value
            Rb = (byte)((Instruction & 0x07c0) >> 6);       // Base Register
            Rd = (byte)(Instruction & 0x0007);              // Source/Destination Register

            /*
             For word accesses (B = 0), the value specified by #Imm is a full 7-bit address, but must
             be word-aligned (ie with bits 1:0 set to 0), since the assembler places #Imm >> 2 in
             the Offset5 field.
            */
            if (!ByteQuantity)
                Offset5 <<= 2;

            uint Address = this.Registers[Rb] + Offset5;

            if (LoadFromMemory)
            {
                if (ByteQuantity)
                    this.Registers[Rd] = this.GetAt<byte>(Address);
                else
                    this.Registers[Rd] = this.GetAt<uint>(Address);
            }
            else
            {
                if (ByteQuantity)
                    this.SetAt<byte>(Address, (byte)this.Registers[Rd]);
                else
                    this.SetAt<uint>(Address, this.Registers[Rd]);
            }
        }

        private void LoadStoreHalfword(ushort Instruction)
        {
            bool LoadFromMemory;
            byte Offset5, Rb, Rd;
            
            LoadFromMemory = (Instruction & 0x0800) > 0;
            Offset5 = (byte)((Instruction & 0x07c0) >> 6);  // Offset value
            Rb = (byte)((Instruction & 0x0038) >> 3);       // Base Register
            Rd = (byte)(Instruction & 0x0007);              // Source/Destination Register

            /*
             #Imm is a full 6-bit address but must be halfword-aligned (ie with bit 0 set to 0) since
             the assembler places #Imm >> 1 in the Offset5 field.
            */
            Offset5 <<= 1;
            uint Address = this.Registers[Rb] + Offset5;

            if (LoadFromMemory)
                this.Registers[Rd] = this.GetAt<ushort>(Address);
            else
                this.SetAt<ushort>(Address, (ushort)this.Registers[Rd]);
        }

        private void LoadStoreSPRelative(ushort Instruction)
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

            if (LoadFromMemory)
                this.Registers[Rd] = this.GetAt<uint>(SP + Word8);
            else
                this.SetAt<uint>(SP + Word8, this.Registers[Rd]);
        }

        private void LoadAddress(ushort Instruction)
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
        }

        private void MultipleLoadStore(ushort Instruction)
        {
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
                */
                if (LoadFromMemory)
                    PC = this.GetAt<uint>(Address) | 1;
                else
                    this.SetAt<uint>(Address, PC + 2);  // My PC is 4 ahead, but it should be 6 in this case

                // Writeback
                this.Registers[Rb] += 0x40;
            }
            else if (LoadFromMemory)
            {
                for (byte i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                    {
                        this.Registers[i] = this.GetAt<uint>(Address);
                        Address += 4;
                    }
                }
                // No writeback in this case
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

                // we know that the queue is not empty, because RList != 0
                if (RegisterQueue.Peek() == Rb)
                {
                    this.SetAt<uint>(Address, this.Registers[Rb]);
                    Address += 4;
                    RegisterQueue.Dequeue();
                }

                // Writeback, we want to write Rb as the new value if it is not the first to be written
                this.Registers[Rb] = Address + 4 * (uint)RegisterQueue.Count;

                while (RegisterQueue.Count > 0)
                {
                    this.SetAt<uint>(Address, this.Registers[RegisterQueue.Dequeue()]);
                    Address += 4;
                }
            }
        }
    }
}
