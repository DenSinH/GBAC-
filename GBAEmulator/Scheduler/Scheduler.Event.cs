using System;

namespace GBAEmulator.Scheduler
{
    public class Event : IComparable<Event>
    {
        public long Time;
        public delegate void Handler(Event sender, Scheduler scheduler);
        private readonly Handler _Handler;

        public Event(long Time, Handler _Handler)
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
            return (int)(this.Time - other.Time);
        }

    }
}
