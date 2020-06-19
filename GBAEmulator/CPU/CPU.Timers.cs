using System;
using System.Runtime.CompilerServices;
using GBAEmulator.Audio.Channels;
using GBAEmulator.IO;
using GBAEmulator.Scheduler;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void InitTimers()
        {
            this.Timers = new cTimer[4];
            this.Timers[3] = new cTimer(this, 3);
            this.Timers[2] = new cTimer(this, 2, this.Timers[3]);
            this.Timers[1] = new cTimer(this, 1, this.Timers[2]);
            this.Timers[0] = new cTimer(this, 0, this.Timers[1]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickTimers(int cycles)
        {
            for (int i = 0; i < 4; i++) this.Timers[i].Tick(cycles);
        }

        public class cTimer
        {
            private readonly cTimer Next;

            private readonly ARM7TDMI cpu;
            private readonly int index;

            public readonly cTMCNT_L Data;
            public readonly cTMCNT_H Control;
            public readonly FIFOChannel[] FIFO = new FIFOChannel[2];
            private readonly bool IsSound;

            public cTimer(ARM7TDMI cpu, int index)
            {
                this.cpu = cpu;
                this.index = index;
                this.Data = new cTMCNT_L();
                this.Control = new cTMCNT_H(this.Data);
                this.IsSound = index == 0 || index == 1;
            }

            public cTimer(ARM7TDMI cpu, int index, cTimer Next) : this(cpu, index)
            {
                this.Next = Next;
            }

            public void TickDirect(int cycles)
            {
                // countup tick calls
                this.Data.TickUnscaled((ushort)cycles);
            }

            public void Tick(int cycles)
            {
                if (!this.Control.Enabled || this.Control.CountUpTiming)
                    return;

                if (!this.Data.Tick((ushort)cycles))  // overflow
                    return;  // no overflow means no other action

                if (this.Next?.Control.CountUpTiming ?? false)
                    this.Next?.TickDirect(1);

                if (this.IsSound)
                {
                    this.FIFO[0]?.TimerOverflow();
                    this.FIFO[1]?.TimerOverflow();
                }

                if (this.Control.TimerIRQEnable)
                {
                    this.cpu.IO.IF.Request((Interrupt)((ushort)Interrupt.TimerOverflow << this.index));
                }
            }
        }

        public cTimer[] Timers;
    }
}
