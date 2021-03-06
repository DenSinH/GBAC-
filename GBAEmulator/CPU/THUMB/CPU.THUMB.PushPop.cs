﻿using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int PushPopRegisters(ushort Instructions)
        {
            this.Log("Push/Pop registers");
            bool LoadFromMemory, PCLR;
            byte RList;

            LoadFromMemory = (Instructions & 0x0800) > 0;
            PCLR = (Instructions & 0x0100) > 0;
            RList = (byte)(Instructions & 0x00ff);

            if (LoadFromMemory)
            {
                byte RegisterCount = 0;

                // Pop from stack
                for (int i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                    {
                        // Pop from stack
                        this.Log(string.Format("POP Mem[{0:x8}] -> R{1}", SP, i));
                        this.Registers[i] = this.mem.GetWordAt(SP);
                        SP += 4;
                        RegisterCount++;
                    }
                }

                if (PCLR)
                {
                    PC = this.mem.GetWordAt(SP) & 0xffff_fffe;
                    SP += 4;
                    this.PipelineFlush();
                }
                return ICycle;
            }
            else
            {
                // Reverse pushing, like in ARM block data transfer instruction.
                sRegisterList RegisterQueue = new sRegisterList(9);
                for (byte i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                    {
                        // Pop from stack
                        RegisterQueue.Enqueue(i);
                    }
                }

                if (PCLR)
                    RegisterQueue.Enqueue(14);  // Also push link register

                SP -= 4 * (uint)RegisterQueue.Count;
                uint Address = SP;
                while (RegisterQueue.Count > 0)
                {
                    this.Log(string.Format("PUSH R{1} -> Mem[{0:x8}]", Address, RegisterQueue.Peek()));
                    this.mem.SetWordAt(Address, this.Registers[RegisterQueue.Dequeue()]);
                    Address += 4;
                }

                return 0;
            }
        }
    }
}
