﻿using System;
using System.Runtime.CompilerServices;

using GBAEmulator.IO;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public class DMAChannel
        {
            public readonly cDMACNT_H DMACNT_H;
            public readonly cDMACNT_L DMACNT_L;
            private ushort SNDCOUNT = 4;
            public readonly cDMAAddress DMASAD;
            public readonly cDMAAddress DMADAD;
            private readonly cIF IF;
            private readonly ARM7TDMI cpu;
            private readonly int index;
            private readonly bool Sound;

            public bool Active { get; private set; }

            public DMAChannel(ARM7TDMI cpu, int index)
            {
                this.DMACNT_H = new cDMACNT_H(this, index == 3);
                this.DMACNT_L = new cDMACNT_L(cpu.bus, (ushort)(index == 3 ? 0xffff : 0x3fff));
                this.DMASAD = new cDMAAddress(cpu.bus, index == 0);
                this.DMADAD = new cDMAAddress(cpu.bus, index != 3);
                this.cpu = cpu;
                this.IF = cpu.IO.IF;
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

            public void Reload()
            {
                this.DMADAD.Reload();
                this.DMASAD.Reload();
                this.DMACNT_L.Reload();
            }

            public bool Trigger(DMAStartTiming timing)
            {
                if (this.DMACNT_H.Enabled && timing == this.DMACNT_H.StartTiming)  // enabled
                {
                    // Console.WriteLine($"DMA{this.index}: {this.SAD:x8} -> {this.DAD:x8}");
                    this.cpu.DMAActive = this.Active = true;
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
                this.cpu.ResetDMAActive();

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
    }
}
