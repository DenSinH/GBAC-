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
            private bool HasOverflowEvent;
            public readonly int index;

            public readonly cTMCNT_L Data;
            public readonly cTMCNT_H Control;
            public readonly FIFOChannel[] FIFO = new FIFOChannel[2];
            private readonly bool IsSound;

            private readonly Event OverflowEvent;

            public cTimer(ARM7TDMI cpu, Scheduler.Scheduler scheduler, int index)
            {
                this.cpu = cpu;
                this.scheduler = scheduler;
                this.OverflowEvent = new Event(0, this.Overflow);
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
                this.Data.Restart(this.Control.CountUpTiming);
                if (!this.HasOverflowEvent)
                {
                    this.HasOverflowEvent = true;
                    this.OverflowEvent.Time = this.cpu.GlobalCycleCount + (this.Data.NextPrescalerLimit * (0x10000 - this.Data.Reload));
                    this.scheduler.Push(this.OverflowEvent);
                }
                else
                {
                    // this can only be the case if our timer was already triggered, so we can just increment the OverflowEvent.Time instead of
                    // recalculating it is not necessary (and wrong even!) so we
                    this.OverflowEvent.Time += this.Data.NextPrescalerLimit * (0x10000 - this.Data.Reload);
                    this.scheduler.EventChanged(this.OverflowEvent);
                }
            }

            public void Disable()
            {
                if (this.HasOverflowEvent)
                {
                    this.HasOverflowEvent = false;
                    this.scheduler.Remove(this.OverflowEvent);
                    this.Data.Stop();
                }
            }

            public void Overflow(Event sender, Scheduler.Scheduler scheduler)
            {
                this.HasOverflowEvent = false;
                this.Data.Restart(this.Control.CountUpTiming);

                // with event handling, we have already covered timers being turned off / countup
                if (this.Next?.Control.CountUpTiming ?? false && this.Next.Control.Enabled)
                {
                    // tick countup timers "normally" (not scheduled)
                    if (this.Next?.TickDirect(1) ?? false)
                        this.Next?.Overflow(null, scheduler);
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
