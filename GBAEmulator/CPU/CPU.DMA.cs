using System;
using System.Runtime.CompilerServices;

using GBAEmulator.Memory;

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
                    if (this.mem.IORAMSection.DMACNT_H[i].Active) return true;
                }
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMA(DMAStartTiming timing)
        {
            for (int i = 0; i < 4; i++)
                if (!this.mem.IORAMSection.DMACNT_H[i].Active) this.mem.IORAMSection.DMACNT_H[i].Trigger(timing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TriggerDMASpecial(int i)
        {
            // todo: Sound FIFO
            this.mem.IORAMSection.DMACNT_H[i].Trigger(DMAStartTiming.Special);
        }

        private void DoDMA(int i)
        {
            cIORAM.cDMACNT_H dmacnt_h = this.mem.IORAMSection.DMACNT_H[i];
            cIORAM.cDMACNT_L dmacnt_l = this.mem.IORAMSection.DMACNT_L[i];
            cIORAM.cDMAAddress dmasad = this.mem.IORAMSection.DMASAD[i];
            cIORAM.cDMAAddress dmadad = this.mem.IORAMSection.DMADAD[i];

            this.Log($"DMA: {dmasad.Address.ToString("x8")} -> {dmadad.Address.ToString("x8")}");

            uint UnitLength = (uint)(dmacnt_h.DMATransferType ? 4 : 2);  // bytes: 32 / 16 bits
            uint DMAData;

            if (UnitLength == 4)
            {
                DMAData = this.mem.GetWordAt(dmasad.Address & 0xffff_fffc);
                this.mem.SetWordAt(dmadad.Address & 0xffff_fffc, DMAData, offset: dmadad.Address & 3);
            }
            else  // 16 bit
            {
                DMAData = this.mem.GetHalfWordAt(dmasad.Address & 0xffff_fffe);
                this.mem.SetHalfWordAt(dmadad.Address & 0xffff_fffe, (ushort)DMAData, offset: dmadad.Address & 1);
            }

            this.UpdateDMAAddress(dmasad, dmacnt_h.SourceAddrControl, UnitLength);
            this.UpdateDMAAddress(dmadad, dmacnt_h.DestAddrControl, UnitLength);
            dmacnt_l.UnitCount--;

            if (dmacnt_l.Empty)
                this.EndDMA(dmacnt_h, dmacnt_l, dmasad, dmadad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateDMAAddress(cIORAM.cDMAAddress dmaxad, AddrControl control, uint amount)
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

        private void EndDMA(cIORAM.cDMACNT_H dmacnt_h, cIORAM.cDMACNT_L dmacnt_l, cIORAM.cDMAAddress dmasad, cIORAM.cDMAAddress dmadad)
        {
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
                    this.mem.IORAMSection.IF.Request((ushort)((ushort)Interrupt.DMA << dmacnt_h.index));
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
                if (this.mem.IORAMSection.DMACNT_H[i].Active)
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
