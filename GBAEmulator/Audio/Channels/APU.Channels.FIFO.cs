using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using GBAEmulator.CPU;

namespace GBAEmulator.Audio.Channels
{
    public class FIFOChannel : IChannel
    {
        public short CurrentSample { get; private set; }
        public Queue<byte> Queue = new Queue<byte>(32);
        private readonly ARM7TDMI cpu;
        private readonly int FIFODATAAddr;

        public FIFOChannel(ARM7TDMI cpu, int FIFODATAAddr)
        {
            this.cpu = cpu;
            this.FIFODATAAddr = FIFODATAAddr;
        }

        public void TimerOverflow()
        {
            if (this.Queue.Count > 0)
            {
                this.CurrentSample = (short)((sbyte)this.Queue.Dequeue() << 8);
            }

            if (this.Queue.Count < 16)
            {
                if (this.cpu.DMAChannels[1].DAD == this.FIFODATAAddr)
                {
                    this.cpu.DMAChannels[1].Trigger(DMAStartTiming.Special);
                }
                else if (this.cpu.DMAChannels[2].DAD == this.FIFODATAAddr)
                {
                    this.cpu.DMAChannels[2].Trigger(DMAStartTiming.Special);
                }
            }
            else if (this.Queue.Count > 32)
            {
                Console.Error.WriteLine("FIFO Channel queue overfilled");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            this.Queue.Clear();
        }
    }
}
