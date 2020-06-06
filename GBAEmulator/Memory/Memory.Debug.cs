using System;

using System.Diagnostics;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        private void Error(string message)
        {
            Console.Error.WriteLine($"Memory Error: {message}");
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
