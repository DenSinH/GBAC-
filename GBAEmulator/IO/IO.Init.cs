using System;

using GBAEmulator.CPU;
using GBAEmulator.Bus;
using GBAEmulator.Audio;
using GBAEmulator.Video;

namespace GBAEmulator.IO
{
    partial class IORAMSection
    {
        public void Init(PPU ppu, BUS bus)
        {
            this.DISPCNT  = new cDISPCNT(ppu);
            this.DISPSTAT = new cDISPSTAT(this.IF, ppu);
            this.VCOUNT   = new cVCOUNT(this.IF, this.DISPSTAT);
            this.BGCNT    = new cBGControl[4]   { new cBGControl(ppu, 0xdfff), new cBGControl(ppu, 0xdfff),
                                                  new cBGControl(ppu, 0xffff), new cBGControl(ppu, 0xffff) };

            this.BGHOFS   = new cBGScrolling[4] { new cBGScrolling(ppu, bus, true), new cBGScrolling(ppu, bus, false),
                                                  new cBGScrolling(ppu, bus, true), new cBGScrolling(ppu, bus, false) };
            this.BGVOFS   = new cBGScrolling[4] { new cBGScrolling(ppu, bus, true), new cBGScrolling(ppu, bus, false),
                                                  new cBGScrolling(ppu, bus, true), new cBGScrolling(ppu, bus, false) };

            this.BG2X = new cReferencePoint(ppu, bus);
            this.BG2Y = new cReferencePoint(ppu, bus);
            this.BG3X = new cReferencePoint(ppu, bus);
            this.BG3Y = new cReferencePoint(ppu, bus);

            this.BG2PA = new cRotationScaling(bus, true);
            this.BG2PB = new cRotationScaling(bus, false);
            this.BG2PC = new cRotationScaling(bus, true);
            this.BG2PD = new cRotationScaling(bus, false);

            this.BG3PA = new cRotationScaling(bus, true);
            this.BG3PB = new cRotationScaling(bus, false);
            this.BG3PC = new cRotationScaling(bus, true);
            this.BG3PD = new cRotationScaling(bus, false);

            this.WINH = new cWindowDimensions[2] { new cWindowDimensions(ppu, bus, true), new cWindowDimensions(ppu, bus, false) };
            this.WINV = new cWindowDimensions[2] { new cWindowDimensions(ppu, bus, true), new cWindowDimensions(ppu, bus, false) };

            this.WININ  = new cWindowControl();
            this.WINOUT = new cWindowControl();

            this.MOSAIC = new cMosaic(ppu);
            this.BLDCNT = new cBLDCNT(ppu);
            this.BLDALPHA = new cBLDALPHA(ppu);
            this.BLDY = new cBLDY(ppu);

            this.SIOCNT = new cSIOCNT(this.IF);

            this.KEYCNT = new cKeyInterruptControl(this);
            this.KEYINPUT = new cKeyInput(this.KEYCNT, this.IF);

            this.MasterUnusedRegister = new UnusedRegister(bus);
        }

        public void Layout(ARM7TDMI cpu, APU apu)
        {
            // ========================== LCD Registers ==========================
            this.Storage[0x00] = this.Storage[0x01] = this.DISPCNT;
            this.Storage[0x02] = this.Storage[0x03] = new DefaultRegister();  // green swap
            this.Storage[0x04] = this.Storage[0x05] = this.DISPSTAT;
            this.Storage[0x06] = this.Storage[0x07] = this.VCOUNT;

            this.Storage[0x08] = this.Storage[0x09] = this.BGCNT[0];
            this.Storage[0x0a] = this.Storage[0x0b] = this.BGCNT[1];
            this.Storage[0x0c] = this.Storage[0x0d] = this.BGCNT[2];
            this.Storage[0x0e] = this.Storage[0x0f] = this.BGCNT[3];

            this.Storage[0x10] = this.Storage[0x11] = this.BGHOFS[0];
            this.Storage[0x12] = this.Storage[0x13] = this.BGVOFS[0];
            this.Storage[0x14] = this.Storage[0x15] = this.BGHOFS[1];
            this.Storage[0x16] = this.Storage[0x17] = this.BGVOFS[1];
            this.Storage[0x18] = this.Storage[0x19] = this.BGHOFS[2];
            this.Storage[0x1a] = this.Storage[0x1b] = this.BGVOFS[2];
            this.Storage[0x1c] = this.Storage[0x1d] = this.BGHOFS[3];
            this.Storage[0x1e] = this.Storage[0x1f] = this.BGVOFS[3];

            this.Storage[0x20] = this.Storage[0x21] = this.BG2PA;
            this.Storage[0x22] = this.Storage[0x23] = this.BG2PB;
            this.Storage[0x24] = this.Storage[0x25] = this.BG2PC;
            this.Storage[0x26] = this.Storage[0x27] = this.BG2PD;

            this.Storage[0x28] = this.Storage[0x29] = this.BG2X.lower;
            this.Storage[0x2a] = this.Storage[0x2b] = this.BG2X.upper;
            this.Storage[0x2c] = this.Storage[0x2d] = this.BG2Y.lower;
            this.Storage[0x2e] = this.Storage[0x2f] = this.BG2Y.upper;

            this.Storage[0x30] = this.Storage[0x31] = this.BG3PA;
            this.Storage[0x32] = this.Storage[0x33] = this.BG3PB;
            this.Storage[0x34] = this.Storage[0x35] = this.BG3PC;
            this.Storage[0x36] = this.Storage[0x37] = this.BG3PD;

            this.Storage[0x38] = this.Storage[0x39] = this.BG3X.lower;
            this.Storage[0x3a] = this.Storage[0x3b] = this.BG3X.upper;
            this.Storage[0x3c] = this.Storage[0x3d] = this.BG3Y.lower;
            this.Storage[0x3e] = this.Storage[0x3f] = this.BG3Y.upper;

            this.Storage[0x40] = this.Storage[0x41] = this.WINH[0];
            this.Storage[0x42] = this.Storage[0x43] = this.WINH[1];

            this.Storage[0x44] = this.Storage[0x45] = this.WINV[0];
            this.Storage[0x46] = this.Storage[0x47] = this.WINV[1];

            this.Storage[0x48] = this.Storage[0x49] = this.WININ;
            this.Storage[0x4a] = this.Storage[0x4b] = this.WINOUT;

            this.Storage[0x4c] = this.Storage[0x4d] = this.MOSAIC;
            this.Storage[0x4e] = this.Storage[0x4f] = this.MasterZeroRegister;  // unused MOSAIC bits, lower half readable

            this.Storage[0x50] = this.Storage[0x51] = this.BLDCNT;
            this.Storage[0x52] = this.Storage[0x53] = this.BLDALPHA;
            this.Storage[0x54] = this.Storage[0x55] = this.BLDY;
            this.Storage[0x56] = this.Storage[0x57] = this.MasterZeroRegister;  // lower half readable

            for (int i = 0x58; i < 0x60; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // ========================== Sound Registers ==========================
            this.Storage[0x60] = this.Storage[0x61] = new SquareCNT_L(apu.sq1);
            this.Storage[0x62] = this.Storage[0x63] = new SquareCNT_H(apu.sq1);
            this.Storage[0x64] = this.Storage[0x65] = new SquareCNT_X(apu.sq1);
            this.Storage[0x66] = this.Storage[0x67] = this.MasterUnusedRegister.upper;

            this.Storage[0x68] = this.Storage[0x69] = new SquareCNT_H(apu.sq2);
            this.Storage[0x6a] = this.Storage[0x6b] = this.MasterUnusedRegister.upper;
            this.Storage[0x6c] = this.Storage[0x6d] = new SquareCNT_X(apu.sq2);
            this.Storage[0x6e] = this.Storage[0x6f] = this.MasterUnusedRegister.upper;

            this.Storage[0x70] = this.Storage[0x71] = new WaveCNT_L(apu.wave);
            this.Storage[0x72] = this.Storage[0x73] = new WaveCNT_H(apu.wave);
            this.Storage[0x74] = this.Storage[0x75] = new WaveCNT_X(apu.wave);
            this.Storage[0x76] = this.Storage[0x77] = this.MasterUnusedRegister.upper;

            this.Storage[0x78] = this.Storage[0x79] = new NoiseCNT_L(apu.noise);
            this.Storage[0x7a] = this.Storage[0x7b] = this.MasterUnusedRegister.upper;
            this.Storage[0x7c] = this.Storage[0x7d] = new NoiseCNT_H(apu.noise);
            this.Storage[0x7e] = this.Storage[0x7f] = this.MasterUnusedRegister.upper;

            this.Storage[0x80] = this.Storage[0x81] = new SOUNDCNT_L(apu);
            this.Storage[0x82] = this.Storage[0x83] = new SOUNDCNT_H(apu, cpu.Timers[0], cpu.Timers[1]);
            this.Storage[0x84] = this.Storage[0x85] = new SOUNDCNT_X(apu);
            this.Storage[0x86] = this.Storage[0x87] = this.MasterUnusedRegister.upper;
            this.Storage[0x88] = this.Storage[0x89] = new DefaultRegister();  // SOUNDBIAS (unnecessary?)

            this.Storage[0x8a] = this.Storage[0x8b] = this.MasterUnusedRegister.lower;
            this.Storage[0x8c] = this.Storage[0x8d] = this.MasterUnusedRegister.upper;
            this.Storage[0x8e] = this.Storage[0x8f] = this.MasterUnusedRegister.lower;

            for (int i = 0; i < 0x10; i += 2) this.Storage[0x90 + i] = this.Storage[0x91 + i] = new WAVE_RAM(apu.wave, i);

            this.Storage[0xa0] = this.Storage[0xa1] = new FIFO_Data(apu.FIFOA, cpu.bus, true);
            this.Storage[0xa2] = this.Storage[0xa3] = new FIFO_Data(apu.FIFOA, cpu.bus, false);
            this.Storage[0xa4] = this.Storage[0xa5] = new FIFO_Data(apu.FIFOB, cpu.bus, true);
            this.Storage[0xa6] = this.Storage[0xa7] = new FIFO_Data(apu.FIFOB, cpu.bus, false);

            for (int i = 0xa8; i < 0xb0; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // ======================= DMA Transfer Channels =======================
            this.Storage[0xb0] = this.Storage[0xb1] = cpu.DMAChannels[0].DMASAD.lower;
            this.Storage[0xb2] = this.Storage[0xb3] = cpu.DMAChannels[0].DMASAD.upper;
            this.Storage[0xb4] = this.Storage[0xb5] = cpu.DMAChannels[0].DMADAD.lower;
            this.Storage[0xb6] = this.Storage[0xb7] = cpu.DMAChannels[0].DMADAD.upper;
            this.Storage[0xb8] = this.Storage[0xb9] = cpu.DMAChannels[0].DMACNT_L;
            this.Storage[0xba] = this.Storage[0xbb] = cpu.DMAChannels[0].DMACNT_H;

            this.Storage[0xbc] = this.Storage[0xbd] = cpu.DMAChannels[1].DMASAD.lower;
            this.Storage[0xbe] = this.Storage[0xbf] = cpu.DMAChannels[1].DMASAD.upper;
            this.Storage[0xc0] = this.Storage[0xc1] = cpu.DMAChannels[1].DMADAD.lower;
            this.Storage[0xc2] = this.Storage[0xc3] = cpu.DMAChannels[1].DMADAD.upper;
            this.Storage[0xc4] = this.Storage[0xc5] = cpu.DMAChannels[1].DMACNT_L;
            this.Storage[0xc6] = this.Storage[0xc7] = cpu.DMAChannels[1].DMACNT_H;

            this.Storage[0xc8] = this.Storage[0xc9] = cpu.DMAChannels[2].DMASAD.lower;
            this.Storage[0xca] = this.Storage[0xcb] = cpu.DMAChannels[2].DMASAD.upper;
            this.Storage[0xcc] = this.Storage[0xcd] = cpu.DMAChannels[2].DMADAD.lower;
            this.Storage[0xce] = this.Storage[0xcf] = cpu.DMAChannels[2].DMADAD.upper;
            this.Storage[0xd0] = this.Storage[0xd1] = cpu.DMAChannels[2].DMACNT_L;
            this.Storage[0xd2] = this.Storage[0xd3] = cpu.DMAChannels[2].DMACNT_H;

            this.Storage[0xd4] = this.Storage[0xd5] = cpu.DMAChannels[3].DMASAD.lower;
            this.Storage[0xd6] = this.Storage[0xd7] = cpu.DMAChannels[3].DMASAD.upper;
            this.Storage[0xd8] = this.Storage[0xd9] = cpu.DMAChannels[3].DMADAD.lower;
            this.Storage[0xda] = this.Storage[0xdb] = cpu.DMAChannels[3].DMADAD.upper;
            this.Storage[0xdc] = this.Storage[0xdd] = cpu.DMAChannels[3].DMACNT_L;
            this.Storage[0xde] = this.Storage[0xdf] = cpu.DMAChannels[3].DMACNT_H;

            for (int i = 0xe0; i < 0x100; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // ========================== Timer Registers ==========================
            this.Storage[0x100] = this.Storage[0x101] = cpu.Timers[0].Data;
            this.Storage[0x102] = this.Storage[0x103] = cpu.Timers[0].Control;

            this.Storage[0x104] = this.Storage[0x105] = cpu.Timers[1].Data;
            this.Storage[0x106] = this.Storage[0x107] = cpu.Timers[1].Control;

            this.Storage[0x108] = this.Storage[0x109] = cpu.Timers[2].Data;
            this.Storage[0x10a] = this.Storage[0x10b] = cpu.Timers[2].Control;

            this.Storage[0x10c] = this.Storage[0x10d] = cpu.Timers[3].Data;
            this.Storage[0x10e] = this.Storage[0x10f] = cpu.Timers[3].Control;

            for (int i = 0x110; i < 0x120; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // ============================ SIO (1) ============================
            this.Storage[0x120] = this.Storage[0x121] = this.SIODATA32.lower;  // shared
            this.Storage[0x122] = this.Storage[0x123] = this.SIODATA32.upper;  // shared
            this.Storage[0x124] = this.Storage[0x125] = new DefaultRegister();
            this.Storage[0x126] = this.Storage[0x127] = new DefaultRegister();
            this.Storage[0x128] = this.Storage[0x129] = this.SIOCNT;
            this.Storage[0x12a] = this.Storage[0x12b] = this.SIODATA8;  // shared

            this.Storage[0x012c] = this.Storage[0x012d] = this.MasterUnusedRegister.lower;
            this.Storage[0x012e] = this.Storage[0x012f] = this.MasterUnusedRegister.upper;

            // =========================== Keypad Input ========================
            this.Storage[0x0130] = this.Storage[0x0131] = this.KEYINPUT;
            this.Storage[0x0132] = this.Storage[0x0133] = this.KEYCNT;

            // ============================ SIO (2) ============================
            this.Storage[0x134] = this.Storage[0x135] = this.RCNT;

            this.Storage[0x136] = this.Storage[0x137] = this.MasterUnusedRegister.upper;  // note offset!
            for (int i = 0x0138; i < 0x140; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.Storage[0x140] = this.Storage[0x141] = new DefaultRegister();

            for (int i = 0x0142; i < 0x150; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.Storage[0x150] = this.Storage[0x151] = new DefaultRegister();
            this.Storage[0x152] = this.Storage[0x153] = new DefaultRegister();
            this.Storage[0x154] = this.Storage[0x155] = new DefaultRegister();
            this.Storage[0x156] = this.Storage[0x157] = new DefaultRegister();
            this.Storage[0x158] = this.Storage[0x159] = new DefaultRegister();

            this.Storage[0x15a] = this.Storage[0x15b] = this.MasterZeroRegister;
            for (int i = 0x015c; i < 0x200; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            // ========== Interrupt, Waitstate and Power-Down Control ===========
            this.Storage[0x0200] = this.Storage[0x0201] = this.IE;
            this.Storage[0x0202] = this.Storage[0x0203] = this.IF;
            this.Storage[0x0204] = this.Storage[0x0205] = this.WAITCNT;

            this.Storage[0x0206] = this.Storage[0x0207] = this.MasterZeroRegister;
            this.Storage[0x0208] = this.Storage[0x0209] = this.IME;
            this.Storage[0x020a] = this.Storage[0x020b] = this.MasterZeroRegister;

            for (int i = 0x20c; i < 0x300; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            this.Storage[0x0300] = this.Storage[0x0301] = this.HALTCNT;
            this.Storage[0x0302] = this.Storage[0x0303] = this.MasterZeroRegister;

            for (int i = 0x304; i < 0x400; i += 4)
            {
                this.Storage[i] = this.Storage[i + 1] = this.MasterUnusedRegister.lower;
                this.Storage[i + 2] = this.Storage[i + 3] = this.MasterUnusedRegister.upper;
            }

            for (int i = 0; i < 0x400; i++)
            {
                if (this.Storage[i] is null)
                {
                    this.Error(i.ToString("x4") + " in IORAM not initialized");
                    Console.ReadKey();
                }
            }

            this.InitLCD();
        }

        public void InitLCD()
        {
            this.LCDRegisters = new LCDRegister2[]
            {
                this.DISPCNT,
                this.DISPSTAT,
                this.VCOUNT,

                this.BGCNT[0],
                this.BGCNT[1],
                this.BGCNT[2],
                this.BGCNT[3],

                this.BGHOFS[0],
                this.BGVOFS[0],
                this.BGHOFS[1],
                this.BGVOFS[1],
                this.BGHOFS[2],
                this.BGVOFS[2],
                this.BGHOFS[3],
                this.BGVOFS[3],

                this.BG2PA,
                this.BG2PB,
                this.BG2PC,
                this.BG2PD,
                this.BG2X.lower,
                this.BG2X.upper,
                this.BG2Y.lower,
                this.BG2Y.upper,

                this.BG3PA,
                this.BG3PB,
                this.BG3PC,
                this.BG3PD,
                this.BG3X.lower,
                this.BG3X.upper,
                this.BG3Y.lower,
                this.BG3Y.upper,

                this.WINH[0],
                this.WINH[1],
                this.WINV[0],
                this.WINV[1],
                this.WININ,
                this.WINOUT,

                this.MOSAIC,

                this.BLDCNT,
                this.BLDALPHA,
                this.BLDY
            };
        }

    }
}
