using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMA(DMAStartTiming timing)
        {
            for (int i = 0; i < 4; i++)
                if (!this.DMACNT_H[i].Active) this.DMACNT_H[i].Trigger(timing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMASpecial(int i)
        {
            // todo: Sound FIFO
            this.DMACNT_H[i].Trigger(DMAStartTiming.Special);
        }

        private void DoDMA(int i)
        {
            cDMACNT_H dmacnt_h = this.DMACNT_H[i];
            cDMACNT_L dmacnt_l = this.DMACNT_L[i];
            cDMAAddress dmasad = this.DMASAD[i];
            cDMAAddress dmadad = this.DMADAD[i];

            this.Log($"DMA: {dmasad.Address.ToString("x8")} -> {dmadad.Address.ToString("x8")}");

            uint UnitLength = (uint)(dmacnt_h.DMATransferType ? 4 : 2);  // bytes: 16 / 32 bits
            
            if (UnitLength == 4)
            {
                this.SetWordAt(dmadad.Address, this.GetWordAt(dmasad.Address));
            }
            else  // 16 bit
            {
                this.SetHalfWordAt(dmadad.Address, this.GetHalfWordAt(dmasad.Address));
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
            // Immediate DMA transfers should ignore the Repeat bit  - Fleroviux
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
                    this.IF.Request((ushort)((ushort)Interrupt.DMA << dmacnt_h.index));
                }
                dmacnt_h.Disable();  // clear enabled bit
            }
            dmacnt_h.Active = false;
        }

        private int HandleDMAs()
        {
            for (int i = 0; i < 4; i++)
            {
                // DMA channel 0 has highest priority, 3 the lowest
                if (this.DMACNT_H[i].Active)
                {
                    this.DoDMA(i);
                    // 2N cycles for first, then 2S every cycle after
                    // Internal time for DMA processing is 2I (normally), or 4I (if both source and destination are in gamepak memory area)
                    return SCycle << 1;
                }
            }
            return 0;
        }
    }
}
