using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void PushPopRegisters(ushort Instructions)
        {
            this.Log("THUMB Push/Pop registers");
            bool LoadFromMemory, PCLR;
            byte RList;

            LoadFromMemory = (Instructions & 0x0800) > 0;
            PCLR = (Instructions & 0x0100) > 0;
            RList = (byte)(Instructions & 0x00ff);

            if (LoadFromMemory)
            {
                // Pop from stack
                for (byte i = 0; i < 8; i++)
                {
                    if ((RList & (1 << i)) > 0)
                    {
                        // Pop from stack
                        this.Registers[i] = this.GetAt<uint>(SP);
                        SP += 4;
                    }
                }

                if (PCLR)
                {
                    PC = this.GetAt<uint>(SP);
                    SP += 4;
                    this.PipelineFlush();
                }
            }
            else
            {
                // Reverse pushing, like in ARM block data transfer instruction.
                Queue<byte> RegisterQueue = new Queue<byte>(9);
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
                    this.SetAt<uint>(Address, this.Registers[RegisterQueue.Dequeue()]);
                    Address += 4;
                }
            }
        }
    }
}
