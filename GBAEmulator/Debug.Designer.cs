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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.PalettePage = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.BGPalette = new System.Windows.Forms.PictureBox();
            this.OBJPalette = new System.Windows.Forms.PictureBox();
            this.BGPaletteLabel = new System.Windows.Forms.Label();
            this.OBJPaletteLabel = new System.Windows.Forms.Label();
            this.DISPCNTLabel = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.PalettePage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BGPalette)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OBJPalette)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.PalettePage);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1162, 608);
            this.tabControl1.TabIndex = 0;
            // 
            // PalettePage
            // 
            this.PalettePage.Controls.Add(this.DISPCNTLabel);
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
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1154, 582);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // BGPalette
            // 
            this.BGPalette.Location = new System.Drawing.Point(3, 26);
            this.BGPalette.Name = "BGPalette";
            this.BGPalette.Size = new System.Drawing.Size(272, 272);
            this.BGPalette.TabIndex = 0;
            this.BGPalette.TabStop = false;
            // 
            // OBJPalette
            // 
            this.OBJPalette.Location = new System.Drawing.Point(281, 26);
            this.OBJPalette.Name = "OBJPalette";
            this.OBJPalette.Size = new System.Drawing.Size(272, 272);
            this.OBJPalette.TabIndex = 1;
            this.OBJPalette.TabStop = false;
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
            // DISPCNTLabel
            // 
            this.DISPCNTLabel.AutoSize = true;
            this.DISPCNTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DISPCNTLabel.Location = new System.Drawing.Point(559, 3);
            this.DISPCNTLabel.Name = "DISPCNTLabel";
            this.DISPCNTLabel.Size = new System.Drawing.Size(78, 20);
            this.DISPCNTLabel.TabIndex = 4;
            this.DISPCNTLabel.Text = "DISPCNT";
            // 
            // Debug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1186, 632);
            this.Controls.Add(this.tabControl1);
            this.Name = "Debug";
            this.Text = "Debug";
            this.tabControl1.ResumeLayout(false);
            this.PalettePage.ResumeLayout(false);
            this.PalettePage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BGPalette)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OBJPalette)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage PalettePage;
        private System.Windows.Forms.Label DISPCNTLabel;
        private System.Windows.Forms.Label OBJPaletteLabel;
        private System.Windows.Forms.Label BGPaletteLabel;
        private System.Windows.Forms.PictureBox OBJPalette;
        private System.Windows.Forms.PictureBox BGPalette;
        private System.Windows.Forms.TabPage tabPage2;
    }
}