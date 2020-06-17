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
        public int Period;
        public int Volume;

        public abstract short GetSample();

        protected abstract void OnTick();
        public Event Tick(int Time)
        {
            this.OnTick();
            return new Event(Time + this.Period, this.Tick);
        }

        public void TickLengthCounter()
        {
            if (this.LengthCounter > 0)
                this.LengthCounter--;
        }

        public virtual void Trigger()
        {
            this.Enabled = true;
            if (this.LengthCounter == 0) this.LengthCounter = 64;
        }
    }
}
