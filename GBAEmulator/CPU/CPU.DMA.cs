using System;
using System.Runtime.CompilerServices;

using GBAEmulator.IO;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public class DMAChannel
        {
            private readonly cDMACNT_H DMACNT_H;
            private readonly cDMACNT_L DMACNT_L;
            private ushort SNDCOUNT = 4;
            private readonly cDMAAddress DMASAD;
            private readonly cDMAAddress DMADAD;
            private readonly cIF IF;
            private readonly int index;
            private readonly bool Sound;

            public bool Active { get; private set; }

            public DMAChannel(cDMACNT_H DMACNT_H, cDMACNT_L DMACNT_L, cDMAAddress DMASAD, cDMAAddress DMADAD, cIF IF, int index)
            {
                this.DMACNT_H = DMACNT_H;
                this.DMACNT_L = DMACNT_L;
                this.DMASAD = DMASAD;
                this.DMADAD = DMADAD;
                this.IF = IF;
                this.index = index;
                this.Sound = index == 1 || index == 2;
            }

            public uint DAD => this.DMADAD.Address;

            public uint SAD => this.DMASAD.Address;

            public ushort UnitCount
            {
                get
                {
                    if (this.Sound && this.DMACNT_H.StartTiming == DMAStartTiming.Special)
                        return this.SNDCOUNT;
                    return this.DMACNT_L.UnitCount;
                }
                set
                {
                    if (this.Sound && this.DMACNT_H.StartTiming == DMAStartTiming.Special)
                        this.SNDCOUNT = value;
                    else 
                        this.DMACNT_L.UnitCount = value;
                }
            }

            public uint UnitLength
            {
                get
                {
                    if (this.Sound && this.DMACNT_H.StartTiming == DMAStartTiming.Special)
                        return 4;  // always 32 bit
                    return (uint)(this.DMACNT_H.DMATransferType ? 4 : 2);  // 32 bit/16 bit
                }
            }

            public bool Empty
            {
                get
                {
                    if (this.Sound && this.DMACNT_H.StartTiming == DMAStartTiming.Special)
                        return this.SNDCOUNT == 0;
                    return this.DMACNT_L.Empty;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update()
            {
                if (this.DMACNT_H.Triggered)
                {
                    this.DMACNT_H.Triggered = false;

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
                if (this.DMACNT_H.Enabled && timing == this.DMACNT_H.StartTiming)  // enabled
                {
                    // Console.WriteLine($"DMA{this.index}: {this.SAD:x8} -> {this.DAD:x8}");
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
                if (!(this.Sound && this.DMACNT_H.StartTiming == DMAStartTiming.Special))
                    this.UpdateDMAAddress(this.DMADAD, this.DMACNT_H.DestAddrControl);
            }

            public int End()
            {
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
                    this.DMACNT_H.Disable();  // clear enabled bit
                }
                this.Active = false;

                // end of the transfer
                if (this.DMACNT_H.IRQOnEnd)
                {
                    this.IF.Request((ushort)((ushort)Interrupt.DMA << this.index));
                }

                if (this.Sound) this.SNDCOUNT = 4;

                // should actually happen at the start, but it does not matter, timers don't IRQ anyway during a DMA
                if (this.DMADAD.Address > 0x0800_0000 && this.DMASAD.Address > 0x0800_0000)
                    return 4 * ICycle;
                return 2 * ICycle;
            }
        }

        public readonly DMAChannel[] DMAChannels = new DMAChannel[4];

        public bool DMAActive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.DMAChannels[i].Active)
                    {
                        return true;
                    }
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

        private void DoDMA(DMAChannel DMA)
        {
            this.Log($"DMA: {DMA.SAD.ToString("x8")} -> {DMA.DAD.ToString("x8")}");

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
