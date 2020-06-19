using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Scheduler
{
    public class Scheduler
    {
        private Event[] Storage;
        public int Count { get; private set; } = 0;

        public Scheduler(int Size)
        {
            Storage = new Event[Size];
        }

        public void Handle(int GlobalTime)
        {
            while (this.Count > 0 && GlobalTime - this.Peek().Time > 0)
            {
                this.Pop().Handle(this);
            }
        }

        public void Push(Event e)
        {
            Storage[++Count] = e;
            this.TrickleUp(Count);
        }

        private Event Pop()
        {
            Event root = Storage[1];
            Swap(1, Count--);
            this.TrickleDown(1);
            return root;
        }

        private Event Peek()
        {
            return Storage[1];
        }

        private void Swap(int index1, int index2)
        {
            Event temp = Storage[index1];
            Storage[index1] = Storage[index2];
            Storage[index2] = temp;
        }

        private void TrickleUp(int index)
        {
            // reached top of heap
            if (index / 2 == 0) return;

            // compare to parent
            if (Storage[index].CompareTo(Storage[index / 2]) < 0)
            {
                this.Swap(index, index / 2);

                // trickle up further
                this.TrickleUp(index / 2);
            }
        }

        private void TrickleDown(int index)
        {
            if (2 * index > Count)
            {
                // reached bottom of heap
                return;
            }
            else if (2 * index + 1 > Count)
            {
                // we only get here if there is only 1 child. From the way the tree is structured, this must be the left child
                // if this is the case, we also know that we have reached the bottom of the stack
                if (Storage[2 * index].CompareTo(Storage[index]) < 0)
                {
                    this.Swap(index, 2 * index);
                }
            }
            else
            {
                // 2 children, find smallest child
                int MinChild = 2 * index;
                if (Storage[MinChild + 1].CompareTo(Storage[MinChild]) < 0) MinChild++;

                // swap
                if (Storage[MinChild].CompareTo(Storage[index]) < 0)
                {
                    this.Swap(index, MinChild);
                    TrickleDown(MinChild);
                }
                return;
            }
        }
    }
}
