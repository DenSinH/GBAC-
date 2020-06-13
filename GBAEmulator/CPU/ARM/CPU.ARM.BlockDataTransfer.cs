using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int BlockDataTransfer(uint Instruction)
        {
            this.Log("Block Data Transfer");

            bool PreIndex, Up, PSR_ForceUser, WriteBack, LoadFromMemory;
            byte Rn;  // Base register
            ushort RegisterList;
            int Cycles;

            PreIndex       = (Instruction & 0x0100_0000) > 0;
            Up             = (Instruction & 0x0080_0000) > 0;
            PSR_ForceUser  = (Instruction & 0x0040_0000) > 0;
            WriteBack      = (Instruction & 0x0020_0000) > 0;
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
            Mode OldMode = this.mode;
            if (PSR_ForceUser)
            {
                this.ChangeMode(Mode.User);
            }

            /*
             Whenever R15 is stored to memory the stored value is the address of the STM
             instruction plus 12. (manual)

             So because our PC is always ahead by exactly 8, we must increase this value by 4
            */
            
            uint StartAddress = this.Registers[Rn];
            uint OriginalAddress = StartAddress;
            // R15 should not be used as the base register in any LDM or STM instruction

            if (RegisterList == 0)  // Invalid Register lists (see https://problemkaputt.de/gbatek.htm#armopcodesmemoryblockdatatransferldmstm)
            {
                if (LoadFromMemory)
                {
                    this.PC = this.mem.GetWordAt(StartAddress);
                    this.PipelineFlush();
                }
                else
                {
                    if (Up)
                    {
                        if (PreIndex)
                            this.mem.SetWordAt(StartAddress + 4, this.PC + 4);
                        else
                            this.mem.SetWordAt(StartAddress, this.PC + 4);
                    }
                    else
                    {
                        if (PreIndex)
                            this.mem.SetWordAt(StartAddress - 0x40, this.PC + 4);
                        else
                            this.mem.SetWordAt(StartAddress - 0x3c, this.PC + 4);
                    }
                }

                if (WriteBack)
                    this.Registers[Rn] = Up ? OriginalAddress + 0x40 : OriginalAddress - 0x40;

                Cycles = SCycle;  // todo: find out
            }
            else
            {
                sRegisterList RegisterQueue = new sRegisterList(16);  // at most 16 registers to store
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
                    // Normal LDM instructions take nS + 1N + 1I
                    Cycles = (byte)(RegisterQueue.Count * SCycle + NCycle + ICycle);

                    // Writeback with Rb included in Rlist: Store OLD base if Rb is FIRST entry in Rlist, otherwise store NEW base (STM/ARMv4)
                    // (GBATek)

                    // Here we handle Rn being the first element to store
                    if (RegisterQueue.Peek() == Rn)
                    {
                        if (PreIndex)
                            this.mem.SetWordAt(CurrentAddress + 4, this.Registers[Rn]);
                        else
                            this.mem.SetWordAt(CurrentAddress, this.Registers[Rn]);
                        CurrentAddress += 4;
                        OriginalAddress = (uint)(OriginalAddress + (Up? 4 : -4));  // for writeback
                        RegisterQueue.Dequeue();
                    }
                }
                else
                {
                    // STM instructions take (n-1)S + 2N incremental cycles to execute
                    Cycles = (byte)((RegisterQueue.Count - 1) * SCycle + (NCycle << 1));
                }

                // so we must set Rn on the case of writeback in case we store it later
                // In case of a load, Rn is overwritten with the loaded value, so we can do it here too
                // If no registers were loaded/stored, the writeback was already handled
                if (WriteBack)
                    this.Registers[Rn] = Up ? (uint)(OriginalAddress + 4 * RegisterQueue.Count) : (uint)(OriginalAddress - 4 * RegisterQueue.Count);

                byte Register = 0;
                while (RegisterQueue.Count > 0)
                {
                    Register = RegisterQueue.Dequeue();
                    if (PreIndex)
                        CurrentAddress += 4;  // always +4 because we start from the bottom in case of decr.

                    if (LoadFromMemory)
                    {
                        this.Registers[Register] = this.mem.GetWordAt(CurrentAddress);
                        this.Log(string.Format("{0:x8} -> R{1} from {2:x8}", this.Registers[Register], Register, CurrentAddress));
                    }
                    else
                    {
                        this.mem.SetWordAt(CurrentAddress, this.Registers[Register]);
                        this.Log(string.Format("{0:x8} -> MEM${1:x8} from R{2}", this.Registers[Register], CurrentAddress, Register));
                    }

                    if (!PreIndex)
                        CurrentAddress += 4;
                }

                if (Register == 15)
                {
                    if (LoadFromMemory)
                    {
                        //  LDM PC takes (n+1)S + 2N + 1I incremental cycles
                        Cycles += SCycle + NCycle;

                        this.PipelineFlush();  // Flush pipeline when changing PC
                    }
                    else
                        // PC is 8 ahead, while it should be 12
                        this.mem.SetWordAt(CurrentAddress - (uint)((!PreIndex) ? 4 : 0), this.Registers[15] + 4);
                }
                    
            }

            if (PSR_ForceUser)
            {
                this.ChangeMode(OldMode);
            }

            return Cycles;
        }
    }
}
