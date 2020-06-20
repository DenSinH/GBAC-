using System;
using System.Collections.Generic;
using System.Text;

using GBAEmulator.Scheduler;
using GBAEmulator.IO;
using GBAEmulator.CPU;
using GBAEmulator.Audio.Peripherals;
using GBAEmulator.Audio.Channels;

namespace GBAEmulator.Audio
{
    public class APU
    {
        private int FrameSequencer;
        private const int FrameSequencerPeriod = 0x8000;
        private const int SamplePeriod = ARM7TDMI.Frequency / Speaker.SampleFrequency;

        public readonly SquareChannel sq1 = new SquareChannel();
        public readonly SquareChannel sq2 = new SquareChannel();
        public readonly NoiseChannel noise = new NoiseChannel();
        public readonly WaveChannel wave = new WaveChannel();

        public readonly Channel[] Channels;

        public readonly FIFOChannel FIFOA;
        public readonly FIFOChannel FIFOB;

        public readonly FIFOChannel[] FIFO = new FIFOChannel[2];

        /* SOUNDCNT_L params */
        public uint MasterVolumeRight;  // 0 - 7
        public uint MasterVolumeLeft;   // 0 - 7
        public bool[] MasterEnableRight = new bool[4];
        public bool[] MasterEnableLeft = new bool[4];

        /* SOUNDCNT_H params */
        public int Sound1_4Volume;
        public bool[] DMASoundVolume = new bool[2];
        public bool[] DMAEnableLeft = new bool[2];
        public bool[] DMAEnableRight = new bool[2];

        /* SOUNDCNT_X params */

        public readonly Speaker speaker = new Speaker();
        private const double Amplitude = 0.05;

        public APU(ARM7TDMI cpu, Scheduler.Scheduler scheduler)
        {
            this.Channels = new Channel[] { sq1, sq2, wave, noise };

            this.FIFO[0] = this.FIFOA = new FIFOChannel(cpu, 0x0400_00a0);
            this.FIFO[1] = this.FIFOB = new FIFOChannel(cpu, 0x0400_00a4);

            // initial APU events
            scheduler.Push(new Event(FrameSequencerPeriod, this.TickFrameSequencer));
            foreach (Channel ch in this.Channels) scheduler.Push(new Event(ch.Period, ch.Tick));
            scheduler.Push(new Event(FrameSequencerPeriod, this.ProvideSample));
        }

        private void TickFrameSequencer(int time, Scheduler.Scheduler scheduler)
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

            scheduler.Push(new Event(time + FrameSequencerPeriod, this.TickFrameSequencer));
        }

        private void ProvideSample(int time, Scheduler.Scheduler scheduler)
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

            for (int i = 0; i < 2; i++)
            {
                if (this.DMAEnableRight[i])
                {
                    SampleRight += this.FIFO[i].CurrentSample << (this.DMASoundVolume[i] ? 0 : 1);  // false = 50%, true = 100%
                }

                if (this.DMAEnableLeft[i])
                {
                    SampleLeft += this.FIFO[i].CurrentSample << (this.DMASoundVolume[i] ? 0 : 1);  // false = 50%, true = 100%
                }
            }

            SampleRight = (int)(SampleRight * Amplitude);
            SampleLeft = (int)(SampleLeft * Amplitude);

            SampleRight = (int)((SampleRight * this.MasterVolumeRight) / 8);
            SampleLeft = (int)((SampleLeft * this.MasterVolumeLeft) / 8);

            this.speaker.AddSample((short)SampleLeft, (short)SampleRight);

            scheduler.Push(new Event(time + SamplePeriod, this.ProvideSample));
        }
    }
}
