namespace GBAEmulator
{
    partial class Debug
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Tabs = new System.Windows.Forms.TabControl();
            this.PalettePage = new System.Windows.Forms.TabPage();
            this.OBJPaletteLabel = new System.Windows.Forms.Label();
            this.BGPaletteLabel = new System.Windows.Forms.Label();
            this.OBJPalette = new System.Windows.Forms.PictureBox();
            this.BGPalette = new System.Windows.Forms.PictureBox();
            this.Registers = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.DISPCNTLabel = new System.Windows.Forms.Label();
            this.BGModeLabel = new System.Windows.Forms.Label();
            this.DPFrameSelectLabel = new System.Windows.Forms.Label();
            this.HBlankIntervalFreeLabel = new System.Windows.Forms.Label();
            this.OBJVRAMMappingLabel = new System.Windows.Forms.Label();
            this.ForcedBlankLabel = new System.Windows.Forms.Label();
            this.Window0DisplayLabel = new System.Windows.Forms.Label();
            this.Window1DisplayLabel = new System.Windows.Forms.Label();
            this.OBJWindowDisplayLabel = new System.Windows.Forms.Label();
            this.BGMode = new System.Windows.Forms.Label();
            this.DPFrameSelect = new System.Windows.Forms.Label();
            this.HBlankIntervalFree = new System.Windows.Forms.Label();
            this.OBJVRAMMapping = new System.Windows.Forms.Label();
            this.ForcedBlank = new System.Windows.Forms.Label();
            this.Window0Display = new System.Windows.Forms.Label();
            this.Window1Display = new System.Windows.Forms.Label();
            this.OBJWindowDisplay = new System.Windows.Forms.Label();
            this.VCountSetting = new System.Windows.Forms.Label();
            this.VCountIRQEnable = new System.Windows.Forms.Label();
            this.HBlankIRQEnable = new System.Windows.Forms.Label();
            this.VBlankIRQEnable = new System.Windows.Forms.Label();
            this.VCounterFlag = new System.Windows.Forms.Label();
            this.HBlankFlag = new System.Windows.Forms.Label();
            this.VBlankFlag = new System.Windows.Forms.Label();
            this.VCountSettingLabel = new System.Windows.Forms.Label();
            this.VCountIRQEnableLabel = new System.Windows.Forms.Label();
            this.HBlankIRQEnableFlag = new System.Windows.Forms.Label();
            this.VBlankIRQEnableLabel = new System.Windows.Forms.Label();
            this.VCounterFlagLabel = new System.Windows.Forms.Label();
            this.HBlankFlagLabel = new System.Windows.Forms.Label();
            this.VBlankFlagLabel = new System.Windows.Forms.Label();
            this.DISPSTAT = new System.Windows.Forms.Label();
            this.VCountLabel = new System.Windows.Forms.Label();
            this.VCOUNT = new System.Windows.Forms.Label();
            this.KEYCNT = new System.Windows.Forms.Label();
            this.KEYCNTLabel = new System.Windows.Forms.Label();
            this.IME = new System.Windows.Forms.Label();
            this.IMELabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.IELabel = new System.Windows.Forms.Label();
            this.IFLabel = new System.Windows.Forms.Label();
            this.HALTCNT = new System.Windows.Forms.Label();
            this.HALTCNTLabel = new System.Windows.Forms.Label();
            this.IRQLabel = new System.Windows.Forms.Label();
            this.SWILabel = new System.Windows.Forms.Label();
            this.IEVBlankLabel = new System.Windows.Forms.Label();
            this.IEVBlank = new System.Windows.Forms.Label();
            this.IEHBlank = new System.Windows.Forms.Label();
            this.IEHBlankLabel = new System.Windows.Forms.Label();
            this.IEVCOUNT = new System.Windows.Forms.Label();
            this.IEVCountLabel = new System.Windows.Forms.Label();
            this.IETimers = new System.Windows.Forms.Label();
            this.IETimersLabel = new System.Windows.Forms.Label();
            this.IESIO = new System.Windows.Forms.Label();
            this.IESIOLabel = new System.Windows.Forms.Label();
            this.IEDMA = new System.Windows.Forms.Label();
            this.IEDMALabel = new System.Windows.Forms.Label();
            this.IEKeypad = new System.Windows.Forms.Label();
            this.IEKeypadLabel = new System.Windows.Forms.Label();
            this.IEGamePak = new System.Windows.Forms.Label();
            this.IEGamePakLabel = new System.Windows.Forms.Label();
            this.IFGamePak = new System.Windows.Forms.Label();
            this.IFKeypad = new System.Windows.Forms.Label();
            this.IFDMA = new System.Windows.Forms.Label();
            this.IFSIO = new System.Windows.Forms.Label();
            this.IFTimers = new System.Windows.Forms.Label();
            this.IFVCOUNT = new System.Windows.Forms.Label();
            this.IFHBlank = new System.Windows.Forms.Label();
            this.IFVBlank = new System.Windows.Forms.Label();
            this.Tabs.SuspendLayout();
            this.PalettePage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OBJPalette)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BGPalette)).BeginInit();
            this.Registers.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.Registers);
            this.Tabs.Controls.Add(this.PalettePage);
            this.Tabs.Controls.Add(this.tabPage1);
            this.Tabs.Location = new System.Drawing.Point(12, 12);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(796, 576);
            this.Tabs.TabIndex = 0;
            // 
            // PalettePage
            // 
            this.PalettePage.Controls.Add(this.OBJPaletteLabel);
            this.PalettePage.Controls.Add(this.BGPaletteLabel);
            this.PalettePage.Controls.Add(this.OBJPalette);
            this.PalettePage.Controls.Add(this.BGPalette);
            this.PalettePage.Location = new System.Drawing.Point(4, 22);
            this.PalettePage.Name = "PalettePage";
            this.PalettePage.Padding = new System.Windows.Forms.Padding(3);
            this.PalettePage.Size = new System.Drawing.Size(1154, 582);
            this.PalettePage.TabIndex = 0;
            this.PalettePage.Text = "Palette";
            this.PalettePage.UseVisualStyleBackColor = true;
            // 
            // OBJPaletteLabel
            // 
            this.OBJPaletteLabel.AutoSize = true;
            this.OBJPaletteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OBJPaletteLabel.Location = new System.Drawing.Point(277, 3);
            this.OBJPaletteLabel.Name = "OBJPaletteLabel";
            this.OBJPaletteLabel.Size = new System.Drawing.Size(94, 20);
            this.OBJPaletteLabel.TabIndex = 3;
            this.OBJPaletteLabel.Text = "OBJ Palette";
            // 
            // BGPaletteLabel
            // 
            this.BGPaletteLabel.AutoSize = true;
            this.BGPaletteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BGPaletteLabel.Location = new System.Drawing.Point(3, 3);
            this.BGPaletteLabel.Name = "BGPaletteLabel";
            this.BGPaletteLabel.Size = new System.Drawing.Size(149, 20);
            this.BGPaletteLabel.TabIndex = 2;
            this.BGPaletteLabel.Text = "Background Palette";
            // 
            // OBJPalette
            // 
            this.OBJPalette.Location = new System.Drawing.Point(281, 26);
            this.OBJPalette.Name = "OBJPalette";
            this.OBJPalette.Size = new System.Drawing.Size(272, 272);
            this.OBJPalette.TabIndex = 1;
            this.OBJPalette.TabStop = false;
            // 
            // BGPalette
            // 
            this.BGPalette.Location = new System.Drawing.Point(3, 26);
            this.BGPalette.Name = "BGPalette";
            this.BGPalette.Size = new System.Drawing.Size(272, 272);
            this.BGPalette.TabIndex = 0;
            this.BGPalette.TabStop = false;
            // 
            // Registers
            // 
            this.Registers.Controls.Add(this.IFGamePak);
            this.Registers.Controls.Add(this.IFKeypad);
            this.Registers.Controls.Add(this.IFDMA);
            this.Registers.Controls.Add(this.IFSIO);
            this.Registers.Controls.Add(this.IFTimers);
            this.Registers.Controls.Add(this.IFVCOUNT);
            this.Registers.Controls.Add(this.IFHBlank);
            this.Registers.Controls.Add(this.IFVBlank);
            this.Registers.Controls.Add(this.IEGamePak);
            this.Registers.Controls.Add(this.IEGamePakLabel);
            this.Registers.Controls.Add(this.IEKeypad);
            this.Registers.Controls.Add(this.IEKeypadLabel);
            this.Registers.Controls.Add(this.IEDMA);
            this.Registers.Controls.Add(this.IEDMALabel);
            this.Registers.Controls.Add(this.IESIO);
            this.Registers.Controls.Add(this.IESIOLabel);
            this.Registers.Controls.Add(this.IETimers);
            this.Registers.Controls.Add(this.IETimersLabel);
            this.Registers.Controls.Add(this.IEVCOUNT);
            this.Registers.Controls.Add(this.IEVCountLabel);
            this.Registers.Controls.Add(this.IEHBlank);
            this.Registers.Controls.Add(this.IEHBlankLabel);
            this.Registers.Controls.Add(this.IEVBlank);
            this.Registers.Controls.Add(this.IEVBlankLabel);
            this.Registers.Controls.Add(this.SWILabel);
            this.Registers.Controls.Add(this.IRQLabel);
            this.Registers.Controls.Add(this.HALTCNT);
            this.Registers.Controls.Add(this.HALTCNTLabel);
            this.Registers.Controls.Add(this.IFLabel);
            this.Registers.Controls.Add(this.IELabel);
            this.Registers.Controls.Add(this.label3);
            this.Registers.Controls.Add(this.label2);
            this.Registers.Controls.Add(this.IME);
            this.Registers.Controls.Add(this.IMELabel);
            this.Registers.Controls.Add(this.KEYCNT);
            this.Registers.Controls.Add(this.KEYCNTLabel);
            this.Registers.Controls.Add(this.VCOUNT);
            this.Registers.Controls.Add(this.VCountLabel);
            this.Registers.Controls.Add(this.VCountSetting);
            this.Registers.Controls.Add(this.VCountIRQEnable);
            this.Registers.Controls.Add(this.HBlankIRQEnable);
            this.Registers.Controls.Add(this.VBlankIRQEnable);
            this.Registers.Controls.Add(this.VCounterFlag);
            this.Registers.Controls.Add(this.HBlankFlag);
            this.Registers.Controls.Add(this.VBlankFlag);
            this.Registers.Controls.Add(this.VCountSettingLabel);
            this.Registers.Controls.Add(this.VCountIRQEnableLabel);
            this.Registers.Controls.Add(this.HBlankIRQEnableFlag);
            this.Registers.Controls.Add(this.VBlankIRQEnableLabel);
            this.Registers.Controls.Add(this.VCounterFlagLabel);
            this.Registers.Controls.Add(this.HBlankFlagLabel);
            this.Registers.Controls.Add(this.VBlankFlagLabel);
            this.Registers.Controls.Add(this.DISPSTAT);
            this.Registers.Controls.Add(this.OBJWindowDisplay);
            this.Registers.Controls.Add(this.Window1Display);
            this.Registers.Controls.Add(this.Window0Display);
            this.Registers.Controls.Add(this.ForcedBlank);
            this.Registers.Controls.Add(this.OBJVRAMMapping);
            this.Registers.Controls.Add(this.HBlankIntervalFree);
            this.Registers.Controls.Add(this.DPFrameSelect);
            this.Registers.Controls.Add(this.BGMode);
            this.Registers.Controls.Add(this.OBJWindowDisplayLabel);
            this.Registers.Controls.Add(this.Window1DisplayLabel);
            this.Registers.Controls.Add(this.Window0DisplayLabel);
            this.Registers.Controls.Add(this.ForcedBlankLabel);
            this.Registers.Controls.Add(this.OBJVRAMMappingLabel);
            this.Registers.Controls.Add(this.HBlankIntervalFreeLabel);
            this.Registers.Controls.Add(this.DPFrameSelectLabel);
            this.Registers.Controls.Add(this.BGModeLabel);
            this.Registers.Controls.Add(this.DISPCNTLabel);
            this.Registers.Location = new System.Drawing.Point(4, 22);
            this.Registers.Name = "Registers";
            this.Registers.Padding = new System.Windows.Forms.Padding(3);
            this.Registers.Size = new System.Drawing.Size(788, 550);
            this.Registers.TabIndex = 1;
            this.Registers.Text = "Registers";
            this.Registers.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1154, 582);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // DISPCNTLabel
            // 
            this.DISPCNTLabel.AutoSize = true;
            this.DISPCNTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DISPCNTLabel.Location = new System.Drawing.Point(6, 51);
            this.DISPCNTLabel.Name = "DISPCNTLabel";
            this.DISPCNTLabel.Size = new System.Drawing.Size(85, 20);
            this.DISPCNTLabel.TabIndex = 0;
            this.DISPCNTLabel.Text = "DISPCNT";
            // 
            // BGModeLabel
            // 
            this.BGModeLabel.AutoSize = true;
            this.BGModeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BGModeLabel.Location = new System.Drawing.Point(6, 78);
            this.BGModeLabel.Name = "BGModeLabel";
            this.BGModeLabel.Size = new System.Drawing.Size(73, 20);
            this.BGModeLabel.TabIndex = 1;
            this.BGModeLabel.Text = "BGMode";
            // 
            // DPFrameSelectLabel
            // 
            this.DPFrameSelectLabel.AutoSize = true;
            this.DPFrameSelectLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DPFrameSelectLabel.Location = new System.Drawing.Point(6, 98);
            this.DPFrameSelectLabel.Name = "DPFrameSelectLabel";
            this.DPFrameSelectLabel.Size = new System.Drawing.Size(159, 20);
            this.DPFrameSelectLabel.TabIndex = 2;
            this.DPFrameSelectLabel.Text = "Display Frame Select";
            // 
            // HBlankIntervalFreeLabel
            // 
            this.HBlankIntervalFreeLabel.AutoSize = true;
            this.HBlankIntervalFreeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HBlankIntervalFreeLabel.Location = new System.Drawing.Point(6, 118);
            this.HBlankIntervalFreeLabel.Name = "HBlankIntervalFreeLabel";
            this.HBlankIntervalFreeLabel.Size = new System.Drawing.Size(154, 20);
            this.HBlankIntervalFreeLabel.TabIndex = 3;
            this.HBlankIntervalFreeLabel.Text = "HBlank Interval Free";
            // 
            // OBJVRAMMappingLabel
            // 
            this.OBJVRAMMappingLabel.AutoSize = true;
            this.OBJVRAMMappingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OBJVRAMMappingLabel.Location = new System.Drawing.Point(6, 138);
            this.OBJVRAMMappingLabel.Name = "OBJVRAMMappingLabel";
            this.OBJVRAMMappingLabel.Size = new System.Drawing.Size(156, 20);
            this.OBJVRAMMappingLabel.TabIndex = 4;
            this.OBJVRAMMappingLabel.Text = "OBJ VRAM Mapping";
            // 
            // ForcedBlankLabel
            // 
            this.ForcedBlankLabel.AutoSize = true;
            this.ForcedBlankLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForcedBlankLabel.Location = new System.Drawing.Point(6, 158);
            this.ForcedBlankLabel.Name = "ForcedBlankLabel";
            this.ForcedBlankLabel.Size = new System.Drawing.Size(99, 20);
            this.ForcedBlankLabel.TabIndex = 5;
            this.ForcedBlankLabel.Text = "ForcedBlank";
            // 
            // Window0DisplayLabel
            // 
            this.Window0DisplayLabel.AutoSize = true;
            this.Window0DisplayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Window0DisplayLabel.Location = new System.Drawing.Point(6, 178);
            this.Window0DisplayLabel.Name = "Window0DisplayLabel";
            this.Window0DisplayLabel.Size = new System.Drawing.Size(133, 20);
            this.Window0DisplayLabel.TabIndex = 6;
            this.Window0DisplayLabel.Text = "Window 0 Display";
            // 
            // Window1DisplayLabel
            // 
            this.Window1DisplayLabel.AutoSize = true;
            this.Window1DisplayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Window1DisplayLabel.Location = new System.Drawing.Point(6, 198);
            this.Window1DisplayLabel.Name = "Window1DisplayLabel";
            this.Window1DisplayLabel.Size = new System.Drawing.Size(133, 20);
            this.Window1DisplayLabel.TabIndex = 7;
            this.Window1DisplayLabel.Text = "Window 1 Display";
            // 
            // OBJWindowDisplayLabel
            // 
            this.OBJWindowDisplayLabel.AutoSize = true;
            this.OBJWindowDisplayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OBJWindowDisplayLabel.Location = new System.Drawing.Point(6, 218);
            this.OBJWindowDisplayLabel.Name = "OBJWindowDisplayLabel";
            this.OBJWindowDisplayLabel.Size = new System.Drawing.Size(155, 20);
            this.OBJWindowDisplayLabel.TabIndex = 8;
            this.OBJWindowDisplayLabel.Text = "OBJ Window Display";
            // 
            // BGMode
            // 
            this.BGMode.AutoSize = true;
            this.BGMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BGMode.Location = new System.Drawing.Point(175, 78);
            this.BGMode.Name = "BGMode";
            this.BGMode.Size = new System.Drawing.Size(18, 20);
            this.BGMode.TabIndex = 9;
            this.BGMode.Text = "0";
            // 
            // DPFrameSelect
            // 
            this.DPFrameSelect.AutoSize = true;
            this.DPFrameSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DPFrameSelect.Location = new System.Drawing.Point(175, 98);
            this.DPFrameSelect.Name = "DPFrameSelect";
            this.DPFrameSelect.Size = new System.Drawing.Size(18, 20);
            this.DPFrameSelect.TabIndex = 10;
            this.DPFrameSelect.Text = "0";
            // 
            // HBlankIntervalFree
            // 
            this.HBlankIntervalFree.AutoSize = true;
            this.HBlankIntervalFree.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HBlankIntervalFree.Location = new System.Drawing.Point(175, 118);
            this.HBlankIntervalFree.Name = "HBlankIntervalFree";
            this.HBlankIntervalFree.Size = new System.Drawing.Size(18, 20);
            this.HBlankIntervalFree.TabIndex = 11;
            this.HBlankIntervalFree.Text = "0";
            // 
            // OBJVRAMMapping
            // 
            this.OBJVRAMMapping.AutoSize = true;
            this.OBJVRAMMapping.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OBJVRAMMapping.Location = new System.Drawing.Point(175, 138);
            this.OBJVRAMMapping.Name = "OBJVRAMMapping";
            this.OBJVRAMMapping.Size = new System.Drawing.Size(18, 20);
            this.OBJVRAMMapping.TabIndex = 12;
            this.OBJVRAMMapping.Text = "0";
            // 
            // ForcedBlank
            // 
            this.ForcedBlank.AutoSize = true;
            this.ForcedBlank.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForcedBlank.Location = new System.Drawing.Point(175, 158);
            this.ForcedBlank.Name = "ForcedBlank";
            this.ForcedBlank.Size = new System.Drawing.Size(18, 20);
            this.ForcedBlank.TabIndex = 13;
            this.ForcedBlank.Text = "0";
            // 
            // Window0Display
            // 
            this.Window0Display.AutoSize = true;
            this.Window0Display.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Window0Display.Location = new System.Drawing.Point(175, 178);
            this.Window0Display.Name = "Window0Display";
            this.Window0Display.Size = new System.Drawing.Size(18, 20);
            this.Window0Display.TabIndex = 14;
            this.Window0Display.Text = "0";
            // 
            // Window1Display
            // 
            this.Window1Display.AutoSize = true;
            this.Window1Display.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Window1Display.Location = new System.Drawing.Point(175, 198);
            this.Window1Display.Name = "Window1Display";
            this.Window1Display.Size = new System.Drawing.Size(18, 20);
            this.Window1Display.TabIndex = 15;
            this.Window1Display.Text = "0";
            // 
            // OBJWindowDisplay
            // 
            this.OBJWindowDisplay.AutoSize = true;
            this.OBJWindowDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OBJWindowDisplay.Location = new System.Drawing.Point(175, 218);
            this.OBJWindowDisplay.Name = "OBJWindowDisplay";
            this.OBJWindowDisplay.Size = new System.Drawing.Size(18, 20);
            this.OBJWindowDisplay.TabIndex = 16;
            this.OBJWindowDisplay.Text = "0";
            // 
            // VCountSetting
            // 
            this.VCountSetting.AutoSize = true;
            this.VCountSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCountSetting.Location = new System.Drawing.Point(175, 404);
            this.VCountSetting.Name = "VCountSetting";
            this.VCountSetting.Size = new System.Drawing.Size(36, 20);
            this.VCountSetting.TabIndex = 33;
            this.VCountSetting.Text = "000";
            // 
            // VCountIRQEnable
            // 
            this.VCountIRQEnable.AutoSize = true;
            this.VCountIRQEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCountIRQEnable.Location = new System.Drawing.Point(175, 380);
            this.VCountIRQEnable.Name = "VCountIRQEnable";
            this.VCountIRQEnable.Size = new System.Drawing.Size(18, 20);
            this.VCountIRQEnable.TabIndex = 31;
            this.VCountIRQEnable.Text = "0";
            // 
            // HBlankIRQEnable
            // 
            this.HBlankIRQEnable.AutoSize = true;
            this.HBlankIRQEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HBlankIRQEnable.Location = new System.Drawing.Point(175, 360);
            this.HBlankIRQEnable.Name = "HBlankIRQEnable";
            this.HBlankIRQEnable.Size = new System.Drawing.Size(18, 20);
            this.HBlankIRQEnable.TabIndex = 30;
            this.HBlankIRQEnable.Text = "0";
            // 
            // VBlankIRQEnable
            // 
            this.VBlankIRQEnable.AutoSize = true;
            this.VBlankIRQEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VBlankIRQEnable.Location = new System.Drawing.Point(175, 340);
            this.VBlankIRQEnable.Name = "VBlankIRQEnable";
            this.VBlankIRQEnable.Size = new System.Drawing.Size(18, 20);
            this.VBlankIRQEnable.TabIndex = 29;
            this.VBlankIRQEnable.Text = "0";
            // 
            // VCounterFlag
            // 
            this.VCounterFlag.AutoSize = true;
            this.VCounterFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCounterFlag.Location = new System.Drawing.Point(175, 320);
            this.VCounterFlag.Name = "VCounterFlag";
            this.VCounterFlag.Size = new System.Drawing.Size(18, 20);
            this.VCounterFlag.TabIndex = 28;
            this.VCounterFlag.Text = "0";
            // 
            // HBlankFlag
            // 
            this.HBlankFlag.AutoSize = true;
            this.HBlankFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HBlankFlag.Location = new System.Drawing.Point(175, 300);
            this.HBlankFlag.Name = "HBlankFlag";
            this.HBlankFlag.Size = new System.Drawing.Size(18, 20);
            this.HBlankFlag.TabIndex = 27;
            this.HBlankFlag.Text = "0";
            // 
            // VBlankFlag
            // 
            this.VBlankFlag.AutoSize = true;
            this.VBlankFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VBlankFlag.Location = new System.Drawing.Point(175, 280);
            this.VBlankFlag.Name = "VBlankFlag";
            this.VBlankFlag.Size = new System.Drawing.Size(18, 20);
            this.VBlankFlag.TabIndex = 26;
            this.VBlankFlag.Text = "0";
            // 
            // VCountSettingLabel
            // 
            this.VCountSettingLabel.AutoSize = true;
            this.VCountSettingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCountSettingLabel.Location = new System.Drawing.Point(6, 404);
            this.VCountSettingLabel.Name = "VCountSettingLabel";
            this.VCountSettingLabel.Size = new System.Drawing.Size(118, 20);
            this.VCountSettingLabel.TabIndex = 25;
            this.VCountSettingLabel.Text = "VCount Setting";
            // 
            // VCountIRQEnableLabel
            // 
            this.VCountIRQEnableLabel.AutoSize = true;
            this.VCountIRQEnableLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCountIRQEnableLabel.Location = new System.Drawing.Point(6, 380);
            this.VCountIRQEnableLabel.Name = "VCountIRQEnableLabel";
            this.VCountIRQEnableLabel.Size = new System.Drawing.Size(150, 20);
            this.VCountIRQEnableLabel.TabIndex = 23;
            this.VCountIRQEnableLabel.Text = "VCount IRQ Enable";
            // 
            // HBlankIRQEnableFlag
            // 
            this.HBlankIRQEnableFlag.AutoSize = true;
            this.HBlankIRQEnableFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HBlankIRQEnableFlag.Location = new System.Drawing.Point(6, 360);
            this.HBlankIRQEnableFlag.Name = "HBlankIRQEnableFlag";
            this.HBlankIRQEnableFlag.Size = new System.Drawing.Size(148, 20);
            this.HBlankIRQEnableFlag.TabIndex = 22;
            this.HBlankIRQEnableFlag.Text = "HBlank IRQ Enable";
            // 
            // VBlankIRQEnableLabel
            // 
            this.VBlankIRQEnableLabel.AutoSize = true;
            this.VBlankIRQEnableLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VBlankIRQEnableLabel.Location = new System.Drawing.Point(6, 340);
            this.VBlankIRQEnableLabel.Name = "VBlankIRQEnableLabel";
            this.VBlankIRQEnableLabel.Size = new System.Drawing.Size(147, 20);
            this.VBlankIRQEnableLabel.TabIndex = 21;
            this.VBlankIRQEnableLabel.Text = "VBlank IRQ Enable";
            // 
            // VCounterFlagLabel
            // 
            this.VCounterFlagLabel.AutoSize = true;
            this.VCounterFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCounterFlagLabel.Location = new System.Drawing.Point(6, 320);
            this.VCounterFlagLabel.Name = "VCounterFlagLabel";
            this.VCounterFlagLabel.Size = new System.Drawing.Size(77, 20);
            this.VCounterFlagLabel.TabIndex = 20;
            this.VCounterFlagLabel.Text = "VCounter";
            // 
            // HBlankFlagLabel
            // 
            this.HBlankFlagLabel.AutoSize = true;
            this.HBlankFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HBlankFlagLabel.Location = new System.Drawing.Point(6, 300);
            this.HBlankFlagLabel.Name = "HBlankFlagLabel";
            this.HBlankFlagLabel.Size = new System.Drawing.Size(96, 20);
            this.HBlankFlagLabel.TabIndex = 19;
            this.HBlankFlagLabel.Text = "HBlank Flag";
            // 
            // VBlankFlagLabel
            // 
            this.VBlankFlagLabel.AutoSize = true;
            this.VBlankFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VBlankFlagLabel.Location = new System.Drawing.Point(6, 280);
            this.VBlankFlagLabel.Name = "VBlankFlagLabel";
            this.VBlankFlagLabel.Size = new System.Drawing.Size(95, 20);
            this.VBlankFlagLabel.TabIndex = 18;
            this.VBlankFlagLabel.Text = "VBlank Flag";
            // 
            // DISPSTAT
            // 
            this.DISPSTAT.AutoSize = true;
            this.DISPSTAT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DISPSTAT.Location = new System.Drawing.Point(6, 253);
            this.DISPSTAT.Name = "DISPSTAT";
            this.DISPSTAT.Size = new System.Drawing.Size(95, 20);
            this.DISPSTAT.TabIndex = 17;
            this.DISPSTAT.Text = "DISPSTAT";
            // 
            // VCountLabel
            // 
            this.VCountLabel.AutoSize = true;
            this.VCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCountLabel.Location = new System.Drawing.Point(6, 437);
            this.VCountLabel.Name = "VCountLabel";
            this.VCountLabel.Size = new System.Drawing.Size(81, 20);
            this.VCountLabel.TabIndex = 34;
            this.VCountLabel.Text = "VCOUNT";
            // 
            // VCOUNT
            // 
            this.VCOUNT.AutoSize = true;
            this.VCOUNT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VCOUNT.Location = new System.Drawing.Point(175, 437);
            this.VCOUNT.Name = "VCOUNT";
            this.VCOUNT.Size = new System.Drawing.Size(36, 20);
            this.VCOUNT.TabIndex = 35;
            this.VCOUNT.Text = "000";
            // 
            // KEYCNT
            // 
            this.KEYCNT.AutoSize = true;
            this.KEYCNT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KEYCNT.Location = new System.Drawing.Point(399, 78);
            this.KEYCNT.Name = "KEYCNT";
            this.KEYCNT.Size = new System.Drawing.Size(45, 20);
            this.KEYCNT.TabIndex = 45;
            this.KEYCNT.Text = "0000";
            // 
            // KEYCNTLabel
            // 
            this.KEYCNTLabel.AutoSize = true;
            this.KEYCNTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KEYCNTLabel.Location = new System.Drawing.Point(243, 78);
            this.KEYCNTLabel.Name = "KEYCNTLabel";
            this.KEYCNTLabel.Size = new System.Drawing.Size(78, 20);
            this.KEYCNTLabel.TabIndex = 36;
            this.KEYCNTLabel.Text = "KEYCNT";
            // 
            // IME
            // 
            this.IME.AutoSize = true;
            this.IME.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IME.Location = new System.Drawing.Point(399, 98);
            this.IME.Name = "IME";
            this.IME.Size = new System.Drawing.Size(45, 20);
            this.IME.TabIndex = 47;
            this.IME.Text = "0000";
            // 
            // IMELabel
            // 
            this.IMELabel.AutoSize = true;
            this.IMELabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IMELabel.Location = new System.Drawing.Point(243, 98);
            this.IMELabel.Name = "IMELabel";
            this.IMELabel.Size = new System.Drawing.Size(41, 20);
            this.IMELabel.TabIndex = 46;
            this.IMELabel.Text = "IME";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 20);
            this.label2.TabIndex = 48;
            this.label2.Text = "LCD Control";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(243, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 20);
            this.label3.TabIndex = 49;
            this.label3.Text = "Interrupt Control";
            // 
            // IELabel
            // 
            this.IELabel.AutoSize = true;
            this.IELabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IELabel.Location = new System.Drawing.Point(385, 118);
            this.IELabel.Name = "IELabel";
            this.IELabel.Size = new System.Drawing.Size(27, 20);
            this.IELabel.TabIndex = 50;
            this.IELabel.Text = "IE";
            // 
            // IFLabel
            // 
            this.IFLabel.AutoSize = true;
            this.IFLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFLabel.Location = new System.Drawing.Point(418, 118);
            this.IFLabel.Name = "IFLabel";
            this.IFLabel.Size = new System.Drawing.Size(26, 20);
            this.IFLabel.TabIndex = 52;
            this.IFLabel.Text = "IF";
            // 
            // HALTCNT
            // 
            this.HALTCNT.AutoSize = true;
            this.HALTCNT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HALTCNT.Location = new System.Drawing.Point(394, 320);
            this.HALTCNT.Name = "HALTCNT";
            this.HALTCNT.Size = new System.Drawing.Size(18, 20);
            this.HALTCNT.TabIndex = 55;
            this.HALTCNT.Text = "0";
            // 
            // HALTCNTLabel
            // 
            this.HALTCNTLabel.AutoSize = true;
            this.HALTCNTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HALTCNTLabel.Location = new System.Drawing.Point(243, 320);
            this.HALTCNTLabel.Name = "HALTCNTLabel";
            this.HALTCNTLabel.Size = new System.Drawing.Size(88, 20);
            this.HALTCNTLabel.TabIndex = 54;
            this.HALTCNTLabel.Text = "HALTCNT";
            // 
            // IRQLabel
            // 
            this.IRQLabel.AutoSize = true;
            this.IRQLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IRQLabel.ForeColor = System.Drawing.Color.Red;
            this.IRQLabel.Location = new System.Drawing.Point(243, 51);
            this.IRQLabel.Name = "IRQLabel";
            this.IRQLabel.Size = new System.Drawing.Size(41, 20);
            this.IRQLabel.TabIndex = 56;
            this.IRQLabel.Text = "IRQ";
            // 
            // SWILabel
            // 
            this.SWILabel.AutoSize = true;
            this.SWILabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SWILabel.ForeColor = System.Drawing.Color.Red;
            this.SWILabel.Location = new System.Drawing.Point(401, 51);
            this.SWILabel.Name = "SWILabel";
            this.SWILabel.Size = new System.Drawing.Size(43, 20);
            this.SWILabel.TabIndex = 57;
            this.SWILabel.Text = "SWI";
            // 
            // IEVBlankLabel
            // 
            this.IEVBlankLabel.AutoSize = true;
            this.IEVBlankLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEVBlankLabel.Location = new System.Drawing.Point(243, 138);
            this.IEVBlankLabel.Name = "IEVBlankLabel";
            this.IEVBlankLabel.Size = new System.Drawing.Size(60, 20);
            this.IEVBlankLabel.TabIndex = 58;
            this.IEVBlankLabel.Text = "VBlank";
            // 
            // IEVBlank
            // 
            this.IEVBlank.AutoSize = true;
            this.IEVBlank.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEVBlank.Location = new System.Drawing.Point(394, 138);
            this.IEVBlank.Name = "IEVBlank";
            this.IEVBlank.Size = new System.Drawing.Size(18, 20);
            this.IEVBlank.TabIndex = 59;
            this.IEVBlank.Text = "0";
            // 
            // IEHBlank
            // 
            this.IEHBlank.AutoSize = true;
            this.IEHBlank.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEHBlank.Location = new System.Drawing.Point(394, 158);
            this.IEHBlank.Name = "IEHBlank";
            this.IEHBlank.Size = new System.Drawing.Size(18, 20);
            this.IEHBlank.TabIndex = 61;
            this.IEHBlank.Text = "0";
            // 
            // IEHBlankLabel
            // 
            this.IEHBlankLabel.AutoSize = true;
            this.IEHBlankLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEHBlankLabel.Location = new System.Drawing.Point(243, 158);
            this.IEHBlankLabel.Name = "IEHBlankLabel";
            this.IEHBlankLabel.Size = new System.Drawing.Size(61, 20);
            this.IEHBlankLabel.TabIndex = 60;
            this.IEHBlankLabel.Text = "HBlank";
            // 
            // IEVCOUNT
            // 
            this.IEVCOUNT.AutoSize = true;
            this.IEVCOUNT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEVCOUNT.Location = new System.Drawing.Point(394, 178);
            this.IEVCOUNT.Name = "IEVCOUNT";
            this.IEVCOUNT.Size = new System.Drawing.Size(18, 20);
            this.IEVCOUNT.TabIndex = 63;
            this.IEVCOUNT.Text = "0";
            // 
            // IEVCountLabel
            // 
            this.IEVCountLabel.AutoSize = true;
            this.IEVCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEVCountLabel.Location = new System.Drawing.Point(243, 178);
            this.IEVCountLabel.Name = "IEVCountLabel";
            this.IEVCountLabel.Size = new System.Drawing.Size(123, 20);
            this.IEVCountLabel.TabIndex = 62;
            this.IEVCountLabel.Text = "VCOUNT Match";
            // 
            // IETimers
            // 
            this.IETimers.AutoSize = true;
            this.IETimers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IETimers.Location = new System.Drawing.Point(394, 198);
            this.IETimers.Name = "IETimers";
            this.IETimers.Size = new System.Drawing.Size(18, 20);
            this.IETimers.TabIndex = 65;
            this.IETimers.Text = "0";
            // 
            // IETimersLabel
            // 
            this.IETimersLabel.AutoSize = true;
            this.IETimersLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IETimersLabel.Location = new System.Drawing.Point(243, 198);
            this.IETimersLabel.Name = "IETimersLabel";
            this.IETimersLabel.Size = new System.Drawing.Size(56, 20);
            this.IETimersLabel.TabIndex = 64;
            this.IETimersLabel.Text = "Timers";
            // 
            // IESIO
            // 
            this.IESIO.AutoSize = true;
            this.IESIO.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IESIO.Location = new System.Drawing.Point(394, 218);
            this.IESIO.Name = "IESIO";
            this.IESIO.Size = new System.Drawing.Size(18, 20);
            this.IESIO.TabIndex = 67;
            this.IESIO.Text = "0";
            // 
            // IESIOLabel
            // 
            this.IESIOLabel.AutoSize = true;
            this.IESIOLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IESIOLabel.Location = new System.Drawing.Point(243, 218);
            this.IESIOLabel.Name = "IESIOLabel";
            this.IESIOLabel.Size = new System.Drawing.Size(37, 20);
            this.IESIOLabel.TabIndex = 66;
            this.IESIOLabel.Text = "SIO";
            // 
            // IEDMA
            // 
            this.IEDMA.AutoSize = true;
            this.IEDMA.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEDMA.Location = new System.Drawing.Point(394, 238);
            this.IEDMA.Name = "IEDMA";
            this.IEDMA.Size = new System.Drawing.Size(18, 20);
            this.IEDMA.TabIndex = 69;
            this.IEDMA.Text = "0";
            // 
            // IEDMALabel
            // 
            this.IEDMALabel.AutoSize = true;
            this.IEDMALabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEDMALabel.Location = new System.Drawing.Point(243, 238);
            this.IEDMALabel.Name = "IEDMALabel";
            this.IEDMALabel.Size = new System.Drawing.Size(45, 20);
            this.IEDMALabel.TabIndex = 68;
            this.IEDMALabel.Text = "DMA";
            // 
            // IEKeypad
            // 
            this.IEKeypad.AutoSize = true;
            this.IEKeypad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEKeypad.Location = new System.Drawing.Point(394, 258);
            this.IEKeypad.Name = "IEKeypad";
            this.IEKeypad.Size = new System.Drawing.Size(18, 20);
            this.IEKeypad.TabIndex = 71;
            this.IEKeypad.Text = "0";
            // 
            // IEKeypadLabel
            // 
            this.IEKeypadLabel.AutoSize = true;
            this.IEKeypadLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEKeypadLabel.Location = new System.Drawing.Point(243, 258);
            this.IEKeypadLabel.Name = "IEKeypadLabel";
            this.IEKeypadLabel.Size = new System.Drawing.Size(62, 20);
            this.IEKeypadLabel.TabIndex = 70;
            this.IEKeypadLabel.Text = "Keypad";
            // 
            // IEGamePak
            // 
            this.IEGamePak.AutoSize = true;
            this.IEGamePak.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEGamePak.Location = new System.Drawing.Point(394, 278);
            this.IEGamePak.Name = "IEGamePak";
            this.IEGamePak.Size = new System.Drawing.Size(18, 20);
            this.IEGamePak.TabIndex = 73;
            this.IEGamePak.Text = "0";
            // 
            // IEGamePakLabel
            // 
            this.IEGamePakLabel.AutoSize = true;
            this.IEGamePakLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IEGamePakLabel.Location = new System.Drawing.Point(243, 278);
            this.IEGamePakLabel.Name = "IEGamePakLabel";
            this.IEGamePakLabel.Size = new System.Drawing.Size(80, 20);
            this.IEGamePakLabel.TabIndex = 72;
            this.IEGamePakLabel.Text = "GamePak";
            // 
            // IFGamePak
            // 
            this.IFGamePak.AutoSize = true;
            this.IFGamePak.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFGamePak.Location = new System.Drawing.Point(426, 278);
            this.IFGamePak.Name = "IFGamePak";
            this.IFGamePak.Size = new System.Drawing.Size(18, 20);
            this.IFGamePak.TabIndex = 89;
            this.IFGamePak.Text = "0";
            // 
            // IFKeypad
            // 
            this.IFKeypad.AutoSize = true;
            this.IFKeypad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFKeypad.Location = new System.Drawing.Point(426, 258);
            this.IFKeypad.Name = "IFKeypad";
            this.IFKeypad.Size = new System.Drawing.Size(18, 20);
            this.IFKeypad.TabIndex = 87;
            this.IFKeypad.Text = "0";
            // 
            // IFDMA
            // 
            this.IFDMA.AutoSize = true;
            this.IFDMA.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFDMA.Location = new System.Drawing.Point(426, 238);
            this.IFDMA.Name = "IFDMA";
            this.IFDMA.Size = new System.Drawing.Size(18, 20);
            this.IFDMA.TabIndex = 85;
            this.IFDMA.Text = "0";
            // 
            // IFSIO
            // 
            this.IFSIO.AutoSize = true;
            this.IFSIO.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFSIO.Location = new System.Drawing.Point(426, 218);
            this.IFSIO.Name = "IFSIO";
            this.IFSIO.Size = new System.Drawing.Size(18, 20);
            this.IFSIO.TabIndex = 83;
            this.IFSIO.Text = "0";
            // 
            // IFTimers
            // 
            this.IFTimers.AutoSize = true;
            this.IFTimers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFTimers.Location = new System.Drawing.Point(426, 198);
            this.IFTimers.Name = "IFTimers";
            this.IFTimers.Size = new System.Drawing.Size(18, 20);
            this.IFTimers.TabIndex = 81;
            this.IFTimers.Text = "0";
            // 
            // IFVCOUNT
            // 
            this.IFVCOUNT.AutoSize = true;
            this.IFVCOUNT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFVCOUNT.Location = new System.Drawing.Point(426, 178);
            this.IFVCOUNT.Name = "IFVCOUNT";
            this.IFVCOUNT.Size = new System.Drawing.Size(18, 20);
            this.IFVCOUNT.TabIndex = 79;
            this.IFVCOUNT.Text = "0";
            // 
            // IFHBlank
            // 
            this.IFHBlank.AutoSize = true;
            this.IFHBlank.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFHBlank.Location = new System.Drawing.Point(426, 158);
            this.IFHBlank.Name = "IFHBlank";
            this.IFHBlank.Size = new System.Drawing.Size(18, 20);
            this.IFHBlank.TabIndex = 77;
            this.IFHBlank.Text = "0";
            // 
            // IFVBlank
            // 
            this.IFVBlank.AutoSize = true;
            this.IFVBlank.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IFVBlank.Location = new System.Drawing.Point(426, 138);
            this.IFVBlank.Name = "IFVBlank";
            this.IFVBlank.Size = new System.Drawing.Size(18, 20);
            this.IFVBlank.TabIndex = 75;
            this.IFVBlank.Text = "0";
            // 
            // Debug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 600);
            this.Controls.Add(this.Tabs);
            this.Name = "Debug";
            this.Text = "Debug";
            this.Tabs.ResumeLayout(false);
            this.PalettePage.ResumeLayout(false);
            this.PalettePage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OBJPalette)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BGPalette)).EndInit();
            this.Registers.ResumeLayout(false);
            this.Registers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage PalettePage;
        private System.Windows.Forms.Label OBJPaletteLabel;
        private System.Windows.Forms.Label BGPaletteLabel;
        private System.Windows.Forms.PictureBox OBJPalette;
        private System.Windows.Forms.PictureBox BGPalette;
        private System.Windows.Forms.TabPage Registers;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label Window1DisplayLabel;
        private System.Windows.Forms.Label Window0DisplayLabel;
        private System.Windows.Forms.Label ForcedBlankLabel;
        private System.Windows.Forms.Label OBJVRAMMappingLabel;
        private System.Windows.Forms.Label HBlankIntervalFreeLabel;
        private System.Windows.Forms.Label DPFrameSelectLabel;
        private System.Windows.Forms.Label BGModeLabel;
        private System.Windows.Forms.Label DISPCNTLabel;
        private System.Windows.Forms.Label SWILabel;
        private System.Windows.Forms.Label IRQLabel;
        private System.Windows.Forms.Label HALTCNT;
        private System.Windows.Forms.Label HALTCNTLabel;
        private System.Windows.Forms.Label IFLabel;
        private System.Windows.Forms.Label IELabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label IME;
        private System.Windows.Forms.Label IMELabel;
        private System.Windows.Forms.Label KEYCNT;
        private System.Windows.Forms.Label KEYCNTLabel;
        private System.Windows.Forms.Label VCOUNT;
        private System.Windows.Forms.Label VCountLabel;
        private System.Windows.Forms.Label VCountSetting;
        private System.Windows.Forms.Label VCountIRQEnable;
        private System.Windows.Forms.Label HBlankIRQEnable;
        private System.Windows.Forms.Label VBlankIRQEnable;
        private System.Windows.Forms.Label VCounterFlag;
        private System.Windows.Forms.Label HBlankFlag;
        private System.Windows.Forms.Label VBlankFlag;
        private System.Windows.Forms.Label VCountSettingLabel;
        private System.Windows.Forms.Label VCountIRQEnableLabel;
        private System.Windows.Forms.Label HBlankIRQEnableFlag;
        private System.Windows.Forms.Label VBlankIRQEnableLabel;
        private System.Windows.Forms.Label VCounterFlagLabel;
        private System.Windows.Forms.Label HBlankFlagLabel;
        private System.Windows.Forms.Label VBlankFlagLabel;
        private System.Windows.Forms.Label DISPSTAT;
        private System.Windows.Forms.Label OBJWindowDisplay;
        private System.Windows.Forms.Label Window1Display;
        private System.Windows.Forms.Label Window0Display;
        private System.Windows.Forms.Label ForcedBlank;
        private System.Windows.Forms.Label OBJVRAMMapping;
        private System.Windows.Forms.Label HBlankIntervalFree;
        private System.Windows.Forms.Label DPFrameSelect;
        private System.Windows.Forms.Label BGMode;
        private System.Windows.Forms.Label OBJWindowDisplayLabel;
        private System.Windows.Forms.Label IFGamePak;
        private System.Windows.Forms.Label IFKeypad;
        private System.Windows.Forms.Label IFDMA;
        private System.Windows.Forms.Label IFSIO;
        private System.Windows.Forms.Label IFTimers;
        private System.Windows.Forms.Label IFVCOUNT;
        private System.Windows.Forms.Label IFHBlank;
        private System.Windows.Forms.Label IFVBlank;
        private System.Windows.Forms.Label IEGamePak;
        private System.Windows.Forms.Label IEGamePakLabel;
        private System.Windows.Forms.Label IEKeypad;
        private System.Windows.Forms.Label IEKeypadLabel;
        private System.Windows.Forms.Label IEDMA;
        private System.Windows.Forms.Label IEDMALabel;
        private System.Windows.Forms.Label IESIO;
        private System.Windows.Forms.Label IESIOLabel;
        private System.Windows.Forms.Label IETimers;
        private System.Windows.Forms.Label IETimersLabel;
        private System.Windows.Forms.Label IEVCOUNT;
        private System.Windows.Forms.Label IEVCountLabel;
        private System.Windows.Forms.Label IEHBlank;
        private System.Windows.Forms.Label IEHBlankLabel;
        private System.Windows.Forms.Label IEVBlank;
        private System.Windows.Forms.Label IEVBlankLabel;
    }
}