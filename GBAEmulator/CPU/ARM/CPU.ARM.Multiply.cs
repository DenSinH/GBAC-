using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int Multiply(uint Instruction)
        {
            bool Accumulate, SetCondition;
            byte Rd, Rn, Rs, Rm;
            int mCycles = 4;

            Accumulate = (Instruction & 0x0020_0000) > 0;
            SetCondition = (Instruction & 0x0010_0000) > 0;
            Rd = (byte)((Instruction & 0x000f_0000) >> 16);
            Rn = (byte)((Instruction & 0x0000_f000) >> 12);
            Rs = (byte)((Instruction & 0x0000_0f00) >> 8);
            Rm = (byte)(Instruction & 0x0000_000f);

            // Restrictions: Rd may not be same as Rm. Rd,Rn,Rs,Rm may not be R15.
            if (Accumulate)
            {
                this.Registers[Rd] = this.Registers[Rm] * this.Registers[Rs] + this.Registers[Rn];
            }
            else
            {
                this.Registers[Rd] = this.Registers[Rm] * this.Registers[Rs];
            }

            if (SetCondition)
                this.SetNZ(this.Registers[Rd]);

            this.Log(string.Format("Multiply (Accumulate: {0}) (R{1} * R{2} ( + R{3}) -> R{4})", Accumulate, Rm, Rs, Rn, Rd));

            /*
             Execution Time: 1S+mI for MUL, and 1S+(m+1)I for MLA.
             Whereas 'm' depends on whether/how many most significant bits of Rs are all zero or all one.
             That is m=1 for Bit 31-8, m=2 for Bit 31-16, m=3 for Bit 31-24, and m=4 otherwise.
            */
            uint OperandBitComparison = this.Registers[Rs] ^ (this.Registers[Rs] << 1);
            if ((OperandBitComparison & 0xfe00_0000) == 0)
            {
                mCycles--;
                if ((OperandBitComparison & 0xfffe_0000) == 0)
                {
                    mCycles--;
                    if ((OperandBitComparison & 0xffff_fe00) == 0)
                    {
                        mCycles--;
                    }
                }
            }

            return mCycles * ICycle;
        }

        private int MultiplyLong(uint Instruction)
        {
            /*
             • R15 must not be used as an operand or as a destination register.
             • RdHi, RdLo, and Rm must all specify different registers.
             (GBATek)
            */
            bool Signed, Accumulate, SetCondition;
            byte RdHi, RdLo, Rs, Rm;
            int mCycles = 4;

            Signed = (Instruction & 0x0040_0000) > 0;
            Accumulate = (Instruction & 0x0020_0000) > 0;
            SetCondition = (Instruction & 0x0010_0000) > 0;
            RdHi = (byte)((Instruction & 0x000f_0000) >> 16);
            RdLo = (byte)((Instruction & 0x0000_f000) >> 12);
            Rs = (byte)((Instruction & 0x0000_0f00) >> 8);
            Rm = (byte)(Instruction & 0x0000_000f);
            
            if (!Accumulate)
            {
                if (Signed)
                {
                    long Result = (long)(int)this.Registers[Rs] * (long)(int)this.Registers[Rm];
                    this.Registers[RdHi] = (uint)((Result >> 32) & 0xffff_ffff);
                    this.Registers[RdLo] = (uint)(Result & 0xffff_ffff);
                }
                else
                {
                    ulong Result = (ulong)this.Registers[Rs] * (ulong)this.Registers[Rm];
                    this.Registers[RdHi] = (uint)((Result >> 32) & 0xffff_ffff);
                    this.Registers[RdLo] = (uint)(Result & 0xffff_ffff);
                }
            }
            else
            {
                if (Signed)
                {
                    long RdHiRdLo = ((long)this.Registers[RdHi] << 32) | (this.Registers[RdLo]);
                    long Result = (long)(int)this.Registers[Rs] * (long)(int)this.Registers[Rm] + RdHiRdLo;
                    this.Registers[RdHi] = (uint)((Result >> 32) & 0xffff_ffff);
                    this.Registers[RdLo] = (uint)(Result & 0xffff_ffff);
                }
                else
                {
                    ulong RdHiRdLo = ((ulong)this.Registers[RdHi] << 32) | (ulong)(this.Registers[RdLo]);
                    ulong Result = (ulong)this.Registers[Rs] * (ulong)this.Registers[Rm] + RdHiRdLo;
                    this.Registers[RdHi] = (uint)((Result >> 32) & 0xffff_ffff);
                    this.Registers[RdLo] = (uint)(Result & 0xffff_ffff);
                }
            }

            if (SetCondition)
            {
                this.N = (byte)(((this.Registers[RdHi] & 0x8000_0000) > 0) ? 1 : 0);
                this.Z = (byte)(((this.Registers[RdHi] == 0) && (this.Registers[RdLo] == 0)) ? 1 : 0);
            }

            this.Log(
                string.Format("Multiply Long (Accumulate: {0}, Signed: {1}) (R{2} * R{3} -> R{4}R{5})", Accumulate, Signed, Rm, Rs, RdHi, RdLo)
                );

            /*
             Execution Time: 1S+(m+1)I for MULL, and 1S+(m+2)I for MLAL.
             Whereas 'm' depends on whether/how many most significant bits of Rs are "all zero" (UMULL/UMLAL) or "all zero or all one" (SMULL,SMLAL).
             That is m=1 for Bit31-8, m=2 for Bit31-16, m=3 for Bit31-24, and m=4 otherwise.
            */

            uint OperandBitComparison = Accumulate ? this.Registers[Rs] : this.Registers[Rs] ^ (this.Registers[Rs] << 1);
            if ((OperandBitComparison & 0xfe00_0000) == 0)
            {
                // we falsely get here if the first 7 bits are 0 in Accumulate mode
                mCycles--;
                if ((OperandBitComparison & 0xfffe_0000) == 0)
                {
                    mCycles--;
                    if ((OperandBitComparison & 0xffff_fe00) == 0)
                    {
                        mCycles--;
                    }
                }
            }

            return mCycles * ICycle;
        }
    }
}
