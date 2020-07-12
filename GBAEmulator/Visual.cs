using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Media;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

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
        private const int SaveDumpFrameDelay = 10;

        public TickDelegate Tick;
        private const byte interval = 17; // ms

        private GBA gba;
        private Thread PlayThread;
        private string PrevRomFile = null;

        private Debug DebugScreen;
        private bool DebugActive;

        private const string ScreenshotFolder = "./Screenshots";

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

            this.InitMenustrip(ms);

            this.Load += new EventHandler(Visual_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Visual_Paint);

            this.KeyDown += new KeyEventHandler(Visual_KeyDown);

            // Keyboard input handling
            this.KeyDown += this.gba.mem.IO.KEYINPUT.keyboard.KeyDown;
            this.KeyUp += this.gba.mem.IO.KEYINPUT.keyboard.KeyUp;


            if (!Directory.Exists(ScreenshotFolder))
                Directory.CreateDirectory(ScreenshotFolder);
            this.Icon = Properties.Resources.dillonbeeg_icon;
        }

        private void LoadBeeg()
        {
            try
            {
                this.CreateGraphics().DrawImage(Properties.Resources.DillonBeeg, 0, 0, (int)(scale * width), (int)(scale * height));
            }
            catch (Exception)
            {
                Console.WriteLine("The background file could not be loaded");
            }
        }

        private void ScreenShot()
        {
            this.Backbuffer.Save($"{ScreenshotFolder}/{this.gba.mem.ROMName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.png", ImageFormat.Png);
            using (SoundPlayer snd = new SoundPlayer(Properties.Resources.camera_shutter))
            {
                snd.Play();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.gba.PowerOff();
            this.gba.apu.speaker.ShutDown();
            this.PlayThread?.Join();
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
#if THREADED_RENDERING
                this.gba.ppu.GetRenderStatus();
#else
                Console.WriteLine("Threaded rendering is turned off...");
#endif
            }
            else if (e.KeyCode == Keys.F2)
            {
                Console.WriteLine(this.gba.bus.BusValue.ToString("x8"));
            }
            else if (e.KeyCode == Keys.F3)
            {

            }
            else if (e.KeyCode == Keys.F4)
            {
                this.gba.Pause ^= true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                
            }
            else if (e.KeyCode == Keys.F9)
            {
                if (this.PrevRomFile != null)
                    this.StartPlay(this.PrevRomFile);
            }
            else if (e.KeyCode == Keys.F12)
            {
                this.ScreenShot();
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
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "../../../roms/";
                openFileDialog.Filter = "GBA ROMS (*.gba)|*.gba|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.StartPlay(openFileDialog.FileName);
                }
            }
        }

        private void StartPlay(string filename)
        {
            this.gba.ShutDown = true;
            if (this.PlayThread != null)
            {
                this.PlayThread.Join();
                this.gba.Reset();
            }

            this.PlayThread = new Thread(() => Play(this.PrevRomFile = filename));
            this.PlayThread.SetApartmentState(ApartmentState.STA);
            this.PlayThread.Start();
        }

        private void Play(string filename)
        {
            this.gba.ShutDown = false;
            this.gba.Run(filename);
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
