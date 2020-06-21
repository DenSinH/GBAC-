using System;
using System.Threading;
using System.Windows.Forms;

namespace GBAEmulator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ushort[] display = new ushort[240 * 160];
            GBA gba = new GBA(display);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            gba.vis = new Visual(gba);

            Application.Run(gba.vis);
        }
    }
}
