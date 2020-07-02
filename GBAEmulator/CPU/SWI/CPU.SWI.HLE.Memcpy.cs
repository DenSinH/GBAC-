using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.CPU.SWI
{
    static partial class HLE
    {
        static int CpuSet(uint[] r, ARM7TDMI cpu)
        {
            int cycles = 0;

            bool FixedSource = (r[2] & 0x0100_0000) > 0;
            bool DataSize = (r[2] & 0x0400_0000) > 0;  // 0: 16 bit, 1: 32 bit
            uint UnitCount = (r[2] & 0x00f_ffff);

            if (DataSize)
            {
                if (FixedSource)
                {
                    r[3] = cpu.mem.GetWordAt(r[0]);
                    r[0] += 4;
                    cycles += 2;
                }

                while (UnitCount-- > 0)
                {
                    if (FixedSource)
                    {
                        // uses STMIA r1!
                        cpu.mem.SetWordAt(r[1], r[3]);
                        r[1] += 4;
                        cycles += 4;  // branch and instruction fetch, load/store value is in InstructionCycles
                    }
                    else
                    {
                        // uses LDMIA/STMIA rx!
                        r[3] = cpu.mem.GetWordAt(r[0]);
                        r[0] += 4;
                        cpu.mem.SetWordAt(r[1], r[3]);
                        r[1] += 4;
                        cycles += 6;  // branch and instruction fetch, load/store value is in InstructionCycles
                    }
                }
            }
            else
            {
                if (FixedSource)
                {
                    r[3] = cpu.mem.GetHalfWordAt(r[0]);
                    cycles += 2;
                }

                r[5] = 0;
                while (UnitCount-- > 0)
                {
                    if (FixedSource)
                    {
                        cpu.mem.SetHalfWordAt(r[1] + r[5], (ushort)r[3]);
                        r[5] += 2;
                        cycles += 4;
                    }
                    else
                    {
                        r[3] = cpu.mem.GetHalfWordAt(r[0] + r[5]);
                        cpu.mem.SetHalfWordAt(r[1] + r[5], (ushort)r[3]);
                        r[5] += 2;
                        cycles += 4;
                    }
                }

            }

            return cycles;
        }
    }
}
