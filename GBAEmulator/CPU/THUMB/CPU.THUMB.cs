using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private delegate void THUMBInstruction(ushort Instruction);
        private THUMBInstruction[] THUMBInstructions = new THUMBInstruction[0x40];  // top 6 bits are enough to determine what instruction
        // Load / Store (7 and 8 in the manual) must be in one instruction then

        private void InitTHUMB()
        {
            // initialize THUMB instructions
        }

        private void ExecuteTHUMB(ushort Instruction)
        {

        }
    }
}
