using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Audio.Channels
{
    public interface IChannel
    {
        public short CurrentSample { get; }
    }
}
