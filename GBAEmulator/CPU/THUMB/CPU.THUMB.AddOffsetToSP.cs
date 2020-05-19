using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void AddOffsetToSP(ushort Instruction)
        {
            this.Log("Add offset to SP");
            bool Sign;
            uint SWord7;

            Sign = (Instruction & 0x0080) > 0;
            SWord7 = (uint)(Instruction & 0x007f) << 2;

            if (Sign)
                SP -= SWord7;
            else
                SP += SWord7;
        }
    }
}
