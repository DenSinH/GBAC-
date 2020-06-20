using System;
using System.Threading;
using NAudio.Wave;

namespace GBAEmulator.Audio.Peripherals
{
    public class Speaker
    {
        /*
         Copied from NESC- (my NES emulator)
         */
        class cShutDownEvent
        {
            public bool ShutDown;
        }

        public const int SampleFrequency = 32768;

        // 32.768 kHz 16 bit stereo
        private readonly BufferedWaveProvider Buffer = new BufferedWaveProvider(new WaveFormat(SampleFrequency, 16, 2));
        private readonly byte[] TempBuffer;
        private int TempBufferedSamples = 0;
        private readonly Thread Playback;
        private bool PlaybackStarted;
        private readonly cShutDownEvent ShutDownEvent;

        public Speaker()
        {
            this.TempBuffer = new byte[0x100]; // must be multiple of 4

            this.ShutDownEvent = new cShutDownEvent();
            this.ShutDownEvent.ShutDown = false;

            this.Playback = new Thread(() => Play(this.Buffer, this.ShutDownEvent));
        }

        public void AddSample(short SampleLeft, short SampleRight)
        {
            // add single sample for both the left and right speaker

            TempBuffer[TempBufferedSamples]     = (byte) SampleLeft;
            TempBuffer[TempBufferedSamples + 1] = (byte)(SampleLeft >> 8);
            TempBuffer[TempBufferedSamples + 2] = (byte) SampleRight;
            TempBuffer[TempBufferedSamples + 3] = (byte)(SampleRight >> 8);

            TempBufferedSamples += 4;

            if (TempBufferedSamples == TempBuffer.Length)
            {
                lock (this.Buffer)
                {
                    this.Buffer.AddSamples(TempBuffer, 0, TempBuffer.Length);
                }
                TempBufferedSamples = 0;

                if ((!this.PlaybackStarted) && (!this.NeedMoreSamples))
                {
                    this.Playback.Start();
                    this.PlaybackStarted = true;
                }
            }
        }

        public void ClearBuffer()
        {
            this.Buffer.ClearBuffer();
        }

        public void ShutDown()
        {
            lock (this.ShutDownEvent)
            {
                this.ShutDownEvent.ShutDown = true;
            }
        }

        public bool NeedMoreSamples
        {
            // Longer buffer: more delay, less artifacts
            get 
            {
                return this.Buffer.BufferedBytes <= 0x8 * TempBuffer.Length || TempBufferedSamples != 0;
            }
        }

        private static void Play(BufferedWaveProvider bf, cShutDownEvent sd)
        {
            WaveOut wo = new WaveOut();
            wo.DesiredLatency = 20;
            wo.NumberOfBuffers = 25;
            wo.Init(bf);
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing && !sd.ShutDown)
            {
                Thread.Sleep(50);
            }
            wo.Dispose();
        }
    }
}
