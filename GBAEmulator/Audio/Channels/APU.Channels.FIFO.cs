using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Audio.Channels
{
    public class FIFOChannel : IChannel
    {
        public short CurrentSample { get; private set; }
        public Queue<byte> Queue = new Queue<byte>(32);

    }
}
