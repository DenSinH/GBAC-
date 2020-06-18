﻿using System;
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
        private const int FrameSequencerPeriod = 0x8000;
        private const int SamplePeriod = ARM7TDMI.Frequency / Speaker.SampleFrequency;
        private cEventQueue EventQueue = new cEventQueue(8);  // how many events at once (1 for each channel, framecounter, etc.)

        public readonly SquareChannel sq1 = new SquareChannel();
        public readonly SquareChannel sq2 = new SquareChannel();
        public readonly NoiseChannel noise = new NoiseChannel();
        public readonly WaveChannel wave = new WaveChannel();

        /* SOUNDCNT_L params */
        public uint MasterVolumeRight;  // 0 - 7
        public uint MasterVolumeLeft;   // 0 - 7
        public bool[] MasterEnableRight = new bool[4];
        public bool[] MasterEnableLeft = new bool[4];

        /* SOUNDCNT_X params */

        /* SOUNDCNT_H params */


        public readonly Channel[] Channels;

        public readonly Speaker speaker = new Speaker();
        private const double Amplitude = 0.05;

        public APU(IORAMSection IO)
        {
            this.IO = IO;
            this.Channels = new Channel[] { sq1, sq2, wave, noise };

            // todo: add initial EventQueue events
            // initial Frame sequencer event
            this.EventQueue.Push(new Event(FrameSequencerPeriod, this.TickFrameSequencer));
            foreach (Channel ch in this.Channels) this.EventQueue.Push(new Event(ch.Period, ch.Tick));
            this.EventQueue.Push(new Event(ARM7TDMI.Frequency / Speaker.SampleFrequency, this.ProvideSample));
        }

        public void Tick(int cycles)
        {
            this.Timer += cycles;
            // assume only 1 audio event per CPU instruction
            // even if this is not true, the delay will be minimal
            if (this.EventQueue.Count > 0 && this.Timer - this.EventQueue.Peek().Time > 0)
            {
                this.EventQueue.Push(this.EventQueue.Pop().Handle());
            }
        }

        private Event TickFrameSequencer(int time)
        {
            this.FrameSequencer++;
            switch (this.FrameSequencer & 7)
            {
                case 0:
                case 4:
                    foreach (Channel ch in this.Channels) ch.TickLengthCounter();
                    break;

                case 2:
                case 6:
                    foreach (Channel ch in this.Channels) ch.TickLengthCounter();
                    this.sq1.DoSweep();
                    break;

                case 7:
                    this.sq1.DoEnvelope();
                    this.sq2.DoEnvelope();
                    this.noise.DoEnvelope();
                    break;
            }

            return new Event(time + FrameSequencerPeriod, this.TickFrameSequencer);
        }

        private Event ProvideSample(int time)
        {
            while (!this.speaker.NeedMoreSamples) { }  // prevent buffer overflow
            int SampleLeft = 0, SampleRight = 0;
            
            for (int i = 0; i < 4; i++)
            {
                if (this.MasterEnableRight[i])
                {
                    SampleRight += this.Channels[i].CurrentSample;
                }
                if (this.MasterEnableLeft[i])
                {
                    SampleLeft += this.Channels[i].CurrentSample;
                }
            }

            SampleRight = (int)(SampleRight * Amplitude);
            SampleLeft = (int)(SampleLeft * Amplitude);

            SampleRight = (int)((SampleRight * this.MasterVolumeRight) / 8);
            SampleLeft = (int)((SampleLeft * this.MasterVolumeLeft) / 8);

            this.speaker.AddSample((short)SampleLeft, (short)SampleRight);

            return new Event(time + SamplePeriod, this.ProvideSample);
        }
    }
}
