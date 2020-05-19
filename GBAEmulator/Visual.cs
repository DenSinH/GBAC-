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

        private const int width = 240;
        private const int height = 160;
        private const double scale = 2;

        private GBA gba;

        public Visual(GBA gba)
        {
            InitializeComponent();
            this.Size = new Size((int)(scale * width), (int)(scale * height));

            this.gba = gba;

            // disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer, true
            );

            this.Text = "GBA Emulator";

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 17;
            timer.Tick += new EventHandler(Tick);
            timer.Start();

            this.Load += new EventHandler(Visual_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Visual_Paint);

            this.KeyDown += new KeyEventHandler(Visual_KeyDown);
        }

        private void Visual_KeyDown(object sender, KeyEventArgs e)
        {
            // Debugging keys
            if (e.KeyCode == Keys.O)
            {
                Console.WriteLine("Oh!");
            }
        }

        private void Visual_Paint(object sender, PaintEventArgs e)
        {
            if (Backbuffer != null)
            {
                // no image scaling for crisp pixels!
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(this.Backbuffer, 0, 0, this.Size.Width, this.Size.Height);
            }
        }

        private void Visual_CreateBackBuffer(object sender, EventArgs e)
        {
            this.Backbuffer?.Dispose();

            Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        private void Draw()
        {

            // ref: https://github.com/Xyene/Emulator.NES/blob/master/dotNES/Renderers/SoftwareRenderer.cs
            if (Backbuffer != null)
            {
                this.Backbuffer?.Dispose();
                //lock (this.gba.display)
                //{
                _rawBitmap = GCHandle.Alloc(this.gba.display, GCHandleType.Pinned);
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
        }

    }
}
