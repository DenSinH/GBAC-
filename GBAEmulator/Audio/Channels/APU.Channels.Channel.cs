using GBAEmulator.CPU;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Audio.Channels
{
    public abstract class Channel
    {
        public int LengthCounter;
        public bool LengthFlag;
        protected bool Enabled;
        public int Frequency;
        public int Volume;

        public abstract short GetSample();

        protected abstract void OnTick();
        public Event Tick(int Time)
        {
            this.OnTick();
            return new Event(Time + ARM7TDMI.Frequency / this.Frequency, this.Tick);
        }

        public void TickLengthCounter()
        {
            if (this.LengthCounter > 0)
                this.LengthCounter--;
        }

        public void Trigger()
        {
            this.Enabled = true;
        }
    }
}
