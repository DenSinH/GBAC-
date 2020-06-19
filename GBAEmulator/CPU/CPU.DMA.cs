using System;
using System.Runtime.CompilerServices;

using GBAEmulator.IO;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private class DMAChannel
        {
            public readonly cDMACNT_H DMACNT_H;
            public readonly cDMACNT_L DMACNT_L;
            public readonly cDMAAddress DMASAD;
            public readonly cDMAAddress DMADAD;
            private readonly cIF IF;
            private readonly int index;

            public bool Active { get; private set; }

            public DMAChannel(cDMACNT_H DMACNT_H, cDMACNT_L DMACNT_L, cDMAAddress DMASAD, cDMAAddress DMADAD, cIF IF, int index)
            {
                this.DMACNT_H = DMACNT_H;
                this.DMACNT_L = DMACNT_L;
                this.DMASAD = DMASAD;
                this.DMADAD = DMADAD;
                this.IF = IF;
                this.index = index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update()
            {
                if (this.DMACNT_H.ValueChanged)
                {
                    this.DMACNT_H.ValueChanged = false;

                    this.DMADAD.Reload();
                    this.DMASAD.Reload();
                    this.DMACNT_L.Reload();

                    if (this.DMACNT_H.StartTiming == DMAStartTiming.Immediately)
                    {
                        // DMA Enable set AND DMA start timing immediate
                        this.Active = true;
                    }
                }
            }

            public bool Trigger(DMAStartTiming timing)
            {
                if (this.DMACNT_H.DMAEnabled && timing == this.DMACNT_H.StartTiming)  // enabled
                {
                    this.Active = true;
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void UpdateDMAAddress(cDMAAddress dmaxad, AddrControl control)
            {
                uint amount = (uint)(this.DMACNT_H.DMATransferType ? 4 : 2);
                switch (control)
                {
                    case AddrControl.IncrementReload:   // 11 (prohibited for DMASAD)
                    case AddrControl.Increment:         // 00
                        dmaxad.Address += amount;
                        break;
                    case AddrControl.Decrement:         // 01
                        dmaxad.Address -= amount;
                        break;
                    case AddrControl.Fixed:             // 10
                        break;
                }
            }

            public void UpdateDMASAD()
            {
                this.UpdateDMAAddress(this.DMASAD, this.DMACNT_H.SourceAddrControl);
            }

            public void UpdateDMADAD()
            {
                this.UpdateDMAAddress(this.DMADAD, this.DMACNT_H.DestAddrControl);
            }

            public void End()
            {
                // should actually happen at the start, but it does not matter, timers don't IRQ anyway during a DMA
                //InstructionCycles += 2 * ICycle;
                //if (dmadad.Address > 0x0800_0000 && dmasad.Address > 0x0800_0000)
                //    InstructionCycles += 2 * ICycle;

                // Immediate DMA transfers should ignore the Repeat bit - Fleroviux
                if (this.DMACNT_H.DMARepeat && this.DMACNT_H.StartTiming != DMAStartTiming.Immediately)
                {
                    this.DMACNT_L.Reload();
                    if (this.DMACNT_H.DestAddrControl == AddrControl.IncrementReload)
                    {
                        this.DMADAD.Reload();
                    }
                }
                else
                {
                    // end of the transfer
                    if (this.DMACNT_H.IRQOnEnd)
                    {
                        this.IF.Request((ushort)((ushort)Interrupt.DMA << this.index));
                    }
                    this.DMACNT_H.Disable();  // clear enabled bit
                }
                this.Active = false;
            }
        }

        private readonly DMAChannel[] DMAChannels = new DMAChannel[4];

        public bool DMAActive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.DMAChannels[i].Active) return true;
                }
                return false;
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMASpecial(int i)
        {
            // todo: Sound FIFO
            this.DMAChannels[i].Trigger(DMAStartTiming.Special);
        }

        private void DoDMA(DMAChannel DMA)
        {
            this.Log($"DMA: {DMA.DMASAD.Address.ToString("x8")} -> {DMA.DMADAD.Address.ToString("x8")}");

            uint UnitLength = (uint)(DMA.DMACNT_H.DMATransferType ? 4 : 2);  // bytes: 32 / 16 bits

            if (UnitLength == 4)
            {
                // force alignment happens in memory handler
                this.mem.SetWordAt(DMA.DMADAD.Address, this.mem.GetWordAt(DMA.DMASAD.Address));
            }
            else  // 16 bit
            {
                // force alignment happens in memory handler
                this.mem.SetHalfWordAt(DMA.DMADAD.Address, this.mem.GetHalfWordAt(DMA.DMASAD.Address));
            }

            DMA.UpdateDMASAD();
            DMA.UpdateDMADAD();
            DMA.DMACNT_L.UnitCount--;

            if (DMA.DMACNT_L.Empty)
                DMA.End();
        }

        private void HandleDMAs()
        {
            for (int i = 0; i < 4; i++)
            {
                this.DMAChannels[i].Update();

                // DMA channel 0 has highest priority, 3 the lowest
                if (this.DMAChannels[i].Active)
                {
                    this.DoDMA(this.DMAChannels[i]);
                }
            }
        }
    }
}
