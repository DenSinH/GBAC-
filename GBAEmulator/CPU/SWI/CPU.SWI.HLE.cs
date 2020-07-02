using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.CPU.SWI
{
    public static class HLE
    {
        public delegate int Function(uint[] Registers);

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

        public static int Div(uint[] r)
        {
            /* Manually decompiled from the BIOS (0x3b4) */
            int cycles = 0;

            r[3] = r[1] & 0x8000_0000;                                  // 1 cycle
            if ((r[1] & 0x8000_0000) > 0) r[1] = (uint)-r[1];   // 1 cycle

            r[12] = r[3] ^ r[0];                                // 1 cycle
            if ((r[0] & 0x8000_0000) > 0) r[0] = (uint)-r[0];   // 1 cycle

            r[2] = r[1];                                                // 1 cycle
            cycles += 5;

            // LAB_000003c8
            bool loop;
            do
            {
                loop = r[2] < (r[0] >> 1);                              // 1 cycle
                if (r[2] <= (r[0] >> 1)) r[2] <<= 1;            // 1 cycle
                cycles += 5;
            }
            while (loop);                                                               // 3 cycles
            cycles -= 3;  // correct for one branch not taken

            // LAB_000003d4
            uint carry;
            do
            {
                carry = (uint)(r[2] <= r[0] ? 1 : 0);                   // 1 cycle
                r[3] += r[3] + carry;                                   // 1 cycle
                if (carry == 1)
                    r[0] -= r[2];                                       // 1 cycle

                loop = r[2] != r[1];                                    // 1 cycle
                if (loop)
                    r[2] >>= 1;                                         // 1 cycle
                cycles += 8;
            }
            while (loop);                                              // 3 cycles
            cycles -= 3;  // correct for one branch not taken

            r[1] = r[0];                                                // 1 cycle
            r[0] = r[3];                                                // 1 cycle

            if ((r[12] & 0x8000_0000) > 0)                              // 1 cycle
            {
                r[0] = (uint)-r[0];                                     // 1 cycle
                r[1] = (uint)-r[1];                                     // 1 cycle
            }
            r[12] <<= 1;
            cycles += 5;
            return cycles;
        }

        public static int DivArm(uint[] r)
        {
            /* Manually decompiled from the BIOS (0x3a8) */
            r[3] = r[0];
            r[0] = r[1];
            r[1] = r[3];
            return 3 + Div(r);
        }

        public static int Sqrt(uint[] r)
        {
            int cycles = 0;
            uint pushed_r4 = r[4];
            /* 
             * Manually decompiled from the BIOS (0x404) 
             * Cycles are calculated from the ASM (S/N/I all 1 cycle in the BIOS/stack)
             */
            r[12] = r[0];
            r[1] = 1;

            cycles += 5;

            /* Set r[1] to the smallest power of 2 greater than or equal to sqrt(r[0]) (initial guess) */
            while (r[1] < r[0])
            {
                r[0] >>= 1;
                r[1] <<= 1;
                cycles += 6;
            }
            cycles += 6;  // account for first loop

            bool temp;
            do
            {
                r[0] = r[12];
                r[4] = r[1];
                r[3] = 0;

                /* Set r[2] to r[1] times the highest power of 2 such that r[2] <= r[0] */
                r[2] = r[1];

                cycles += 4;
                do
                {
                    temp = r[2] < (r[0] >> 1);
                    if (r[2] <= (r[0] >> 1))
                        r[2] <<= 1;

                    cycles += 5;
                }
                while (temp);

                /* Set r[3] so that r[3] * r[1] is the highest multiple of r[1] less than r[0] */
                do
                {
                    uint carry = (uint)((r[2] <= r[0]) ? 1 : 0);
                    r[3] += r[3] + carry;
                    if (carry == 1)
                        r[0] -= r[2];

                    temp = r[2] != r[1];
                    if (temp)
                        r[2] >>= 1;

                    cycles += 8;
                }
                while (temp);

                /* new average */
                r[1] += r[3];
                r[1] >>= 1;

                cycles += 6;
            }
            while (r[4] > r[1]);

            r[0] = r[4];
            r[4] = pushed_r4;

            cycles += 8;
            return cycles;
        }

        public static int ArcTan(uint[] r)
        {
            /* 
             * Manually decompiled from the BIOS (0x474)
             * Cycles are all 1 cycle per instruction, except mul, I assume 2 cycles per mul (average?)
             * */
            int cycles = 0;

            r[1] = r[0] * r[0];
            r[1] = (uint)(((int)r[1]) >> 14);    // asr
            r[1] = (uint)-(int)r[1];
            cycles += 4;

            r[3] = 0xa9;
            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0x390;
            cycles += 5;

            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0x91c;  // 2 adds
            cycles += 5;

            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0xfb6;  // 2 adds
            cycles += 5;

            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0x16aa; // 2 adds
            cycles += 5;

            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0x2081; // 2 adds
            cycles += 5;

            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0x3651; // 2 adds
            cycles += 5;

            r[3] *= r[1];
            r[3] = (uint)(((int)r[3]) >> 14);    // asr
            r[3] += 0xa2f9; // 2 adds
            cycles += 5;

            r[0] *= r[3];
            r[0] = (uint)(((int)r[0]) >> 16);    // asr
            cycles += 3;
            return cycles;
        }

        public static int ArcTan2(uint[] r)
        {
            /* 
             * Manually decompiled from the BIOS (0x04fc)
             * Cycles are all 1 cycle per instruction
             * */
            int cycles = 0;

            void FUN_03b4(uint[] r)
            {
                r[3] = 0x3b4;
                cycles += Div(r) + 1;
            }

            void FUN_0470(uint[] r) 
            {
                r[3] = 0x474;
                cycles += ArcTan(r) + 1;
            }

            //  PUSH r[4], r[5], r[6], r[7], lr
            uint pushed_r4 = r[4], pushed_r5 = r[5], pushed_r6 = r[6], pushed_r7 = r[7], pushed_lr = r[14];

            if (r[1] != 0)
            {
                //  LAB_510:
                if (r[0] != 0)
                {
                    //  LAB_524:
                    r[2] = r[0] << 14;  //  2 ops
                    r[3] = r[0] << 14;  //  2 ops
                    r[4] = (uint)-r[0];
                    r[5] = (uint)-r[0];
                    r[6] = 0x400;  //  2 ops
                    r[7] = 0x800;  //  (r[6] lsl 1)
                    if ((int)r[1] < 0)
                    {
                        //  LAB_572:
                        if ((int)r[0] > 0)
                        {
                            //  LAB_58a:
                            if (r[0] < r[5])
                            {
                                //  LAB_57a:
                                r[0] = r[2];
                                FUN_03b4(r);
                                FUN_0470(r);
                                r[6] += r[7];
                                r[0] = r[6] - r[0];
                                //  LAB_59e ...
                            }
                            else
                            {
                                // else omitted in asm with branch LAB_59e
                                r[1] = r[0];
                                r[0] = r[3];
                                FUN_03b4(r);
                                FUN_0470(r);
                                r[7] += r[7];
                                r[0] += r[7];
                                //  LAB_59e ...
                            }
                        }
                        else if ((int)r[4] > (int)r[5])
                        {
                            // else omitted in asm with branch LAB_59e
                            //  LAB_562:
                            r[1] = r[0];
                            r[0] = r[3];
                            FUN_03b4(r);
                            FUN_0470(r);
                            r[0] += r[7];
                            // LAB_59e ...
                        }
                        else
                        {
                            // else omitted in asm with branch LAB_59e
                            // LAB_57a
                            r[0] = r[2];
                            FUN_03b4(r);
                            FUN_0470(r);
                            r[6] += r[7];
                            r[0] = r[6] - r[0];
                            // LAB_59e ...
                        }
                    }
                    else if ((int)r[0] < 0)
                    {
                        // else omitted in asm with branch LAB_59e
                        //  LAB_55e:
                        if (r[4] < r[1])
                        {
                            //  LAB_550:
                            r[0] = r[2];
                            FUN_03b4(r);
                            FUN_0470(r);
                            r[0] = r[6] - r[0];
                            //  LAB_59e ...
                        }
                        else
                        {
                            // else omitted in asm with branch LAB_59e
                            //  LAB_562:
                            r[1] = r[0];
                            r[0] = r[3];
                            FUN_03b4(r);
                            FUN_0470(r);
                            r[0] += r[7];
                            //  LAB_59e ...
                        }
                    }
                    else if ((int)r[0] < (int)r[1])
                    {
                        // else omitted in asm with branch LAB_59e
                        //  LAB_550:
                        r[0] = r[2];
                        FUN_03b4(r);
                        FUN_0470(r);
                        r[0] = r[6] - r[0];
                        //  LAB_59e ...
                    }
                    else
                    {
                        // else omitted in asm with branch LAB_59e
                        r[1] = r[0];
                        r[0] = r[3];

                        FUN_03b4(r);
                        FUN_0470(r);
                        //  LAB_59e ...
                    }
                }
                else if ((int)r[1] < 0)
                {
                    // else omitted in asm with branch LAB_59e
                    //  LAB_51e:
                    r[0] = 0xc000;  //  with LSL
                    //  LAB_59e ...
                }
                else
                {
                    // else omitted in asm with branch LAB_59e
                    r[0] = 0x4000;  //  with LSL
                    //  LAB_59e ...
                }
            }
            else if ((int)r[0] < 0)
            {
                // else omitted in asm with branch LAB_59e
                //  LAB_50a:
                r[0] = 0x8000;  //  with LSL
                //  LAB_59e ...
            }
            else
            {
                // else omitted in asm with branch LAB_59e
                r[0] = 0;
                //  LAB_59e ...
            }


            //  LAB_59e:
            //  POP r[4], r[5], r[6], r[7]
            //  POP r[3]
            r[4] = pushed_r4;
            r[5] = pushed_r5;
            r[6] = pushed_r6;
            r[7] = pushed_r7;
            r[3] = pushed_lr;
            //  bx (return)
            return cycles;
        }
    }
}
