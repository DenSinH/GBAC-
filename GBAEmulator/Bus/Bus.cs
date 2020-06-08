using System;
using System.Runtime.CompilerServices;

using GBAEmulator.CPU;

namespace GBAEmulator.Bus
{
    public class BUS
    {
        private ARM7TDMI cpu;
        public uint BusValue;
        
        public BUS(ARM7TDMI cpu)
        {
            this.cpu = cpu;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint OpenBus()
        {
            return this.BusValue;
        }
    }
}
