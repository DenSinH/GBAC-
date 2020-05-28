using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private const double scale = 2;

        private const byte interval = 17; // ms
        private double time;

        private GBA gba;

        public Visual(GBA gba)
        {
            InitializeComponent();
            this.ClientSize = new Size((int)(scale * width), (int)(scale * height));

            this.gba = gba;
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
            timer.Tick += new EventHandler(Tick);
            timer.Start();

            this.Load += new EventHandler(Visual_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Visual_Paint);

            this.KeyDown += new KeyEventHandler(Visual_KeyDown);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.gba.ShutDown = true;
            base.OnFormClosing(e);
        }

        private void Visual_KeyDown(object sender, KeyEventArgs e)
        {
            // Debugging keys
            if (e.KeyCode == Keys.I)
            {
                this.gba.cpu.ShowInfo();
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
                this.gba.cpu.DumpVRAM(1, 4);
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
                e.Graphics.DrawImage(this.Backbuffer, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }
        }

        private void Visual_CreateBackBuffer(object sender, EventArgs e)
        {
            this.Backbuffer?.Dispose();

            Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        private void LoadDisplay()
        {
            // converting BRG555 to RGB555
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ushort BRGEntry = this.gba.display[width * y + x];
                    this._display[width * y + x] = (ushort)(((BRGEntry & 0x001f) << 10) | (BRGEntry & 0x03e0) | ((BRGEntry & 0x7c00) >> 10));
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

        private void Tick(object sender, EventArgs e)
        {
            Draw();

            time += interval;
            this.Text = string.Format("GBAC- <{0:0.0} fps>", (1000 * this.gba.ppu.frame / this.time));
            if (time > 2000)
            {
                this.gba.ppu.frame = 0;
                this.time = 0;
            }
        }

    }
}
