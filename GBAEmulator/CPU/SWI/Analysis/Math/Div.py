"""                          
        **************************************************************
        *                        Div / DivARM                        *
        **************************************************************
        000003a8 00 30 a0 e1     mov        r3,r0
        000003ac 01 00 a0 e1     mov        r0,r1
        000003b0 03 10 a0 e1     mov        r1,r3

        000003b4 02 31 11 e2     ands       r3,r1,#0x80000000
        000003b8 00 10 61 42     rsbmi      r1,r1,#0x0
        000003bc 40 c0 33 e0     eors       r12,r3,r0, asr #32
        000003c0 00 00 60 22     rsbcs      r0,r0,#0x0
        000003c4 01 20 b0 e1     movs       r2,r1
                             LAB_000003c8                                    XREF[1]:     000003d0(j)  
        000003c8 a0 00 52 e1     cmp        r2,r0, lsr #0x1
        000003cc 82 20 a0 91     movls      r2,r2, lsl #0x1
        000003d0 fc ff ff 3a     bcc        LAB_000003c8
                             LAB_000003d4                                    XREF[1]:     000003e8(j)  
        000003d4 02 00 50 e1     cmp        r0,r2
        000003d8 03 30 a3 e0     adc        r3,r3,r3
        000003dc 02 00 40 20     subcs      r0,r0,r2
        000003e0 01 00 32 e1     teq        r2,r1
        000003e4 a2 20 a0 11     movne      r2,r2, lsr #0x1
        000003e8 f9 ff ff 1a     bne        LAB_000003d4
        000003ec 00 10 a0 e1     mov        r1,r0
        000003f0 03 00 a0 e1     mov        r0,r3
        000003f4 8c c0 b0 e1     movs       r12,r12, lsl #0x1
        000003f8 00 00 60 22     rsbcs      r0,r0,#0x0
        000003fc 00 10 61 42     rsbmi      r1,r1,#0x0
        00000400 1e ff 2f e1     bx         lr

    GBATek:
        Signed Division, r0/r1.
          r0  signed 32bit Number
          r1  signed 32bit Denom
        Return:
          r0  Number DIV Denom ;signed
          r1  Number MOD Denom ;signed
          r3  ABS (Number DIV Denom) ;unsigned
        For example, incoming -1234, 10 should return -123, -4, +123.
        The function usually gets caught in an endless loop upon division by zero.
        Note: The NDS9 and DSi9 additionally support hardware division,
        by math coprocessor, accessed via I/O Ports, however, the SWI function is a raw software division.
"""
# example input:
Number_r0 = -45
Denom_r1 = 10
DivARM = True

if DivARM:
    temp_r3 = Number_r0
    Number_r0 = Denom_r1
    Denom_r1 = temp_r3


# function:            
abs_Div_r3 = Denom_r1 & 0x8000_0000
if Denom_r1 < 0:
    Denom_r1 = -Denom_r1

both_negative_r12 = abs_Div_r3 ^ Number_r0  # carry if IsNegative
if Number_r0 < 0:
    Number_r0 = -Number_r0

temp_r2 = Denom_r1

# LAB_000003c8
while temp_r2 <= Number_r0 >> 1:
    temp_r2 <<= 1

# LAB_000003d4
while True:
    c = 1 if temp_r2 <= Number_r0 else 0
    abs_Div_r3 += abs_Div_r3 + c
    if c:
        Number_r0 -= temp_r2

    if (temp_r2 ^ Denom_r1) != 0:
        temp_r2 >>= 1
    else:
        break


Mod_r1 = Number_r0
Div_r0 = abs_Div_r3
if (both_negative_r12 & 0x8000_0000) > 0:  # r12 has bit 31 set only if one of the operands was negative
    Div_r0 = -Div_r0
    Mod_r1 = -Mod_r1

"return Div_r0, Mod_r1, abs_Div_r3"
# example output: -4 -5 4

