using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void HiReg_BX(ushort Instruction)
        {
            this.Log("Hi Register Operations / BX");
            byte Opcode, Rs, Rd;
            bool H1, H2;

            Opcode = (byte)((Instruction & 0x0300) >> 8);
            H1 = (Instruction & 0x0080) > 0;
            H2 = (Instruction & 0x0040) > 0;
            Rs = (byte)(((Instruction & 0x0038) >> 3) | (H2 ? 8 : 0));  // Source Register
            Rd = (byte)((Instruction & 0x0007) | (H1 ? 8 : 0));  // Destination Register

            uint Op1, Op2, Result;
            Op1 = this.Registers[Rd];
            Op2 = this.Registers[Rs];
            /*
             If R15 is used as an operand, the value will be the address of the instruction + 4 with
             bit 0 cleared. Executing a BX PC in THUMB state from a non-word aligned address
             will result in unpredictable execution.
             (manual)

            For me, PC is always 4 ahead in THUMB mode, so I don't need to account for this offset
            */
            if (Rd == 15)
                Op1 &= 0xffff_fffe;
            if (Rs == 15)
                Op2 &= 0xffff_fffe;

            /*
             In this group only CMP (Op = 01) sets the CPSR condition codes.
             The action of H1= 0, H2 = 0 for Op = 00 (ADD), Op =01 (CMP) and Op = 10 (MOV) is
             undefined, and should not be used.

            (manual)
            */

            switch (Opcode)
            {
                case 0b00:
                    this.Log(string.Format("R{0} + R{1} -> R{0}", Rd, Rs));
                    this.Registers[Rd] = Op1 + Op2;
                    if (Rd == 15)
                    {
                        this.PipelineFlush();
                        this.PC &= 0xffff_fffe;
                    }
                    break;
                case 0b01:
                    this.Log(string.Format("R{0} - R{1} -> R{0}", Rd, Rs));
                    Result = Op1 - Op2;
                    this.SetCVSub(Op1, Op2, Result);
                    this.SetNZ(Result);
                    break;
                case 0b10:
                    this.Log(string.Format("R{1} -> R{0}", Rd, Rs));
                    this.Registers[Rd] = Op2;
                    if (Rd == 15)
                    {
                        this.PipelineFlush();
                        this.PC &= 0xffff_fffe;
                    }
                    break;
                case 0b11:
                    this.Log("BX");
                    this.BX(Rs);  // rest of instruction does not matter for BX, we do not read that part
                    break;
            }
        }
    }
}
