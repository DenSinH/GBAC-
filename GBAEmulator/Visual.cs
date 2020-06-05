﻿using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;

namespace GBAEmulator
{
    public partial class Visual : Form
    {
        private Bitmap Backbuffer;
        private GCHandle _rawBitmap;
        private ushort[] _display;

        private const int width = 240;
        private const int height = 160;
        private readonly int MenuStripHeight;
        private const double scale = 2;

        private readonly Stopwatch FPSTimer;
        private const byte interval = 17; // ms
        private double time;

        private GBA gba;

        private Debug DebugScreen;
        private bool DebugActive;

        public Visual(GBA gba)
        {
            InitializeComponent();
            MenuStrip ms = new MenuStrip();
            MenuStripHeight = ms.Bounds.Height;

            this.ClientSize = new Size((int)(scale * width), (int)(scale * height) + MenuStripHeight);

            this.gba = gba;
            this.DebugScreen = new Debug(gba);
            this._display = new ushort[width * height];

            // disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer, true
            );

            this.Text = "GBAC-";

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = interval;
            timer.Tick += new EventHandler(TickDebug);
            timer.Start();

            this.FPSTimer = Stopwatch.StartNew();

            ToolStripMenuItem GameMenu = new ToolStripMenuItem("Game");
            ToolStripMenuItem GameOpenItem = new ToolStripMenuItem("Open", null, new EventHandler(LoadGame));
            ToolStripMenuItem GameDebugItem = new ToolStripMenuItem("Debug", null, new EventHandler(OpenDebug));

            GameMenu.DropDownItems.Add(GameOpenItem);
            GameMenu.DropDownItems.Add(GameDebugItem);
            ((ToolStripDropDownMenu)(GameMenu.DropDown)).ShowImageMargin = false;
            ((ToolStripDropDownMenu)(GameMenu.DropDown)).ShowCheckMargin = false;

            // Assign the ToolStripMenuItem that displays 
            // the list of child forms.
            ms.MdiWindowListItem = GameMenu;

            // Add the window ToolStripMenuItem to the MenuStrip.
            ms.Items.Add(GameMenu);

            // Dock the MenuStrip to the top of the form.
            ms.Dock = DockStyle.Top;

            // The Form.MainMenuStrip property determines the merge target.
            this.MainMenuStrip = ms;

            // Add the MenuStrip last.
            // This is important for correct placement in the z-order.
            this.Controls.Add(ms);

            this.Load += new EventHandler(Visual_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Visual_Paint);

            this.KeyDown += new KeyEventHandler(Visual_KeyDown);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.gba.ShutDown = true;
            this.DebugScreen?.Close();
            base.OnFormClosing(e);
        }

        private void Visual_KeyDown(object sender, KeyEventArgs e)
        {
            // Debugging keys
            if (e.KeyCode == Keys.I)
            {
                this.gba.cpu.ShowInfo();
                this.gba.cpu.InterruptInfo();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.gba.cpu.DoIRQ();
            }
            else if (e.KeyCode == Keys.F5)
            {
                this.gba.cpu.pause = true;
            }
            else if (e.KeyCode == Keys.O)
            {
                this.gba.cpu.DumpOAM();
            }
            else if (e.KeyCode == Keys.P)
            {
                this.gba.cpu.DumpPAL();
            }
            else if (e.KeyCode == Keys.V)
            {
                this.gba.cpu.DumpVRAM(2, 4);
            }
            else if (e.KeyCode == Keys.D)
            {
                Console.WriteLine(this.gba.cpu.DISPSTAT.Get().ToString("x4"));
            }
            else if (e.KeyCode == Keys.F1)
            {
                ushort ToFind = ushort.Parse(Console.ReadLine());
                this.gba.cpu.FindValueInRAM(ToFind);
            }
            else if (e.KeyCode == Keys.F3)
            {
                this.gba.cpu.pause = true;
            }
        }

        private void Visual_Paint(object sender, PaintEventArgs e)
        {
            if (Backbuffer != null)
            {
                // no image scaling for crisp pixels!
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(this.Backbuffer, 0, MenuStripHeight, this.ClientSize.Width, this.ClientSize.Height - MenuStripHeight);
            }
        }

        private void Visual_CreateBackBuffer(object sender, EventArgs e)
        {
            this.Backbuffer?.Dispose();

            Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        private void LoadDisplay()
        {
            // converting BRG555 to RGB555 into this._display
            int ScreenCoord = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ushort BRGEntry = this.gba.display[ScreenCoord];
                    this._display[ScreenCoord++] = (ushort)(((BRGEntry & 0x001f) << 10) | (BRGEntry & 0x03e0) | ((BRGEntry & 0x7c00) >> 10));
                }
            }
        }

        private void Draw()
        {

            // ref: https://github.com/Xyene/Emulator.NES/blob/master/dotNES/Renderers/SoftwareRenderer.cs
            if (Backbuffer != null)
            {
                this.Backbuffer?.Dispose();
                this.LoadDisplay();

                //lock (this.gba.display)
                //{
                _rawBitmap = GCHandle.Alloc(this._display, GCHandleType.Pinned);
                this.Backbuffer = new Bitmap(width, height, width * 2,
                            PixelFormat.Format16bppRgb555, _rawBitmap.AddrOfPinnedObject());
                //}

                _rawBitmap.Free();
                Invalidate();  // set so that updated pixels are invalidated
            }
        }

        private void LoadGame(object sender, EventArgs e)
        {

        }

        private void OpenDebug(object sender, EventArgs e)
        {
            this.DebugScreen.Show();
            this.DebugActive = true;
        }

        private void TickDebug(object sender, EventArgs e)
        {
            if (this.DebugActive)
            {
                this.DebugScreen.UpdateAll();
            }
        }

        public void Tick()
        {
            Draw();
            
            this.Text = string.Format("GBAC-  : {0} <{1:0.0} fps>", this.gba.cpu.RomName, (1000 * this.gba.ppu.frame / (ulong)this.FPSTimer.ElapsedMilliseconds));

            if (time > 2000)
            {
                this.gba.ppu.frame = 0;
                this.FPSTimer.Restart();
            }
        }

    }
}
