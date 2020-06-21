using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.GPIO
{
    public class GPIOEmpty : IGPIOChip
    {
        public byte Read()
        {
            return 0;
        }

        public void Write(byte value)
        {
            
        }
    }
}
