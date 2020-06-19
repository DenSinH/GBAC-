using GBAEmulator.Audio.Channels;
using GBAEmulator.Bus;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.IO
{
    public class FIFO_Data : WriteOnlyRegister2
    {
        private readonly FIFOChannel FIFO;

        public FIFO_Data(FIFOChannel FIFO, BUS bus, bool IsLower) : base(bus, IsLower)
        {
            this.FIFO = FIFO;
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);
            /* !! NOTE !!  This uses the fact that the lower register is always written to first in a 32 bit data transfer */
            this.FIFO.Queue.Enqueue((byte)this._raw);
            this.FIFO.Queue.Enqueue((byte)(this._raw >> 8));
        }
    }
}
