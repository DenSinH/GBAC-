using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    public partial class Debug : Form
    {
        private GBA gba;
        private PictureBox[] CharBlocks;
        private RadioButton[] CharBlockColorModes;
        private GCHandle _rawBitmap;
        private ushort[] RawCharBlock;

        Label[] TimerCounters, TimerReloads, TimerPrescalers, TimerIRQEnables, TimerEnables, TimerCountUps;
        Label[] DMASAD, DMADAD, DMAUnitCount, DMADestAddrControl, DMASourceAddrControl, DMARepeat, DMAUnitLength, DMAStartTiming, DMAIRQ, DMAEnabled;

        const int CharBlockSize = 16 * 8;

        public Debug(GBA gba)
        {
            InitializeComponent();

            // disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            this.TimerCounters = new Label[4] { Timer0Counter, Timer1Counter, Timer2Counter, Timer3Counter };
            this.TimerReloads = new Label[4] { Timer0Reload, Timer1Reload, Timer2Reload, Timer3Reload };
            this.TimerPrescalers = new Label[4] { Timer0Prescaler, Timer1Prescaler, Timer2Prescaler, Timer3Prescaler };
            this.TimerIRQEnables = new Label[4] { Timer0IRQEnable, Timer1IRQEnable, Timer2IRQEnable, Timer3IRQEnable };
            this.TimerEnables = new Label[4] { Timer0Enabled, Timer1Enabled, Timer2Enabled, Timer3Enabled };
            this.TimerCountUps = new Label[4] { null, Timer1CountUp, Timer2CountUp, Timer3CountUp };

            this.DMASAD = new Label[4] { DMA0SAD, DMA1SAD, DMA2SAD, DMA3SAD };
            this.DMADAD = new Label[4] { DMA0DAD, DMA1DAD, DMA2DAD, DMA3DAD };
            this.DMAUnitCount = new Label[4] { DMA0UnitCount, DMA1UnitCount, DMA2UnitCount, DMA3UnitCount };
            this.DMADestAddrControl = new Label[4] { DMA0DestAddrControl, DMA1DestAddrControl, DMA2DestAddrControl, DMA3DestAddrControl };
            this.DMASourceAddrControl = new Label[4] { DMA0SourceAddrControl, DMA1SourceAddrControl, DMA2SourceAddrControl, DMA3SourceAddrControl };
            this.DMARepeat = new Label[4] { DMA0Repeat, DMA1Repeat, DMA2Repeat, DMA3Repeat };
            this.DMAUnitLength = new Label[4] { DMA0UnitLength, DMA1UnitLength, DMA2UnitLength, DMA3UnitLength };
            this.DMAStartTiming = new Label[4] { DMA0Timing, DMA1Timing, DMA2Timing, DMA3Timing };
            this.DMAIRQ = new Label[4] { DMA0IRQ, DMA1IRQ, DMA2IRQ, DMA3IRQ };
            this.DMAEnabled = new Label[4] { DMA0Enabled, DMA1Enabled, DMA2Enabled, DMA3Enabled };

            this.gba = gba;
            this.CharBlocks = new PictureBox[4] { this.CharBlock0, this.CharBlock1, this.CharBlock2, this.CharBlock3 };
            this.CharBlockColorModes = new RadioButton[4] { this.CharBlock04bpp, this.CharBlock14bpp, this.CharBlock24bpp, this.CharBlock34bpp };
            this.RawCharBlock = new ushort[CharBlockSize * CharBlockSize];
        }

        // same as PPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                 this.gba.mem.PAL[Address] |
                (this.gba.mem.PAL[Address + 1] << 8)
                );
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void UpdateAll()
        {
            switch (this.Tabs.SelectedIndex)
            {
                case 0:  // Registers
                    this.UpdateRegisterTab();
                    break;
                case 1:  // palette
                    break;
                case 2:  // CharBlocks
                    this.UpdateCharBlockTab();
                    break;
            }
        }

        private void UpdateDISPCNT()
        {
            //this.BGMode.Text = this.gba.mem.DISPCNT.BGMode.ToString();
            //this.DPFrameSelect.Text = this.gba.mem.DISPCNT.IsSet(DISPCNTFlags.DPFrameSelect) ? "1" : "0";
            //this.HBlankIntervalFree.Text = this.gba.mem.DISPCNT.IsSet(DISPCNTFlags.HBlankIntervalFree) ? "1" : "0";
            //this.OBJVRAMMapping.Text = this.gba.mem.DISPCNT.IsSet(DISPCNTFlags.OBJVRAMMapping) ? "1" : "0";
            //this.ForcedBlank.Text = this.gba.mem.DISPCNT.IsSet(DISPCNTFlags.ForcedBlank) ? "1" : "0";
            //this.Window0Display.Text = this.gba.mem.DISPCNT.DisplayBGWindow(0) ? "1" : "0";
            //this.Window1Display.Text = this.gba.mem.DISPCNT.DisplayBGWindow(1) ? "1" : "0";
            //this.OBJWindowDisplay.Text = this.gba.mem.DISPCNT.DisplayOBJWindow() ? "1" : "0";
        }

        private void UpdateDISPSTAT()
        {
            //this.VBlankFlag.Text = this.gba.mem.DISPSTAT.IsSet(DISPSTATFlags.VBlankFlag) ? "1" : "0";
            //this.HBlankFlag.Text = this.gba.mem.DISPSTAT.IsSet(DISPSTATFlags.HBlankFlag) ? "1" : "0";
            //this.VCounterFlag.Text = this.gba.mem.DISPSTAT.IsSet(DISPSTATFlags.VCounterFlag) ? "1" : "0";
            //this.VBlankIRQEnable.Text = this.gba.mem.DISPSTAT.IsSet(DISPSTATFlags.VBlankIRQEnable) ? "1" : "0";
            //this.HBlankIRQEnable.Text = this.gba.mem.DISPSTAT.IsSet(DISPSTATFlags.HBlankIRQEnable) ? "1" : "0";
            //this.VCountIRQEnable.Text = this.gba.mem.DISPSTAT.IsSet(DISPSTATFlags.VCounterIRQEnable) ? "1" : "0";
            //this.VCountSetting.Text = this.gba.mem.DISPSTAT.VCountSetting.ToString("d3");
        }

        private void UpdateVCOUNT()
        {
            // this.VCOUNT.Text = this.gba.mem.VCOUNT.CurrentScanline.ToString("d3");
        }

        private void UpdateInterruptControl()
        {
            InterruptControlInfo InterruptControlData = this.gba.cpu.GetInterruptControl();
            this.KEYCNT.Text = InterruptControlData.KEYCNT;
            this.IME.Text = InterruptControlData.IME;

            // Nice and hardcoded, I know
            this.IEVBlank.Text = ((InterruptControlData.IE & (ushort)Interrupt.LCDVBlank) > 0) ? "1" : "0";
            this.IEHBlank.Text = ((InterruptControlData.IE & (ushort)Interrupt.LCDHBlank) > 0) ? "1" : "0";
            this.IEVCOUNT.Text = ((InterruptControlData.IE & (ushort)Interrupt.LCDVCountMatch) > 0) ? "1" : "0";
            this.IETimers.Text = ((InterruptControlData.IE & 0x0078) >> 3).ToString("x1");
            this.IEDMA.Text = ((InterruptControlData.IE & 0x0f00) >> 8).ToString("x1"); ;
            this.IEKeypad.Text = ((InterruptControlData.IE & (ushort)Interrupt.Keypad) > 0) ? "1" : "0";
            this.IEGamePak.Text = ((InterruptControlData.IE & (ushort)Interrupt.GamePak) > 0) ? "1" : "0";

            this.IFVBlank.Text = ((InterruptControlData.IF & (ushort)Interrupt.LCDVBlank) > 0) ? "1" : "0";
            this.IFHBlank.Text = ((InterruptControlData.IF & (ushort)Interrupt.LCDHBlank) > 0) ? "1" : "0";
            this.IFVCOUNT.Text = ((InterruptControlData.IF & (ushort)Interrupt.LCDVCountMatch) > 0) ? "1" : "0";
            this.IFTimers.Text = ((InterruptControlData.IF & 0x0078) >> 3).ToString("x1");
            this.IFDMA.Text = ((InterruptControlData.IF & 0x0f00) >> 8).ToString("x1"); ;
            this.IFKeypad.Text = ((InterruptControlData.IF & (ushort)Interrupt.Keypad) > 0) ? "1" : "0";
            this.IFGamePak.Text = ((InterruptControlData.IF & (ushort)Interrupt.GamePak) > 0) ? "1" : "0";

            this.HALTCNT.Text = InterruptControlData.HALTCNT;
            this.IRQLabel.ForeColor = this.gba.cpu.mode == ARM7TDMI.Mode.IRQ ? Color.Green : Color.Red;
            this.SWILabel.ForeColor = this.gba.cpu.mode == ARM7TDMI.Mode.Supervisor ? Color.Green : Color.Red;
        }

        private void UpdateTimers()
        {
            int index = this.TimerTabs.SelectedIndex;
            TimerInfo info = this.gba.cpu.GetTimerInfo(index);

            this.TimerCounters[index].Text = info.Counter;
            this.TimerReloads[index].Text = info.Reload;
            this.TimerPrescalers[index].Text = info.Prescaler;
            this.TimerIRQEnables[index].Text = info.IRQEnabled;
            this.TimerEnables[index].Text = info.Enabled;
            if (index != 0) this.TimerCountUps[index].Text = info.CountUp;
        }

        private void UpdateDMA()
        {
            int index = this.DMATabs.SelectedIndex;
            DMAInfo info = this.gba.cpu.GetDMAInfo(index);

            this.DMALabel.ForeColor = this.gba.cpu.DMAActive ? Color.Green : Color.Red;
            this.DMASAD[index].Text = info.SAD;
            this.DMADAD[index].Text = info.DAD;
            this.DMAUnitCount[index].Text = info.UnitCount;
            this.DMADestAddrControl[index].Text = info.DestAddrControl;
            this.DMASourceAddrControl[index].Text = info.SourceAddrControl;
            this.DMARepeat[index].Text = info.Repeat;
            this.DMAUnitLength[index].Text = info.UnitLength;
            this.DMAStartTiming[index].Text = info.Timing;
            this.DMAIRQ[index].Text = info.IRQ;
            this.DMAEnabled[index].Text = info.Enabled;
        }

        private void UpdateRegisterTab()
        {
            this.UpdateDISPCNT();
            this.UpdateDISPSTAT();
            this.UpdateVCOUNT();
            this.UpdateInterruptControl();
            this.UpdateTimers();
            this.UpdateDMA();
        }

        private void GenCharBlock8bpp(uint index)
        {
            uint Address = index * 0x4000;
            uint PixelAddress;

            for (uint dTileY = 0; dTileY < 16; dTileY++)  // 16 to not go out of range
            {
                for (uint dTileX = 0; dTileX < 16; dTileX++)
                {
                    for (uint y = 0; y < 8; y++)
                    {
                        for (uint x = 0; x < 8; x++)
                        {
                            PixelAddress = Address + 8 * y + x;
                            this.RawCharBlock[CharBlockSize * (8 * dTileY + y) + 8 * dTileX + x] = 
                                this.GetPaletteEntry(2 * (uint)this.gba.mem.VRAM[PixelAddress]);
                        }
                    }

                    Address += 0x40;
                }
            }
            return;
        }

        private void GenCharBlock4bpp(uint index)
        {
            uint Address = index * 0x4000;
            uint PixelAddress;
            uint PaletteNibble;

            for (uint dTileY = 0; dTileY < 16; dTileY++)
            {
                for (uint dTileX = 0; dTileX < 16; dTileX++)
                {
                    for (uint y = 0; y < 8; y++)
                    {
                        for (uint x = 0; x < 8; x++)
                        {
                            PixelAddress = Address + 4 * y + (x >> 1);

                            PaletteNibble = this.gba.mem.VRAM[PixelAddress];
                            if ((x & 1) == 1) PaletteNibble >>= 4;

                            PaletteNibble &= 0x0f;

                            this.RawCharBlock[CharBlockSize * (8 * dTileY + y) + 8 * dTileX + x] =
                                this.GetPaletteEntry(2 * PaletteNibble);
                        }
                    }

                    Address += 0x20;
                }
            }
        }

        private void UpdateCharBlock(uint index)
        {
            Bitmap CharBlock;

            if (this.CharBlockColorModes[index].Checked)
                this.GenCharBlock4bpp(index);
            else
                this.GenCharBlock8bpp(index);

            this.CharBlocks[index].Image?.Dispose();

            this._rawBitmap = GCHandle.Alloc(this.RawCharBlock, GCHandleType.Pinned);
            CharBlock = new Bitmap(CharBlockSize, CharBlockSize, CharBlockSize * 2,
                        PixelFormat.Format16bppRgb555, _rawBitmap.AddrOfPinnedObject());

            CharBlock = new Bitmap(CharBlock, new Size(512, 512));

            this._rawBitmap.Free();
            
            this.CharBlocks[index].Image = (Image)CharBlock.Clone();
            this.CharBlocks[index].Update();
        }

        private void UpdateCharBlockTab()
        {
            this.UpdateCharBlock((uint)this.CharBlockTabs.SelectedIndex);
        }
    }
}
