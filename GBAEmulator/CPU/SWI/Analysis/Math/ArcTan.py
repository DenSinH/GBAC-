"""                          
        **************************************************************
        *                            ArcTan                          *
        **************************************************************

        00000474 90 00 01 e0     mul        r1,r0,r0
        00000478 41 17 a0 e1     mov        r1,r1, asr #0xe
        0000047c 00 10 61 e2     rsb        r1,r1,#0x0
        00000480 a9 30 a0 e3     mov        r3,#0xa9
        00000484 91 03 03 e0     mul        r3,r1,r3
        00000488 43 37 a0 e1     mov        r3,r3, asr #0xe
        0000048c 39 3e 83 e2     add        r3,r3,#0x390
        00000490 91 03 03 e0     mul        r3,r1,r3
        00000494 43 37 a0 e1     mov        r3,r3, asr #0xe
        00000498 09 3c 83 e2     add        r3,r3,#0x900
        0000049c 1c 30 83 e2     add        r3,r3,#0x1c
        000004a0 91 03 03 e0     mul        r3,r1,r3
        000004a4 43 37 a0 e1     mov        r3,r3, asr #0xe
        000004a8 0f 3c 83 e2     add        r3,r3,#0xf00
        000004ac b6 30 83 e2     add        r3,r3,#0xb6
        000004b0 91 03 03 e0     mul        r3,r1,r3
        000004b4 43 37 a0 e1     mov        r3,r3, asr #0xe
        000004b8 16 3c 83 e2     add        r3,r3,#0x1600
        000004bc aa 30 83 e2     add        r3,r3,#0xaa
        000004c0 91 03 03 e0     mul        r3,r1,r3
        000004c4 43 37 a0 e1     mov        r3,r3, asr #0xe
        000004c8 02 3a 83 e2     add        r3,r3,#0x2000
        000004cc 81 30 83 e2     add        r3,r3,#0x81
        000004d0 91 03 03 e0     mul        r3,r1,r3
        000004d4 43 37 a0 e1     mov        r3,r3, asr #0xe
        000004d8 36 3c 83 e2     add        r3,r3,#0x3600
        000004dc 51 30 83 e2     add        r3,r3,#0x51
        000004e0 91 03 03 e0     mul        r3,r1,r3
        000004e4 43 37 a0 e1     mov        r3,r3, asr #0xe
        000004e8 a2 3c 83 e2     add        r3,r3,#0xa200
        000004ec f9 30 83 e2     add        r3,r3,#0xf9
        000004f0 93 00 00 e0     mul        r0,r3,r0
        000004f4 40 08 a0 e1     mov        r0,r0, asr #0x10
        000004f8 1e ff 2f e1     bx         lr

            GBATek:
            Calculates the arc tangent.
              r0   Tan, 16bit (1bit sign, 1bit integral part, 14bit decimal part)
            Return:
              r0   "-PI/2<THETA/<PI/2" in a range of C000h-4000h.
            Note: there is a problem in accuracy with "THETA<-PI/4, PI/4<THETA".
"""

# I know this is not really python but eh

r1 = r0 * r0
r1 >>= 15;    # asr
r1 = -r1;

r3 = 0xa9;
r3 *= r1;
r3 >>= 15;    # asr
r3 += 0x390;

r3 *= r1;
r3 >>= 15;
r3 += 0x91c;  # 2 adds

r3 *= r1;
r3 >>= 15;
r3 += 0xfb6;  # 2 adds

r3 *= r1;
r3 >>= 15;
r3 += 0x16aa; # 2 adds

r3 *= r1;
r3 >>= 15;
r3 += 0x2081; # 2 adds

r3 *= r1;
r3 >>= 15;
r3 += 0x3651; # 2 adds

r3 *= r1;
r3 >>= 15;
r3 += 0xa2f9; # 2 adds

r0 *= r3;
r0 >>= 16;
# bx (return)
