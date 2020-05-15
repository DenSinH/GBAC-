using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void BlockDataTransfer(uint Instruction)
        {
            this.Log("Block Data Transfer");

            bool PreIndex, Up, PSR_ForceUser, WriteBack, LoadFromMemory;
            byte Rn;  // Base register
            ushort RegisterList;

            PreIndex = (Instruction & 0x0100_0000) > 0;
            Up = (Instruction & 0x0080_0000) > 0;
            PSR_ForceUser = (Instruction & 0x0040_0000) > 0;
            WriteBack = (Instruction & 0x0020_0000) > 0;
            LoadFromMemory = (Instruction & 0x0010_0000) > 0;

            Rn = (byte)((Instruction & 0x000f_0000) >> 16);
            RegisterList = (ushort)(Instruction & 0x0000_ffff);

            /*
             When S Bit is set (S=1)
                If instruction is LDM and R15 is in the list: (Mode Changes)
                  While R15 loaded, additionally: CPSR=SPSR_<current mode>
                Otherwise: (User bank transfer)
                  Rlist is referring to User Bank Registers, R0-R15 (rather than
                  register related to the current mode, such like R14_svc etc.)
                  Base write-back should not be used for User bank transfer.
                  Caution - When instruction is LDM:
                  If the following instruction reads from a banked register (eg. R14_svc),
                  then CPU might still read R14 instead; if necessary insert a dummy NOP.

            (GBATek)

            I don't know what the top part means, but the bottom part we can do
            */
            Mode OldMode = Mode.User;  // some random default value, it doesn't matter what we put here
            if (PSR_ForceUser)
            {
                OldMode = this.mode;
                this.ChangeMode(Mode.User);
            }

            /*
             Whenever R15 is stored to memory the stored value is the address of the STM
             instruction plus 12.

             So because our PC is always ahead by exactly 8, we must increase this value by 4
            */

            // todo: misaligned addresses
            uint StartAddress = this.Registers[Rn];
            // R15 should not be used as the base register in any LDM or STM instruction

            if (RegisterList == 0)  // Invalid Register lists (see https://problemkaputt.de/gbatek.htm#armopcodesmemoryblockdatatransferldmstm)
            {
                if (LoadFromMemory)
                    this.PC = this.GetAt<uint>(StartAddress);
                else
                {
                    if (Up)
                    {
                        if (PreIndex)
                            this.SetAt<uint>(StartAddress + 4, this.PC + 4);
                        else
                            this.SetAt<uint>(StartAddress, this.PC + 4);
                    }
                    else
                    {
                        if (PreIndex)
                            this.SetAt<uint>(StartAddress - 0x40, this.PC + 4);
                        else
                            this.SetAt<uint>(StartAddress - 0x3c, this.PC + 4);
                    }
                }

                if (WriteBack)
                    this.Registers[Rn] = Up ? StartAddress + 0x40 : StartAddress - 0x40;
            }
            else
            {
                Queue<byte> RegisterQueue = new Queue<byte>(16);
                for (byte i = 0; i < 16; i++)
                {
                    if ((RegisterList & (1 << i)) > 0)
                        RegisterQueue.Enqueue(i);
                }

                if (!Up)
                {
                    // We start stacking from the bottom
                    StartAddress -= (uint)RegisterQueue.Count * 4;

                    // Stacking in reverse causes pre-decrement to behave like post-increment
                    PreIndex = !PreIndex;
                }

                uint CurrentAddress = StartAddress;

                if (!LoadFromMemory)
                {
                    // Writeback with Rb included in Rlist: Store OLD base if Rb is FIRST entry in Rlist, otherwise store NEW base (STM/ARMv4)
                    // (GBATek)

                    // Here we handle Rn being the first element to store
                    if (RegisterQueue.Peek() == Rn)
                    {
                        if (PreIndex)
                            this.SetAt<uint>(CurrentAddress + 4, this.Registers[Rn]);
                        else
                            this.SetAt<uint>(CurrentAddress, this.Registers[Rn]);
                        CurrentAddress += 4;
                        RegisterQueue.Dequeue();
                    }
                }

                // so we must set Rn on the case of writeback in case we store it later
                // In case of a load, Rn is overwritten with the loaded value, so we can do it here too
                // If no registers were loaded/stored, the writeback was already handled
                if (WriteBack)
                    this.Registers[Rn] = StartAddress;

                byte Register = 0;
                while (RegisterQueue.Count > 0)
                {
                    Register = RegisterQueue.Dequeue();
                    if (PreIndex)
                        CurrentAddress += 4;  // always +4 because we start from the bottom in case of decr.

                    if (LoadFromMemory)
                        this.Registers[Register] = this.GetAt<uint>(CurrentAddress);
                    else  
                        // PC is 8 ahead, while it should be 12
                        this.SetAt<uint>(CurrentAddress, this.Registers[Register + ((Register == 15) ? 0 : 4)]);

                    if (!PreIndex)
                        CurrentAddress += 4;
                }
            }

            if (PSR_ForceUser)
            {
                this.ChangeMode(OldMode);
            }
            
        }
    }
}
