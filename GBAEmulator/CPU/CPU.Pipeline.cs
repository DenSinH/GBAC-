﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBAEmulator.CPU
{
    // basically just a queue, but without an Exception on Peeks for empty queues
    /*
     The difference with a queue is that it can store at most 4 elements. We only have 2 elements in it at most, so this works.
     When comparing the framerate with a built-in Queue<uint>, this implementation is a bit faster. Probably because we have none 
     of the riff-raff that builtin Queue<>s have.
         */
    public class cPipeline
    {
        private uint[] storage = new uint[4];
        private uint offset = 0;
        public int Count { get; private set; }

        private ARM7TDMI cpu;

        public cPipeline(ARM7TDMI cpu)
        {
            this.cpu = cpu;
        }

        public uint Peek()
        {
            return storage[offset & 3];
        }

        public uint Dequeue()
        {
            Count--;
            return storage[offset++ & 3];
        }

        public void Enqueue(uint value)
        {
            storage[(offset + Count++) & 3] = value;
        }

        public void Clear()
        {
            Count = 0;
            offset = 0;
        }

        public uint PreFetch
        {
            get
            {
                // recently fetched opcode
                if (this.cpu.state == ARM7TDMI.State.ARM)
                    return storage[(offset + Count - 1) & 3];

                uint prefetch = storage[(offset + Count - 1) & 3];
                return (uint)(prefetch << 16) | prefetch;
            }
        }

        public string Dumps()
        {
            string result = "";
            for (int i = 0; i < 4; i++)
            {
                result += storage[(offset + i) & 3].ToString("x8") + " ";
            }
            return result;
        }
    }

}
