using System;

namespace GBAEmulator.CPU
{
    partial class CPU
    {
        private bool Condition(byte field)
        {
            switch (field)
            {
                case 0b0000:  // EQ
                    return Z == 1;
                case 0b0001:  // NE
                    return Z == 0;
                case 0b0010:  // CS
                    return C == 1;
                case 0b0011:  // CC
                    return C == 0;
                case 0b0100:  // MI
                    return N == 1;
                case 0b0101:  // PL
                    return N == 0;
                case 0b0110:  // VS
                    return V == 1;
                case 0b0111:  // VC
                    return V == 0;
                case 0b1000:  // HI
                    return (C == 1) && (Z == 0);
                case 0b1001:  // LS
                    return (C == 0) && (Z == 1);
                case 0b1010:  // GE
                    return N == V;
                case 0b1011:  // LT
                    return N != V;
                case 0b1100:  // GT
                    return (Z == 0) && (N == V);
                case 0b1101:  // LE
                    return (Z == 1) && (N != V);
                case 0b1110:  // AL
                    return true;
                default:
                    throw new Exception(string.Format("Condition field {0:b4} reserved/invalid", field));
            }
        }
        private void ExecuteARM(uint instruction)
        {
            if (!Condition((byte)((instruction & 0xf000_0000) >> 28)))
            {
                return;
            }

            switch ((instruction & 0x0a00_0000) >> 25)
            {
                case 0b00:
                    // Data Processing / Multiply / Multiply Long / Single Data Swap / Branch & Exchange / Halfword Data Transfer
                    if ((instruction & 0x0fa0_00f0) == 0x0000_0090)
                    {
                        // Multiply
                    }
                    else if ((instruction & 0x0fe80_00f0) == 0x0080_0090)
                    {
                        // Multiply Long
                    }
                    else if ((instruction & 0x0fb0_0ff0) == 0x0100_0090)
                    {
                        // Single Data Swap
                    }
                    else if ((instruction & 0x0fff_fff0) == 0x012f_ff10)
                    {
                        // Branch and Exchange
                    }
                    else if ((instruction & 0x0e40_0f90) == 0x0000_0090)
                    {
                        // Halfword Data Transfer: Register Offset
                    }
                    else if ((instruction & 0x0e40_0090) == 0x0040_0090)
                    {
                        // Halfword Data Transfer: Immediate Offset
                    }
                    else
                    {
                        // Data Processing / PSR Transfer
                    }
                    return;

                case 0b01:
                    // Single Data Transfer / Undefined
                    if ((instruction & 0x0e00_0010) == 0x0600_0010)
                    {
                        // Undefined
                    }
                    else
                    {
                        // Single Data Transfer
                    }
                    return;

                case 0b10:
                    // Block Data Transfer / Branch
                    if ((instruction & 0x0e00_0000) == 0x0800_0000)
                    {
                        // Block Data Transfer
                    }
                    else
                    {
                        // Branch
                    }
                    return;

                case 0b11:
                    // Coprocessor Data (Transfer/Operation) / Coprocessor Register Transfer / SWI
                    if ((instruction & 0x0e00_0000) == 0x0c00_0000)
                    {
                        // Coprocessor Data Transfer
                    }
                    else if ((instruction & 0x0f00_0010) == 0x0e00_0000)
                    {
                        // Coprocessor Data Operation
                    }
                    else if ((instruction & 0x0f00_0010) == 0x0e00_0010)
                    {
                        // Coprocessor Register Transfer
                    }
                    else
                    {
                        //SWI
                    }
                    return;
            }
        }
    }
}
