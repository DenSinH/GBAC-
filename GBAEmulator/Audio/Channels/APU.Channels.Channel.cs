﻿using GBAEmulator.CPU;
using System;
using System.Collections.Generic;
using System.Text;

using GBAEmulator.Scheduler;

namespace GBAEmulator.Audio.Channels
{
    public abstract class Channel : IChannel
    {
        public int LengthCounter;
        public bool LengthFlag;
        protected bool Enabled;
        public int Period;
        public int Volume;
        public short CurrentSample { get; protected set; }

        public void Reset()
        {
            this.LengthCounter = 0;
            this.LengthFlag = false;
            this.Enabled = false;
            this.Period = 128 * 2048;  // square channel default value (does not matter much)
            this.Volume = 0;
        }

        protected abstract short GetSample();

        protected abstract void OnTick();
        public void Tick(Event sender, Scheduler.Scheduler scheduler)
        {
            this.OnTick();
            if (!this.SoundOn())
                this.CurrentSample = 0;
            else
                this.CurrentSample = this.GetSample();

            sender.Time += this.Period;
            scheduler.Push(sender);
        }

        public virtual bool SoundOn()
        {
            if (!this.Enabled)
                return false;

            if (this.LengthFlag && (this.LengthCounter == 0))
                return false;

            if (this.Volume == 0)
                return false;

            return true;
        }

        public void TickLengthCounter()
        {
            if (this.LengthCounter > 0)
                this.LengthCounter--;
        }

        public virtual void Trigger()
        {
            this.Enabled = true;
            if (this.LengthCounter == 0) this.LengthCounter = 64;
        }

        public int EnvelopeTime;
        public bool EnvelopeDir;
        public void DoEnvelope()
        {
            if (EnvelopeTime > 0)
            {
                EnvelopeTime--;
                if (EnvelopeDir) Volume++;
                else Volume--;

                Volume = Math.Clamp(Volume, 0, 16);
            }
        }
    }
}
