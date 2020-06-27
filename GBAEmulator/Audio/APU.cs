using System;

using GBAEmulator.Scheduler;
using GBAEmulator.CPU;
using GBAEmulator.Audio.Peripherals;
using GBAEmulator.Audio.Channels;
using System.Threading;

namespace GBAEmulator.Audio
{
    public class APU
    {
        public bool ExternalEnable = true;

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

        public bool[] ExternalChannelEnable = new bool[4] { true, true, true, true };
        public bool[] ExternalFIFOEnable = new bool[2] { true, true };

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

        public readonly Speaker speaker = new Speaker();
        private const double Amplitude = 0.03;

        public APU(ARM7TDMI cpu, Scheduler.Scheduler scheduler)
        {
            this.Channels = new Channel[] { sq1, sq2, wave, noise };

            this.FIFO[0] = this.FIFOA = new FIFOChannel(cpu, 0x0400_00a0);
            this.FIFO[1] = this.FIFOB = new FIFOChannel(cpu, 0x0400_00a4);

            // initial APU events
            scheduler.Push(new Event(FrameSequencerPeriod, this.TickFrameSequencer));
            foreach (Channel ch in this.Channels) scheduler.Push(new Event(ch.Period, ch.Tick));
            scheduler.Push(new Event(SamplePeriod, this.ProvideSample));
        }

        private void TickFrameSequencer(Event sender, Scheduler.Scheduler scheduler)
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

            sender.Time += FrameSequencerPeriod;
            scheduler.Push(sender);
        }

        private void ProvideSample(Event sender, Scheduler.Scheduler scheduler)
        {
            int SampleLeft = 0, SampleRight = 0;
            
            for (int i = 0; i < 4; i++)
            {
                if (!ExternalChannelEnable[i])
                    continue;

                if (this.MasterEnableRight[i])
                {
                    SampleRight += this.Channels[i].CurrentSample;
                }
                if (this.MasterEnableLeft[i])
                {
                    SampleLeft += this.Channels[i].CurrentSample;
                }
            }

            // SOUNDCNT_L volume control does not affect FIFO channels
            SampleRight = (int)((SampleRight * this.MasterVolumeRight) / 8);
            SampleLeft = (int)((SampleLeft * this.MasterVolumeLeft) / 8);

            switch (this.Sound1_4Volume)
            {
                case 0:
                    // 25%
                    SampleLeft >>= 2;
                    SampleRight >>= 2;
                    break;
                case 1:
                    // 50%
                    SampleLeft >>= 1;
                    SampleRight >>= 1;
                    break;
                default:
                    // 100% / prohibited
                    break;
            }

            /*
            GBATek:
             Each of the two FIFOs can span the FULL output range (+/-200h).
             Each of the four PSGs can span one QUARTER of the output range (+/-80h).
            So we multiply the output of the FIFO by 4
            */

            for (int i = 0; i < 2; i++)
            {
                if (!ExternalFIFOEnable[i])
                    continue;

                if (this.DMAEnableRight[i])
                {
                    SampleRight += this.FIFO[i].CurrentSample << (this.DMASoundVolume[i] ? 1 : 2);  // false = 50%, true = 100%
                }

                if (this.DMAEnableLeft[i])
                {
                    SampleLeft += this.FIFO[i].CurrentSample << (this.DMASoundVolume[i] ? 1 : 2);  // false = 50%, true = 100%
                }
            }

            SampleRight = (int)(SampleRight * Amplitude);
            SampleLeft = (int)(SampleLeft * Amplitude);

            if (this.ExternalEnable)
            {
                SpinWait.SpinUntil(() => this.speaker.NeedMoreSamples);  // prevent buffer overflow
                this.speaker.AddSample((short)SampleLeft, (short)SampleRight);
            }

            sender.Time += SamplePeriod;
            scheduler.Push(sender);
        }
    }
}
