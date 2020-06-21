using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.GPIO
{
    public interface IGPIOChip
    {
        public void Write(byte value);

        public byte Read();
    }
}
