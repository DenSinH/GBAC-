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

        const int CharBlockSize = 32 * 8;

        public Debug(GBA gba)
        {
            InitializeComponent();

            this.gba = gba;
            this.CharBlocks = new PictureBox[4] { this.CharBlock0, this.CharBlock1, this.CharBlock2, this.CharBlock3 };
            this.CharBlockColorModes = new RadioButton[4] { this.CharBlock04bpp, this.CharBlock14bpp, this.CharBlock14bpp, this.CharBlock14bpp };
            this.RawCharBlock = new ushort[CharBlockSize * CharBlockSize];
        }

        // same as PPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetPaletteEntry(uint Address)
        {
            // Address within palette memory
            return (ushort)(
                this.gba.cpu.PaletteRAM[Address] |
                (this.gba.cpu.PaletteRAM[Address + 1] << 8)
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
            this.BGMode.Text = this.gba.cpu.DISPCNT.BGMode.ToString();
            this.DPFrameSelect.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.DPFrameSelect) ? "0" : "1";
            this.HBlankIntervalFree.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.HBlankIntervalFree) ? "0" : "1";
            this.OBJVRAMMapping.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.OBJVRAMMapping) ? "0" : "1";
            this.ForcedBlank.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.ForcedBlank) ? "0" : "1";
            this.Window0Display.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.WindowDisplay0) ? "0" : "1";
            this.Window1Display.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.WindowDisplay1) ? "0" : "1";
            this.OBJWindowDisplay.Text = this.gba.cpu.DISPCNT.IsSet(CPU.ARM7TDMI.DISPCNTFlags.OBJWindowDisplay) ? "0" : "1";
        }

        private void UpdateDISPSTAT()
        {
            this.VBlankFlag.Text = this.gba.cpu.DISPSTAT.IsSet(CPU.ARM7TDMI.DISPSTATFlags.VBlankFlag) ? "0" : "1";
            this.HBlankFlag.Text = this.gba.cpu.DISPSTAT.IsSet(CPU.ARM7TDMI.DISPSTATFlags.HBlankFlag) ? "0" : "1";
            this.VCounterFlag.Text = this.gba.cpu.DISPSTAT.IsSet(CPU.ARM7TDMI.DISPSTATFlags.VCounterFlag) ? "0" : "1";
            this.VBlankIRQEnable.Text = this.gba.cpu.DISPSTAT.IsSet(CPU.ARM7TDMI.DISPSTATFlags.VBlankIRQEnable) ? "0" : "1";
            this.HBlankIRQEnable.Text = this.gba.cpu.DISPSTAT.IsSet(CPU.ARM7TDMI.DISPSTATFlags.HBlankIRQEnable) ? "0" : "1";
            this.VCountIRQEnable.Text = this.gba.cpu.DISPSTAT.IsSet(CPU.ARM7TDMI.DISPSTATFlags.VCounterIRQEnable) ? "0" : "1";
            this.VCountSetting.Text = this.gba.cpu.DISPSTAT.VCountSetting.ToString("d3");
        }

        private void UpdateVCOUNT()
        {
            this.VCOUNT.Text = this.gba.cpu.VCOUNT.CurrentScanline.ToString("d3");
        }

        private void UpdateInterruptControl()
        {
            InterruptControlInfo InterruptControlData = this.gba.cpu.GetInterruptControl();
            this.KEYCNT.Text = InterruptControlData.KEYCNT;
            this.IME.Text = InterruptControlData.IME;

            // Nice and hardcoded, I know
            this.IEHBlank.Text = ((InterruptControlData.IE & (ushort)ARM7TDMI.Interrupt.LCDVBlank) > 0) ? "1" : "0";
            this.IEVBlank.Text = ((InterruptControlData.IE & (ushort)ARM7TDMI.Interrupt.LCDHBlank) > 0) ? "1" : "0";
            this.IEVCOUNT.Text = ((InterruptControlData.IE & (ushort)ARM7TDMI.Interrupt.LCDVCountMatch) > 0) ? "1" : "0";
            this.IETimers.Text = ((InterruptControlData.IE & 0x0078) >> 3).ToString("x1");
            this.IESIO.Text = ((InterruptControlData.IE & 0x0f00) >> 8).ToString("x1"); ;
            this.IEKeypad.Text = ((InterruptControlData.IE & (ushort)ARM7TDMI.Interrupt.Keypad) > 0) ? "1" : "0";
            this.IEGamePak.Text = ((InterruptControlData.IE & (ushort)ARM7TDMI.Interrupt.GamePak) > 0) ? "1" : "0";

            this.IFHBlank.Text = ((InterruptControlData.IF & (ushort)ARM7TDMI.Interrupt.LCDVBlank) > 0) ? "1" : "0";
            this.IFVBlank.Text = ((InterruptControlData.IF & (ushort)ARM7TDMI.Interrupt.LCDHBlank) > 0) ? "1" : "0";
            this.IFVCOUNT.Text = ((InterruptControlData.IF & (ushort)ARM7TDMI.Interrupt.LCDVCountMatch) > 0) ? "1" : "0";
            this.IFTimers.Text = ((InterruptControlData.IF & 0x0078) >> 3).ToString("x1");
            this.IFSIO.Text = ((InterruptControlData.IF & 0x0f00) >> 8).ToString("x1"); ;
            this.IFKeypad.Text = ((InterruptControlData.IF & (ushort)ARM7TDMI.Interrupt.Keypad) > 0) ? "1" : "0";
            this.IFGamePak.Text = ((InterruptControlData.IF & (ushort)ARM7TDMI.Interrupt.GamePak) > 0) ? "1" : "0";

            this.HALTCNT.Text = InterruptControlData.HALTCNT;
            this.IRQLabel.ForeColor = this.gba.cpu.mode == ARM7TDMI.Mode.IRQ ? Color.Green : Color.Red;
            this.SWILabel.ForeColor = this.gba.cpu.mode == ARM7TDMI.Mode.Supervisor ? Color.Green : Color.Red;
        }

        private void UpdateRegisterTab()
        {
            this.UpdateDISPCNT();
            this.UpdateDISPSTAT();
            this.UpdateVCOUNT();
            this.UpdateInterruptControl();
        }

        private void GenCharBlock8bpp(uint index)
        {
            uint Address = index * 0x4000;
            uint PixelAddress;

            for (uint dTileY = 0; dTileY < 8; dTileY++)  // 16 to not go out of range
            {
                for (uint dTileX = 0; dTileX < 32; dTileX++)
                {
                    for (uint y = 0; y < 8; y++)
                    {
                        for (uint x = 0; x < 8; x++)
                        {
                            PixelAddress = Address + 8 * y + x;

                            if (this.gba.cpu.VRAM[PixelAddress] == 0)
                            {
                                this.RawCharBlock[CharBlockSize * (8 * dTileY + y) + 8 * dTileX + x] = 0;
                                continue;
                            }

                            this.RawCharBlock[CharBlockSize * (8 * dTileY + y) + 8 * dTileX + x] = 
                                this.GetPaletteEntry(2 * (uint)this.gba.cpu.VRAM[PixelAddress]);
                        }
                    }

                    Address += 0x40;
                }
            }
        }

        private void GenCharBlock4bpp(uint index)
        {
            uint Address = index * 0x4000;
            uint PixelAddress;
            uint PaletteNibble;

            for (uint dTileY = 0; dTileY < 16; dTileY++)
            {
                for (uint dTileX = 0; dTileX < 32; dTileX++)
                {
                    for (uint y = 0; y < 8; y++)
                    {
                        for (uint x = 0; x < 8; x++)
                        {
                            PixelAddress = Address + 4 * y + (x >> 1);

                            PaletteNibble = this.gba.cpu.VRAM[PixelAddress];
                            if ((x & 1) == 1) PaletteNibble >>= 4;

                            PaletteNibble &= 0x0f;
                            if (PaletteNibble == 0)
                            {
                                this.RawCharBlock[CharBlockSize * (8 * dTileY + y) + 8 * dTileX + x] = 0;
                                continue;
                            }

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
