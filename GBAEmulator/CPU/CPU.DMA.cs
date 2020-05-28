using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void DoDMA(int i)
        {
            cDMACNT_H dmacnt_h = this.DMACNT_H[i];
            cDMACNT_L dmacnt_l = this.DMACNT_L[i];
            cDMAAddress dmasad = this.DMASAD[i];
            cDMAAddress dmadad = this.DMADAD[i];

            uint UnitLength = (uint)(dmacnt_h.DMATransferType ? 32 : 16);  // 16 / 32 bits

            if (UnitLength == 32)
            {
                this.SetWordAt(dmadad.Address, this.GetWordAt(dmasad.Address));
            }
            else  // 16 bit
            {
                this.SetHalfWordAt(dmadad.Address, this.GetHalfWordAt(dmasad.Address));
            }
            this.UpdateDMAAddress(dmasad, dmacnt_h.SourceAddrControl, UnitLength);
            this.UpdateDMAAddress(dmadad, dmacnt_h.DestAddrControl, UnitLength);
            dmacnt_l.WordCount--;

            if (dmacnt_l.WordCount == 0)
                this.EndDMA(i);
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

        private void EndDMA(int i)
        {

        }

        private void HandleDMAs()
        {
            for (int i = 0; i < 4; i++)
            {
                // DMA channel 0 has highest priority, 3 the lowest
                if (this.DMACNT_H[i].Active)
                {
                    this.DoDMA(i);
                    return;
                }
            }
        }
    }
}
