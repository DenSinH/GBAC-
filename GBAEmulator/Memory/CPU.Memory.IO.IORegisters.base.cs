using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator.Memory
{
    partial class MEM
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

        public abstract class IORegister4<T> where T : IORegister2
        {
            public T lower;
            public T upper;

            protected IORegister4() { }

            protected IORegister4(T lower, T upper)
            {
                this.lower = lower;
                this.upper = upper;
            }

        }
    }
}
