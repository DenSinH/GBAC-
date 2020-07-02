"""
        **************************************************************
        *                        Div / DivARM                        *
        **************************************************************

        00000404 10 00 2d e9     stmdb      sp!,{ r4 }
        00000408 00 c0 a0 e1     mov        r12,r0
        0000040c 01 10 a0 e3     mov        r1,#0x1
                             LAB_00000410                                    XREF[1]:     0000041c(j)  
        00000410 01 00 50 e1     cmp        r0,r1
        00000414 a0 00 a0 81     movhi      r0,r0, lsr #0x1
        00000418 81 10 a0 81     movhi      r1,r1, lsl #0x1
        0000041c fb ff ff 8a     bhi        LAB_00000410
                             LAB_00000420                                    XREF[1]:     00000460(j)  
        00000420 0c 00 a0 e1     mov        r0,r12
        00000424 01 40 a0 e1     mov        r4,r1
        00000428 00 30 a0 e3     mov        r3,#0x0
        0000042c 01 20 a0 e1     mov        r2,r1
                             LAB_00000430                                    XREF[1]:     00000438(j)  
        00000430 a0 00 52 e1     cmp        r2,r0, lsr #0x1
        00000434 82 20 a0 91     movls      r2,r2, lsl #0x1
        00000438 fc ff ff 3a     bcc        LAB_00000430
                             LAB_0000043c                                    XREF[1]:     00000450(j)  
        0000043c 02 00 50 e1     cmp        r0,r2
        00000440 03 30 a3 e0     adc        r3,r3,r3
        00000444 02 00 40 20     subcs      r0,r0,r2
        00000448 01 00 32 e1     teq        r2,r1
        0000044c a2 20 a0 11     movne      r2,r2, lsr #0x1
        00000450 f9 ff ff 1a     bne        LAB_0000043c
        00000454 03 10 81 e0     add        r1,r1,r3
        00000458 a1 10 b0 e1     movs       r1,r1, lsr #0x1
        0000045c 04 00 51 e1     cmp        r1,r4
        00000460 ee ff ff 3a     bcc        LAB_00000420
        00000464 04 00 a0 e1     mov        r0,r4
        00000468 10 00 bd e8     ldmia      sp!,{ r4 }
        0000046c 1e ff 2f e1     bx         lr

        GBATek:
        Calculate square root.
          r0   unsigned 32bit number
        Return:
          r0   unsigned 16bit number
        The result is an integer value, so Sqrt(2) would return 1, to avoid this inaccuracy,
        shift left incoming number by 2*N as much as possible (the result is then shifted left by 1*N).
"""
# example input
Number_r0 = 169

#PUSH r4
Copy_r12 = Number_r0
High_guess_r1 = 1

""" set r1 to the smallest power of 2 greater or equal to sqrt(r0) """
# LAB_410
while High_guess_r1 < Number_r0:  # unsigned
    Number_r0 >>= 1
    High_guess_r1 <<= 1


# LAB_420
while True:
    Number_r0 = Copy_r12
    Low_guess_r4 = High_guess_r1
    r3 = 0

    """ set r2 to r1 * 2 ** n such that r2 <= Number_r0"""
    r2 = High_guess_r1
    # LAB_430
    while True:
        strict_less_than = r2 < (Number_r0 >> 1)
        if r2 <= (Number_r0 >> 1):  # unsigned
            r2 <<= 1
        if not strict_less_than:
            break

    """ set r3 to so that r3 * Shifted_r1 is the highest multiple of r1 less than Number_r0 """
    # LAB_43c
    while True:
        carry = 1 if r2 <= Number_r0 else 0
        r3 += r3 + carry
        if carry:
            Number_r0 -= r2

        if r2 != High_guess_r1:  # teq, movne
            r2 >>= 1
        # bne LAB_43c
        else:
            break

    """ Average, if average is less than what we previously found, return """
    High_guess_r1 += r3
    High_guess_r1 >>= 1
    if Low_guess_r4 <= High_guess_r1:
        break

Number_r0 = Low_guess_r4
# POP r4
# BX (return)

# example output: 13

# POP r4
# BX (return)