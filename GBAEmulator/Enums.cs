using System;

namespace GBAEmulator
{
    /*
     Contains most enums used for IO registers
     Some enums like ARM7TDMI.Mode and Interrupt are not in here, as they really only apply to the CPU

    These enums are used for communication between the CPU and the PPU and I was getting tired of typing
    ARM7TMID.EnumName the whole time, so I just bunched them together
         */
    [Flags]
    public enum DISPCNTFlags : ushort
    {
        CGBMode = 0x0008,
        DPFrameSelect = 0x0010,
        HBlankIntervalFree = 0x0020,
        OBJVRAMMapping = 0x0040,
        ForcedBlank = 0x0080,

        DisplayOBJ = 0x1000,
    }

    [Flags]
    public enum DISPSTATFlags : ushort
    {
        VBlankFlag = 0x0001,
        HBlankFlag = 0x0002,
        VCounterFlag = 0x0004,
        VBlankIRQEnable = 0x0008,
        HBlankIRQEnable = 0x0010,
        VCounterIRQEnable = 0x0020,
    }
    
    public enum AddrControl : byte
    {
        Increment = 0,
        Decrement = 1,
        Fixed = 2,
        IncrementReload = 3
    }

    public enum DMAStartTiming : byte
    {
        Immediately = 0,
        VBlank = 1,
        HBlank = 2,
        Special = 3
    }
    
    public enum BlendMode : byte
    {
        Off = 0,
        Normal = 1,
        White = 2,
        Black = 3
    }
}
