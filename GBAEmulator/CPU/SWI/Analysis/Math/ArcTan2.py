"""                          
        **************************************************************
        *                            ArcTan2                         *
        **************************************************************

        000004fc f0 b5           push       { r4, r5, r6, r7, lr }
        000004fe 00 29           cmp        r1,#0x0
        00000500 06 d1           bne        LAB_00000510
        00000502 00 28           cmp        r0,#0x0
        00000504 01 db           blt        LAB_0000050a
        00000506 00 20           mov        r0,#0x0
        00000508 49 e0           b          LAB_0000059e
                             LAB_0000050a                                    XREF[1]:     00000504(j)  
        0000050a 80 20           mov        r0,#0x80
        0000050c 00 02           lsl        r0,r0,#0x8
        0000050e 46 e0           b          LAB_0000059e
                             LAB_00000510                                    XREF[1]:     00000500(j)  
        00000510 00 28           cmp        r0,#0x0
        00000512 07 d1           bne        LAB_00000524
        00000514 00 29           cmp        r1,#0x0
        00000516 02 db           blt        LAB_0000051e
        00000518 40 20           mov        r0,#0x40
        0000051a 00 02           lsl        r0,r0,#0x8
        0000051c 3f e0           b          LAB_0000059e
                             LAB_0000051e                                    XREF[1]:     00000516(j)  
        0000051e c0 20           mov        r0,#0xc0
        00000520 00 02           lsl        r0,r0,#0x8
        00000522 3c e0           b          LAB_0000059e
                             LAB_00000524                                    XREF[1]:     00000512(j)  
        00000524 02 1c           add        r2,r0,#0x0
        00000526 92 03           lsl        r2,r2,#0xe
        00000528 0b 1c           add        r3,r1,#0x0
        0000052a 9b 03           lsl        r3,r3,#0xe
        0000052c 44 42           rsb        r4,r0
        0000052e 4d 42           rsb        r5,r1
        00000530 40 26           mov        r6,#0x40
        00000532 36 02           lsl        r6,r6,#0x8
        00000534 77 00           lsl        r7,r6,#0x1
        00000536 00 29           cmp        r1,#0x0
        00000538 1b db           blt        LAB_00000572
        0000053a 00 28           cmp        r0,#0x0
        0000053c 0f db           blt        LAB_0000055e
        0000053e 88 42           cmp        r0,r1
        00000540 06 db           blt        LAB_00000550
        00000542 01 1c           add        r1,r0,#0x0
        00000544 18 1c           add        r0,r3,#0x0
        00000546 ff f7 2d ff     bl         thunk_FUN_000003b4                               undefined thunk_FUN_000003b4()
        0000054a ff f7 91 ff     bl         FUN_00000470                                     undefined FUN_00000470()
        0000054e 26 e0           b          LAB_0000059e
                             LAB_00000550                                    XREF[2]:     00000540(j), 00000560(j)  
        00000550 10 1c           add        r0,r2,#0x0
        00000552 ff f7 27 ff     bl         thunk_FUN_000003b4                               undefined thunk_FUN_000003b4()
        00000556 ff f7 8b ff     bl         FUN_00000470                                     undefined FUN_00000470()
        0000055a 30 1a           sub        r0,r6,r0
        0000055c 1f e0           b          LAB_0000059e
                             LAB_0000055e                                    XREF[1]:     0000053c(j)  
        0000055e 8c 42           cmp        r4,r1
        00000560 f6 db           blt        LAB_00000550
                             LAB_00000562                                    XREF[1]:     00000578(j)  
        00000562 01 1c           add        r1,r0,#0x0
        00000564 18 1c           add        r0,r3,#0x0
        00000566 ff f7 1d ff     bl         thunk_FUN_000003b4                               undefined thunk_FUN_000003b4()
        0000056a ff f7 81 ff     bl         FUN_00000470                                     undefined FUN_00000470()
        0000056e 38 18           add        r0,r7,r0
        00000570 15 e0           b          LAB_0000059e
                             LAB_00000572                                    XREF[1]:     00000538(j)  
        00000572 00 28           cmp        r0,#0x0
        00000574 09 dc           bgt        LAB_0000058a
        00000576 ac 42           cmp        r4,r5
        00000578 f3 dc           bgt        LAB_00000562
                             LAB_0000057a                                    XREF[1]:     0000058c(j)  
        0000057a 10 1c           add        r0,r2,#0x0
        0000057c ff f7 12 ff     bl         thunk_FUN_000003b4                               undefined thunk_FUN_000003b4()
        00000580 ff f7 76 ff     bl         FUN_00000470                                     undefined FUN_00000470()
        00000584 f6 19           add        r6,r6,r7
        00000586 30 1a           sub        r0,r6,r0
        00000588 09 e0           b          LAB_0000059e
                             LAB_0000058a                                    XREF[1]:     00000574(j)  
        0000058a a8 42           cmp        r0,r5
        0000058c f5 db           blt        LAB_0000057a
        0000058e 01 1c           add        r1,r0,#0x0
        00000590 18 1c           add        r0,r3,#0x0
        00000592 ff f7 07 ff     bl         thunk_FUN_000003b4                               undefined thunk_FUN_000003b4()
        00000596 ff f7 6b ff     bl         FUN_00000470                                     undefined FUN_00000470()
        0000059a ff 19           add        r7,r7,r7
        0000059c 38 18           add        r0,r7,r0
                             LAB_0000059e                                    XREF[8]:     00000508(j), 0000050e(j), 
                                                                                          0000051c(j), 00000522(j), 
                                                                                          0000054e(j), 0000055c(j), 
                                                                                          00000570(j), 00000588(j)  
        0000059e f0 bc           pop        { r4, r5, r6, r7 }
        000005a0 08 bc           pop        { r3 }
        000005a2 18 47           bx         r3
                             -- Flow Override: RETURN (TERMINATOR)


            Calculates the arc tangent after correction processing.
            Use this in normal situations.
              r0   X, 16bit (1bit sign, 1bit integral part, 14bit decimal part)
              r1   Y, 16bit (1bit sign, 1bit integral part, 14bit decimal part)
            Return:
              r0   0000h-FFFFh for 0<=THETA<2PI.

"""

# I know this is not really python but eh

def FUN_03b4();
    r3 = 0x3b4
    Div()

def FUN_0470():
    r3 = 0x474
    ArcTan()


# PUSH r4, r5, r6, r7, lr

if r1 != 0:
    # LAB_510:
    if r0 != 0:
        # LAB_524:
        r2 = r0 << 14;  # 2 ops
        r3 = r0 << 14;  # 2 ops
        r4 = -r0;
        r5 = -r0;
        r6 = 0x400;  # 2 ops
        r7 = 0x800;  # (r6 lsl 1)
        if r1 < 0:
            # LAB_572:
            if r0 > 0:
                # LAB_58a:
                if r0 < r5:
                    # LAB_57a:
                    r0 = r2;
                    FUN_03b4()
                    FUN_0470()
                    r6 += r7;
                    r0 = r6 - r0;
                    # LAB_59e ...


                r1 = r0;
                r0 = r3;
                FUN_03b4()
                FUN_0470()
                r7 += r7;
                r0 += r7;
                # LAB_59e ...

            if r4 > r5:
                # LAB_562:
                r1 = r0;
                r0 = r3;
                FUN_03b4()
                FUN_0470()
                r0 += r7;
                # LAB_59e ...

        if r0 < 0:
            # LAB_55e:
            if r4 < r1:
                # LAB_550:
                r0 = r2;
                FUN_03b4()
                FUN_0470()
                r0 = r6 - r0;
                # LAB_59e ...

            # LAB_562:
            r1 = r0;
            r0 = r3;
            FUN_03b4()
            FUN_0470()
            r0 += r7;
            # LAB_59e ...

        if r0 < r1:
            # LAB_550:
            r0 = r2;
            FUN_03b4()
            FUN_0470()
            r0 = r6 - r0;
            # LAB_59e ...


        # copy values over
        r1 = r0
        r0 = r3
        
        FUN_03b4()
        FUN_0470()
        # LAB_59e ...
        ...
    if r1 < 0:
        # LAB_51e:
        r0 = 0xc000;  # with LSL
        # LAB_59e ...

    r0 = 0x4000;  # with LSL
    # LAB_59e ...
    ...

if r0 < 0:
    # LAB_50a:
    r0 = 0x8000;  # with LSL
    # LAB_59e ...

r0 = 0;
# LAB_59e ...

# LAB_59e:
# POP r4, r5, r6, r7
# POP r3
# bx (return)

"""
C#:
def FUN_03b4();
    r[3] = 0x3b4
    Div()

def FUN_0470():
    r[3] = 0x474
    ArcTan()


//  PUSH r[4], r[5], r[6], r[7], lr

if (r[1] != 0) 
{
    //  LAB_510:
    if (r[0] != 0)
    {
        //  LAB_524:
        r[2] = r[0] << 14;  //  2 ops
        r[3] = r[0] << 14;  //  2 ops
        r[4] = -r[0];
        r[5] = -r[0];
        r[6] = 0x400;  //  2 ops
        r[7] = 0x800;  //  (r[6] lsl 1)
        if (r[1] < 0)
        {
            //  LAB_572:
            if (r[0] > 0)
            {
                //  LAB_58a:
                if (r[0] < r[5])
                {
                    //  LAB_57a:
                    r[0] = r[2];
                    FUN_03b4()
                    FUN_0470()
                    r[6] += r[7];
                    r[0] = r[6] - r[0];
                    //  LAB_59e ...
                }

                r[1] = r[0];
                r[0] = r[3];
                FUN_03b4()
                FUN_0470()
                r[7] += r[7];
                r[0] += r[7];
                //  LAB_59e ...
            }

            if (r[4] > r[5])
            {
                //  LAB_562:
                r[1] = r[0];
                r[0] = r[3];
                FUN_03b4()
                FUN_0470()
                r[0] += r[7];
                //  LAB_59e ...
            }
        }

        if (r[0] < 0)
        {
            //  LAB_55e:
            if (r[4] < r[1])
            {
                //  LAB_550:
                r[0] = r[2];
                FUN_03b4()
                FUN_0470()
                r[0] = r[6] - r[0];
                //  LAB_59e ...
            }

            //  LAB_562:
            r[1] = r[0];
            r[0] = r[3];
            FUN_03b4()
            FUN_0470()
            r[0] += r[7];
            //  LAB_59e ...
        }

        if (r[0] < r[1])
        {
            //  LAB_550:
            r[0] = r[2];
            FUN_03b4()
            FUN_0470()
            r[0] = r[6] - r[0];
            //  LAB_59e ...
        }


        //  copy values over
        r[1] = r[0]
        r[0] = r[3]
        
        FUN_03b4()
        FUN_0470()
        //  LAB_59e ...
        ...
	}
    if (r[1] < 0)
    {
        //  LAB_51e:
        r[0] = 0xc000;  //  with LSL
        //  LAB_59e ...
    }

    r[0] = 0x4000;  //  with LSL
    //  LAB_59e ...
    ...
}
if (r[0] < 0)
{
    //  LAB_50a:
    r[0] = 0x8000;  //  with LSL
    //  LAB_59e ...
}

r[0] = 0;
//  LAB_59e ...

//  LAB_59e:
//  POP r[4], r[5], r[6], r[7]
//  POP r[3]
//  bx (return)
"""