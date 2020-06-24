using System;
using System.Collections.Generic;

namespace GBAEmulator.Scheduler
{
    public class Scheduler
    {
        private LinkedList<Event> Storage;
        public int Count { get; private set; } = 0;

        public Scheduler()
        {
            Storage = new LinkedList<Event>();
        }

        public void Handle(long GlobalTime)
        {
            while (this.Storage.Count > 0 && GlobalTime > this.Storage.First.Value.Time)
            {
                this.Storage.First.Value.Handle(this);
                this.Storage.RemoveFirst();
            }
        }

        public void Push(Event e)
        {
            // account for Last being null (empty queue)
            if (!(e.Time < this.Storage.Last?.Value.Time))
            {
                this.Storage.AddLast(e);
                return;
            }

            // add events from the back, because we expect new events to occur late in the "queue"
            for (LinkedListNode<Event> node = this.Storage.Last; node != null; node = node.Previous)
            {
                // add after first node that is less than the new node's time
                if (node.Value.Time < e.Time)
                {
                    this.Storage.AddAfter(node, e);
                    return;
                }
            }

            // we might not find a node that is before this one, so we add it at the end
            this.Storage.AddFirst(e);
        }
        
        public void Remove(Event e)
        {
            // assumes e is actually in the storage
            for (LinkedListNode<Event> node = this.Storage.First; node != null; node = node.Next)
            {
                if (node.Value == e)
                {
                    this.Storage.Remove(node);
                    return;
                }
            }
        }

        public void EventChanged(Event e)
        {
            this.Remove(e);
            this.Push(e);
        }
    }
}
