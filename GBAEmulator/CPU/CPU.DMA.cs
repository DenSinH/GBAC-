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

            if (dmacnt_l.UnitCount == 0)
                this.EndDMA(dmacnt_h, dmacnt_l, dmasad, dmadad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateDMAAddress(cDMAAddress dmaxad, AddrControl control, uint amount)
        {
            switch (control)
            {
                case AddrControl.IncrementReload:
                case AddrControl.Increment:
                    dmaxad.Address += amount;
                    break;
                case AddrControl.Decrement:
                    dmaxad.Address -= amount;
                    break;
                case AddrControl.Fixed:
                    break;
            }
        }

        private void EndDMA(cDMACNT_H dmacnt_h, cDMACNT_L dmacnt_l, cDMAAddress dmasad, cDMAAddress dmadad)
        {
            if (dmacnt_h.DMARepeat)
            {

                dmacnt_l.Reload();
                if (dmacnt_h.DestAddrControl == AddrControl.IncrementReload)
                {
                    dmadad.Reload();
                }

                // set to inactive if not immedieately starting
                dmacnt_h.Active = dmacnt_h.StartTiming == DMAStartTiming.Immediately;
            }
            else
            {
                // end of the transfer
                if (dmacnt_h.IRQOnEnd)
                {
                    this.IF.Request((Interrupt)((ushort)Interrupt.DMA << dmacnt_h.index));
                }
                dmacnt_h.Disable();
                dmacnt_h.Active = false;
            }
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
