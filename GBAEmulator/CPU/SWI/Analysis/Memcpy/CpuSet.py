"""
                             **************************************************************
                             *                           CpuSet                           *
                             **************************************************************
                        00000b4c 30 b5           push       { r4, r5, lr }
                        00000b4e d4 02           lsl        r4,r2,#0xb
                        00000b50 64 0a           lsr        r4,r4,#0x9
                        00000b52 00 f0 23 f8     bl         FUN_00000b9c                                     undefined FUN_00000b9c()
                        00000b56 1e d0           beq        LAB_00000b96
                        00000b58 00 25           mov        r5,#0x0
                        00000b5a d3 0e           lsr        r3,r2,#0x1b
                        00000b5c 0c d3           bcc        LAB_00000b78
                        00000b5e 0d 19           add        r5,r1,r4
                        00000b60 53 0e           lsr        r3,r2,#0x19
                        00000b62 04 d3           bcc        LAB_00000b6e
                        00000b64 08 c8           ldmia      r0!,{ r3 }
                                             LAB_00000b66                                    XREF[1]:     00000b6c(j)  
                        00000b66 a9 42           cmp        r1,r5
                        00000b68 15 da           bge        LAB_00000b96
                        00000b6a 08 c1           stmia      r1!,{  r3 }
                        00000b6c fb e7           b          LAB_00000b66
                                             LAB_00000b6e                                    XREF[2]:     00000b62(j), 00000b76(j)  
                        00000b6e a9 42           cmp        r1,r5
                        00000b70 11 da           bge        LAB_00000b96
                        00000b72 08 c8           ldmia      r0!,{ r3 }
                        00000b74 08 c1           stmia      r1!,{  r3 }
                        00000b76 fa e7           b          LAB_00000b6e
                                             LAB_00000b78                                    XREF[1]:     00000b5c(j)  
                        00000b78 64 08           lsr        r4,r4,#0x1
                        00000b7a 53 0e           lsr        r3,r2,#0x19
                        00000b7c 05 d3           bcc        LAB_00000b8a
                        00000b7e 03 88           ldrh       r3,[r0,#0x0]
                                             LAB_00000b80                                    XREF[1]:     00000b88(j)  
                        00000b80 a5 42           cmp        r5,r4
                        00000b82 08 da           bge        LAB_00000b96
                        00000b84 4b 53           strh       r3,[r1,r5]
                        00000b86 ad 1c           add        r5,r5,#0x2
                        00000b88 fa e7           b          LAB_00000b80
                                             LAB_00000b8a                                    XREF[2]:     00000b7c(j), 00000b94(j)  
                        00000b8a a5 42           cmp        r5,r4
                        00000b8c 03 da           bge        LAB_00000b96
                        00000b8e 43 5b           ldrh       r3,[r0,r5]
                        00000b90 4b 53           strh       r3,[r1,r5]
                        00000b92 ad 1c           add        r5,r5,#0x2
                        00000b94 f9 e7           b          LAB_00000b8a
                                             LAB_00000b96                                    XREF[5]:     00000b56(j), 00000b68(j), 
                                                                                                          00000b70(j), 00000b82(j), 
                                                                                                          00000b8c(j)  
                        00000b96 30 bc           pop        { r4, r5 }
                        00000b98 08 bc           pop        { r3 }
                        00000b9a 18 47           bx         r3

            GBATek:
                Memory copy/fill in units of 4 bytes or 2 bytes. 
                Memcopy is implemented as repeated LDMIA/STMIA [Rb]!,r3 or LDRH/STRH r3,[r0,r5] instructions.
                Memfill as single LDMIA or LDRH followed by repeated STMIA [Rb]!,r3 or STRH r3,[r0,r5].
                The length must be a multiple of 4 bytes (32bit mode) or 2 bytes (16bit mode).
                The (half)wordcount in r2 must be length/4 (32bit mode) or length/2 (16bit mode),
                    ie. length in word/halfword units rather than byte units.
                  r0    Source address        (must be aligned by 4 for 32bit, by 2 for 16bit)
                  r1    Destination address   (must be aligned by 4 for 32bit, by 2 for 16bit)
                  r2    Length/Mode
                          Bit 0-20  Wordcount (for 32bit), or Halfwordcount (for 16bit)
                          Bit 24    Fixed Source Address (0=Copy, 1=Fill by {HALF}WORD[r0])
                          Bit 26    Datasize (0=16bit, 1=32bit)
                Return: No return value, Data written to destination address.
"""

# PUSH r4, r5, LR
r4 = (r2 << 11) >> 9

# FUN_b9c:
    r3 = 0xba4
    r12 = r4
    # FUN_0ba4:
    if r12 == 0:
        return;
    r12 &= ~0xfe00_0000  # BIC
    r12 += r0
    
    if (r0 ^ 0xe000_0000 == 0 || r12 ^ 0xe000_0000 == 0)
        # LAB_0b96
        # POP r4, r5, r3
        return;

r5 = 0
r3 = r2 >> 0x1b
if (carry clear: (bit 26 of r2 is 0))  // GBATek: Datasize 16 bit in this case
# LAB_0b78:
    r4 >>= 1
    r3 = r2 >> 0x19
    if (carry clear: bit 24 of r2 is 0) // GBATek: non-fixed data source address
    # LAB_0b8a
        while (r5 < r4)
            r3 = MEM_halfword[r0 + r5]
            MEM_halfword[r1 + r5] = r3
            r5 += 2
    else:
        # omitted in assembly
        r3 = MEM_halfword[r0]
        # LAB_0b80
        while (r5 < r4)
            MEM_halfword[r1 + r5] = r3
            r5 += 2

else:
    # omitted in assembly:
    r5 = 0
    r3 = r2 >> 0x1b
    if (carry clear: (bit 26 of r2 is 0))
        



