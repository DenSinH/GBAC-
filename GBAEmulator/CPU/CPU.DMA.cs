using System;
using System.Runtime.CompilerServices;

using GBAEmulator.IO;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public bool DMAActive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.IO.DMACNT_H[i].Active) return true;
                }
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMA(DMAStartTiming timing)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!this.IO.DMACNT_H[i].Active)
                {
                    this.IO.DMACNT_H[i].Trigger(timing);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMASpecial(int i)
        {
            // todo: Sound FIFO
            this.IO.DMACNT_H[i].Trigger(DMAStartTiming.Special);
        }

        private void DoDMA(int i)
        {
            cDMACNT_H dmacnt_h = this.IO.DMACNT_H[i];
            cDMACNT_L dmacnt_l = this.IO.DMACNT_L[i];
            cDMAAddress dmasad = this.IO.DMASAD[i];
            cDMAAddress dmadad = this.IO.DMADAD[i];

            this.Log($"DMA: {dmasad.Address.ToString("x8")} -> {dmadad.Address.ToString("x8")}");

            uint UnitLength = (uint)(dmacnt_h.DMATransferType ? 4 : 2);  // bytes: 32 / 16 bits

            if (UnitLength == 4)
            {
                // force alignment happens in memory handler
                this.mem.SetWordAt(dmadad.Address, this.mem.GetWordAt(dmasad.Address));
            }
            else  // 16 bit
            {
                // force alignment happens in memory handler
                this.mem.SetHalfWordAt(dmadad.Address, this.mem.GetHalfWordAt(dmasad.Address));
            }

            this.UpdateDMAAddress(dmasad, dmacnt_h.SourceAddrControl, UnitLength);
            this.UpdateDMAAddress(dmadad, dmacnt_h.DestAddrControl, UnitLength);
            dmacnt_l.UnitCount--;

            if (dmacnt_l.Empty)
                this.EndDMA(dmacnt_h, dmacnt_l, dmasad, dmadad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateDMAAddress(cDMAAddress dmaxad, AddrControl control, uint amount)
        {
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

        private void EndDMA(cDMACNT_H dmacnt_h, cDMACNT_L dmacnt_l, cDMAAddress dmasad, cDMAAddress dmadad)
        {
            // should actually happen at the start, but it does not matter, timers don't IRQ anyway during a DMA
            InstructionCycles += 2 * ICycle;
            if (dmadad.Address > 0x0800_0000 && dmasad.Address > 0x0800_0000)
                InstructionCycles += 2 * ICycle;

            // Immediate DMA transfers should ignore the Repeat bit - Fleroviux
            if (dmacnt_h.DMARepeat && dmacnt_h.StartTiming != DMAStartTiming.Immediately)
            {
                dmacnt_l.Reload();
                if (dmacnt_h.DestAddrControl == AddrControl.IncrementReload)
                {
                    dmadad.Reload();
                }
            }
            else
            {
                // end of the transfer
                if (dmacnt_h.IRQOnEnd)
                {
                    this.IO.IF.Request((ushort)((ushort)Interrupt.DMA << dmacnt_h.index));
                }
                dmacnt_h.Disable();  // clear enabled bit
            }
            dmacnt_h.Active = false;
        }

        private void HandleDMAs()
        {
            for (int i = 0; i < 4; i++)
            {
                // DMA channel 0 has highest priority, 3 the lowest
                if (this.IO.DMACNT_H[i].Active)
                {
                    this.DoDMA(i);
                }
            }
        }
    }
}
