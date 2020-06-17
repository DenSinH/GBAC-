using System;
using System.Collections.Generic;
using System.Text;

using GBAEmulator.IO;
using GBAEmulator.CPU;
using GBAEmulator.Audio.Peripherals;
using GBAEmulator.Audio.Channels;

namespace GBAEmulator.Audio
{
    public class APU
    {
        private const int CPUCyclesPerApuSample = ARM7TDMI.Frequency / Speaker.SampleFrequency;
        private readonly IORAMSection IO;
        private int Timer;
        private int FrameSequencer;
        private const int FrameSequencerFrequency = 512;
        private cEventQueue EventQueue = new cEventQueue(8);  // how many events at once (1 for each channel, framecounter, etc.)

        public readonly SquareChannel sq1 = new SquareChannel();
        public readonly SquareChannel sq2 = new SquareChannel();

        public readonly Speaker speaker = new Speaker();
        private const double Amplitude = 0.02;

        public APU(IORAMSection IO)
        {
            this.IO = IO;

            // todo: add initial EventQueue events
            // initial Frame sequencer event
            this.EventQueue.Push(new Event(ARM7TDMI.Frequency / FrameSequencerFrequency, this.TickFrameSequencer));
            this.EventQueue.Push(new Event(ARM7TDMI.Frequency / this.sq1.Frequency, this.sq1.Tick));
            this.EventQueue.Push(new Event(ARM7TDMI.Frequency / this.sq2.Frequency, this.sq2.Tick));
            this.EventQueue.Push(new Event(ARM7TDMI.Frequency / Speaker.SampleFrequency, this.ProvideSample));
        }

        public void Tick(int cycles)
        {
            this.Timer += cycles;
            while (this.EventQueue.Count > 0 && this.Timer - this.EventQueue.Peek().Time > 0)
            {
                this.EventQueue.Push(this.EventQueue.Pop().Handle());
            }
        }

        private Event TickFrameSequencer(int time)
        {
            this.FrameSequencer++;
            // todo: add frame sequencer events

            return new Event(time + ARM7TDMI.Frequency / FrameSequencerFrequency, this.TickFrameSequencer);
        }

        private Event ProvideSample(int time)
        {
            while (!this.speaker.NeedMoreSamples) { }  // prevent buffer overflow

            short SampleValue = 0;
            SampleValue += (short)(Amplitude * this.sq1.GetSample());
            SampleValue += (short)(Amplitude * this.sq2.GetSample());
            this.speaker.AddSample(SampleValue, SampleValue);

            return new Event(time + ARM7TDMI.Frequency / Speaker.SampleFrequency, this.ProvideSample);
        }
    }
}
