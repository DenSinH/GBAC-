using System;
using System.Runtime.CompilerServices;

using GBAEmulator.Memory.IO;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void InitTimers()
        {
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
            private cTimer Next;

            private ARM7TDMI cpu;
            private int index;

            public cTMCNT_L Data;
            public cTMCNT_H Control;

            public cTimer(ARM7TDMI cpu, int index)
            {
                this.cpu = cpu;
                this.index = index;
                this.Data = new cTMCNT_L();
                this.Control = new cTMCNT_H(this.Data);
            }

            public cTimer(ARM7TDMI cpu, int index, cTimer Next) : this(cpu, index)
            {
                this.Next = Next;
            }

            public void TickDirect(int cycles)
            {
                // countup tick calls
                this.Data.TickDirect((ushort)cycles);
            }

            public void Tick(int cycles)
            {
                if (this.Control.Enabled && !this.Control.CountUpTiming)
                {
                    if (this.Data.Tick((ushort)cycles))  // overflow
                    {
                        if (this.Next?.Control.CountUpTiming ?? false) this.Next?.TickDirect(1);

                        if (this.Control.TimerIRQEnable) this.cpu.mem.IORAM.IF.Request((Interrupt)((ushort)Interrupt.TimerOverflow << this.index));
                    }
                }
            }
        }

        public cTimer[] Timers = new cTimer[4];
    }
}
