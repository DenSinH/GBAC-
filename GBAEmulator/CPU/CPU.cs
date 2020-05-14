using System;
using System.IO;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    public partial class ARM7TDMI
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        State state;
        readonly Queue<uint> Pipeline = new Queue<uint>(3);
        public ARM7TDMI()
        {
            this.SystemBank = new uint[16];
            this.FIQBank = new uint[16];
            this.SupervisorBank = new uint[16];
            this.AbortBank = new uint[16];
            this.IRQBank = new uint[16];
            this.UndefinedBank = new uint[16];
            this.state = State.ARM;

            this.CPSR = 0x0000005F;
        }

        public void LoadRom(string FileName)
        {
            FileStream fs = File.OpenRead(FileName);
            int current = fs.ReadByte();
            uint i = 0;

            while (current != -1)
            {
                this.GamePak[i] = (byte)current;
                current = fs.ReadByte();
                i++;
            }
            Console.WriteLine(string.Format("{0:x8} Bytes loaded (hex)", i));
        }

        private void PipelineFlush()
        {
            // Has to be called when running! 
            this.Pipeline.Clear();
        }

        private void Step()
        {
            // Only ARM mode for now
            this.Pipeline.Enqueue(this.GetAt<uint>(this.PC));
            this.PC += 4;

            if (this.Pipeline.Count == 2)
            {
                this.ExecuteARM(this.Pipeline.Dequeue());
            }
        }
    }
}
