using System;
using System.Drawing;
using System.Windows.Forms;

using GBAEmulator.CPU;

namespace GBAEmulator
{
    public partial class Debug : Form
    {
        private GBA gba;

        public Debug(GBA gba)
        {
            InitializeComponent();

            this.gba = gba;
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

    }
}
