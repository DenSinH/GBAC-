using System;
using System.Runtime.CompilerServices;


namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public readonly DMAChannel[] DMAChannels = new DMAChannel[4];

        public bool DMAActive;
        public void ResetDMAActive()
        {
            for (int i = 0; i < 4; i++)
            {
                if (DMAChannels[i].Active)
                {
                    DMAActive = true;
                    return;
                }
            }
            DMAActive = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMA(DMAStartTiming timing)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!this.DMAChannels[i].Active)
                {
                    this.DMAChannels[i].Trigger(timing);
                }
            }
        }

        private void DoDMA(DMAChannel DMA)
        {
            this.Log($"DMA: {DMA.SAD:x8} -> {DMA.DAD:x8}");

            uint UnitLength = DMA.UnitLength;  // bytes: 32 / 16 bits

            if (UnitLength == 4)
            {
                // force alignment happens in memory handler
                this.mem.SetWordAt(DMA.DAD, this.mem.GetWordAt(DMA.SAD));
            }
            else  // 16 bit
            {
                // force alignment happens in memory handler
                this.mem.SetHalfWordAt(DMA.DAD, this.mem.GetHalfWordAt(DMA.SAD));
            }

            DMA.UpdateDMASAD();
            DMA.UpdateDMADAD();
            DMA.UnitCount--;

            if (DMA.Empty)
                InstructionCycles += DMA.End();
        }

        private void HandleDMAs()
        {
            for (int i = 0; i < 4; i++)
            {
                // DMA channel 0 has highest priority, 3 the lowest
                if (this.DMAChannels[i].Active)
                {
                    this.DoDMA(this.DMAChannels[i]);
                }
            }
        }
    }
}
