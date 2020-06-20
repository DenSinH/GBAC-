using System;
using System.Runtime.CompilerServices;
using GBAEmulator.Audio.Channels;
using GBAEmulator.IO;
using GBAEmulator.Scheduler;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void InitTimers(Scheduler.Scheduler scheduler)
        {
            this.Timers = new cTimer[4];
            this.Timers[3] = new cTimer(this, scheduler, 3);
            this.Timers[2] = new cTimer(this, scheduler, 2, this.Timers[3]);
            this.Timers[1] = new cTimer(this, scheduler, 1, this.Timers[2]);
            this.Timers[0] = new cTimer(this, scheduler, 0, this.Timers[1]);
        }

        public class cTimer
        {
            private readonly cTimer Next;

            private readonly ARM7TDMI cpu;
            private readonly Scheduler.Scheduler scheduler;
            private int NextOverflow;
            private bool HasOverflowEvent;
            private readonly int index;

            public readonly cTMCNT_L Data;
            public readonly cTMCNT_H Control;
            public readonly FIFOChannel[] FIFO = new FIFOChannel[2];
            private readonly bool IsSound;

            public cTimer(ARM7TDMI cpu, Scheduler.Scheduler scheduler, int index)
            {
                this.cpu = cpu;
                this.scheduler = scheduler;
                this.index = index;
                this.Data = new cTMCNT_L(cpu);
                this.Control = new cTMCNT_H(this, this.Data);
                this.IsSound = index == 0 || index == 1;
            }

            public cTimer(ARM7TDMI cpu, Scheduler.Scheduler scheduler, int index, cTimer Next) : this(cpu, scheduler, index)
            {
                this.Next = Next;
            }

            public void Trigger()
            {
                this.NextOverflow = this.cpu.GlobalCycleCount + this.Data.PrescalerLimit * (0x10000 - this.Data.Reload);
                if (!this.HasOverflowEvent)
                {
                    // overflow timing cannot be changed when the timer is still running
                    this.scheduler.Push(new Event(this.NextOverflow, this.Overflow));
                    this.HasOverflowEvent = true;
                }
            }

            public void Overflow(int GlobalTime, Scheduler.Scheduler scheduler)
            {
                this.HasOverflowEvent = false;
                if (!this.Control.Enabled)
                {
                    // timer was turned off
                    return; 
                }

                if (!this.Control.CountUpTiming && GlobalTime - this.NextOverflow < 0)
                {
                    // overflow was changed, should not have happened yet...
                    this.scheduler.Push(new Event(this.NextOverflow, this.Overflow));
                    this.HasOverflowEvent = true;
                    return;
                }

                if (this.Next?.Control.CountUpTiming ?? false && this.Next.Control.Enabled)
                {
                    // tick countup timers "normally" (not scheduled)
                    if (this.Next?.TickDirect(1) ?? false)
                        this.Next?.Overflow(GlobalTime, scheduler);
                }

                if (this.IsSound)
                {
                    this.FIFO[0]?.TimerOverflow();
                    this.FIFO[1]?.TimerOverflow();
                }

                if (this.Control.TimerIRQEnable)
                {
                    this.cpu.IO.IF.Request((Interrupt)((ushort)Interrupt.TimerOverflow << this.index));
                }

                if (!this.Control.CountUpTiming) this.Trigger();
            }

            public bool TickDirect(int cycles)
            {
                // countup tick calls, return if overflow happened
                return this.Data.TickUnscaled((ushort)cycles);
            }
        }

        public cTimer[] Timers;
    }
}
