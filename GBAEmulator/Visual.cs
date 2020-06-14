using System;
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
        private int FramesUntilSaveDump = 0;
        const int SaveDumpFrameDelay = 10;

        public TickDelegate Tick;
        private const byte interval = 17; // ms

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
            this.Tick = new TickDelegate(TickDisplay);
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

            // Keyboard input handling
            this.KeyDown += this.gba.mem.IO.KEYINPUT.keyboard.KeyDown;
            this.KeyUp += this.gba.mem.IO.KEYINPUT.keyboard.KeyUp;
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
            else if (e.KeyCode == Keys.F1)
            {

            }
            else if (e.KeyCode == Keys.F2)
            {

            }
            else if (e.KeyCode == Keys.F3)
            {

            }
            else if (e.KeyCode == Keys.F4)
            {
                this.gba.cpu.pause ^= true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                
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
            this.DebugScreen.BringToFront();
            this.DebugActive = true;
        }

        private void TickDebug(object sender, EventArgs e)
        {
            if (this.DebugActive)
            {
                this.DebugScreen.UpdateAll();
            }
        }

        public delegate void TickDelegate();

        public void TickDisplay()
        {
            Draw();

            // refresh xinput in this thread to save processing power
            this.gba.mem.IO.KEYINPUT.xinput.UpdateState();

            if (this.gba.mem.Backup.BackupChanged)
            {
                this.FramesUntilSaveDump = SaveDumpFrameDelay;
                this.gba.mem.Backup.BackupChanged = false;
            }
            else if (this.FramesUntilSaveDump > 0)
            {
                this.FramesUntilSaveDump--;
                if (this.FramesUntilSaveDump == 0)
                {
                    // freeze GBA for a quick second to prevent the dump from changing while we are dumping it
                    this.gba.Pause = true;
                    this.gba.mem.Backup.DumpBackup();
                    this.gba.Pause = false;
                    Console.WriteLine("Dumped save file");
                }
            }
            
            this.Text = string.Format("GBAC-  : {0} <{1:0.0} fps>",
                this.gba.mem.ROMName, (1000 * this.gba.ppu.frame / (double)this.FPSTimer.ElapsedMilliseconds));

            if (this.gba.mem.Backup.BackupChanged)
            {
                this.Text += " Saving, do not remove GamePak;)";
            }

            if (this.FPSTimer.ElapsedMilliseconds > 2000)
            {
                this.gba.ppu.frame = 0;
                this.FPSTimer.Restart();
            }
        }

    }
}
