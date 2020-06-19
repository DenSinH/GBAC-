using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Scheduler
{
    public struct Event : IComparable<Event>
    {
        public int Time { get; private set; }
        public delegate Event Handler(int time);
        private Handler _Handler;

        public Event(int Time, Handler _Handler)
        {
            this.Time = Time;
            this._Handler = _Handler;
        }

        public Event Handle()
        {
            return this._Handler(this.Time);
        }

        public int CompareTo(Event other)
        {
            return this.Time - other.Time;
        }

    }
}
