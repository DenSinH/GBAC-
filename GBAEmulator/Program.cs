using System;
using System.Threading;
using System.Windows.Forms;
using GBAEmulator.CPU;

namespace GBAEmulator
{
    static class Program
    {
        public static void Run()
        {
            ARM7TDMI cpu = new ARM7TDMI();
            cpu.TestGBASuite("arm");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread t = new Thread(Run);
            t.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Visual());
        }
    }
}
