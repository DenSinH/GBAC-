using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Scheduler
{
    public class Event : IComparable<Event>
    {
        public int Time;
        public delegate void Handler(Event sender, Scheduler scheduler);
        private readonly Handler _Handler;

        public Event(int Time, Handler _Handler)
        {
            this.Time = Time;
            this._Handler = _Handler;
        }

        public void Handle(Scheduler scheduler)
        {
            this._Handler(this, scheduler);
        }

        public int CompareTo(Event other)
        {
            return this.Time - other.Time;
        }

    }
}
