﻿using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    public partial class CPU
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        State state;
        readonly Queue<uint> Pipeline = new Queue<uint>(3);
        public CPU()
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

        private void PipelineFlush()
        {
            this.Pipeline.Clear();
            this.Pipeline.Enqueue(this.GetAt<uint>(this.PC));
            this.PC++;
            this.Pipeline.Enqueue(this.GetAt<uint>(this.PC));
            this.PC++;
        }
    }
}
