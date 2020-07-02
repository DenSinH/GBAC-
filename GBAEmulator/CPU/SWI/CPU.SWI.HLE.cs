using System;

namespace GBAEmulator.CPU.SWI
{
    public static partial class HLE
    {
        public delegate int Function(uint[] Registers, ARM7TDMI cpu);

        public static Function[] Functions = new Function[0x2b]
        {
            null,     // 00
            null,     // 01
            null,     // 02
            null,     // 03
            null,     // 04
            null,     // 05
            Div,      // 06
            DivArm,   // 07
            Sqrt,     // 08
            ArcTan,   // 09
            ArcTan2,  // 0a
            null,     // 0b
            null,     // 0c
            null,     // 0d
            null,     // 0e
            null,     // 0f
            null,     // 10
            null,     // 11
            null,     // 12
            null,     // 13
            null,     // 14
            null,     // 15
            null,     // 16
            null,     // 17
            null,     // 18
            null,     // 19
            null,     // 1a
            null,     // 1b
            null,     // 1c
            null,     // 1d
            null,     // 1e
            null,     // 1f
            null,     // 20
            null,     // 21
            null,     // 22
            null,     // 23
            null,     // 24
            null,     // 25
            null,     // 26
            null,     // 27
            null,     // 28
            null,     // 29
            null,     // 2a
        };
    }
}
