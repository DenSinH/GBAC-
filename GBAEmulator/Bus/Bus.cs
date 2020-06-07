using System;

using GBAEmulator.CPU;

namespace GBAEmulator.Bus
{
    public class BUS
    {
        private ARM7TDMI cpu;
        public uint DMAValue;
        
        public BUS(ARM7TDMI cpu)
        {
            this.cpu = cpu;
        }

        public uint OpenBus()
        {
            if (!this.cpu.DMAActive)
                return this.DMAValue;
            return this.cpu.Pipeline.OpenBus();
        }
    }
}
