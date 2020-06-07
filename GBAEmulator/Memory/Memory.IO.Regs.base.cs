using System;

using GBAEmulator.CPU;

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
        
        private class DefaultRegister : IORegister2 { }  // basically default register (name might be a bit misleading)

        private class ZeroRegister : IORegister
        {
            public ushort Get()
            {
                return 0;
            }

            public void Set(ushort value, bool setlow, bool sethigh)
            {
                
            }
        }

        public class UnusedRegisterHalf : IORegister2
        {
            private ARM7TDMI cpu;
            private bool upper;
            public UnusedRegisterHalf(ARM7TDMI cpu, bool upper)
            {
                this.cpu = cpu;
                this.upper = upper;
            }

            public override ushort Get()
            {
                if (upper)
                {
                    return (ushort)(this.cpu.Pipeline.OpenBus() >> 16);
                }
                return (ushort)this.cpu.Pipeline.OpenBus();
            }

            public override void Set(ushort value, bool setlow, bool sethigh)
            {
                
            }
        }

        private class UnusedRegister : IORegister4<UnusedRegisterHalf>
        {
            public UnusedRegister(ARM7TDMI cpu)
            {
                this.lower = new UnusedRegisterHalf(cpu, false);
                this.upper = new UnusedRegisterHalf(cpu, true);
            }
        }
    }
}
