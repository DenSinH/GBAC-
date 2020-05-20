using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public interface IORegister
        {
            ushort Get();
            void Set(ushort value, bool setlow, bool sethigh);
        }

        public abstract class IORegister2 : IORegister
        {
            protected ushort raw;

            public virtual ushort Get()
            {
                return this.raw;
            }

            public virtual void Set(ushort value, bool setlow, bool sethigh)
            {
                if (setlow)
                    this.raw = (ushort)((this.raw & 0xff00) | (value & 0x00ff));
                else if (sethigh)
                    this.raw = (ushort)((this.raw & 0x00ff) | (value & 0xff00));
            }
        }

        public abstract class IORegister4
        {
            public readonly IORegister2 lower;
            public readonly IORegister2 upper;

            public IORegister4()
            {
                this.lower = new EmptyRegister();
                this.upper = new EmptyRegister();
            }
        }
    }
}
