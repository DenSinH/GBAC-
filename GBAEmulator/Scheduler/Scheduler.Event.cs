using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Scheduler
{
    public struct Event : IComparable<Event>
    {
        public int Time { get; private set; }
        public delegate void Handler(int time, Scheduler scheduler);
        private Handler _Handler;

        public Event(int Time, Handler _Handler)
        {
            this.Time = Time;
            this._Handler = _Handler;
        }

        public void Handle(Scheduler scheduler)
        {
            this._Handler(this.Time, scheduler);
        }

        public int CompareTo(Event other)
        {
            return this.Time - other.Time;
        }

    }
}
