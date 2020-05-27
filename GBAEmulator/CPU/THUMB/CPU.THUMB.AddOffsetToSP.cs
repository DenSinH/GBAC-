using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private byte AddOffsetToSP(ushort Instruction)
        {
            bool Sign;
            uint SWord7;

            Sign = (Instruction & 0x0080) > 0;
            SWord7 = (uint)(Instruction & 0x007f) << 2;

            if (Sign)
                SP -= SWord7;
            else
                SP += SWord7;

            this.Log(string.Format("Add offset to SP: Negative: {0} Offset: {1:x4}", Sign, SWord7));

            // equivalent instructions are ADD/SUB #imm
            return this.DataProcessingTimings(true, false);
        }
    }
}
