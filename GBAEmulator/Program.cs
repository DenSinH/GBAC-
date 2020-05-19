using System;
using System.Threading;
using System.Windows.Forms;

namespace GBAEmulator
{
    static class Program
    {
        public static void Run(GBA gba)
        {
            gba.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ushort[] display = new ushort[240 * 160];
            GBA gba = new GBA(display);

            Thread t = new Thread(() => Run(gba));
            t.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Visual(gba));
        }
    }
}
