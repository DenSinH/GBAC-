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
            protected ushort _raw;

            public virtual ushort Get()
            {
                return this._raw;
            }

            public virtual void Set(ushort value, bool setlow, bool sethigh)
            {
                if (setlow)
                    this._raw = (ushort)((this._raw & 0xff00) | (value & 0x00ff));
                if (sethigh)
                    this._raw = (ushort)((this._raw & 0x00ff) | (value & 0xff00));
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

            protected IORegister4(IORegister2 lower, IORegister2 upper)
            {
                this.lower = lower;
                this.upper = upper;
            }

        }
    }
}
