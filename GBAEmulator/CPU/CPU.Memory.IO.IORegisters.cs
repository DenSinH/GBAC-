using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private interface IORegister
        {
            int Length { get; }

            uint Get();
            void Set(uint value, uint BitMask);
        }

        private abstract class IORegister2 : IORegister
        {
            int IORegister.Length => 2;

            public abstract uint Get();
            public abstract void Set(uint value, uint BitMask);
        }

        private abstract class IORegister4: IORegister
        {
            int IORegister.Length => 4;

            public abstract uint Get();
            public abstract void Set(uint value, uint BitMask);
        }

        private class NORegister : IORegister2
        {
            ushort value;

            public override uint Get()
            {
                return value;
            }

            public override void Set(uint value, uint BitMask)
            {
                this.value = (ushort)((this.value & (~BitMask)) | (value & BitMask));
            }
        }
    }
}
