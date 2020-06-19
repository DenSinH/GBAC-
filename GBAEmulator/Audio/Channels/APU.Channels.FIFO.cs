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

        public FIFOChannel(ARM7TDMI cpu)
        {
            this.cpu = cpu;
        }

        public void TimerOverflow()
        {
            if (this.Queue.Count > 0)
            {
                this.CurrentSample = (short)((sbyte)this.Queue.Dequeue() << 8);
            }

            if (this.Queue.Count <= 16)
            {
                this.cpu.TriggerDMASpecial(1);
                this.cpu.TriggerDMASpecial(2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            this.Queue.Clear();
        }
    }
}
