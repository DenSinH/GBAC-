using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        public enum BIOSReadState : uint
        {
            StartUp =   0x00DC + 8,
            DuringIRQ = 0x0134 + 8,
            AfterIRQ =  0x013C + 8,
            AfterSWI =  0x0188 + 8
        }
    }
}
