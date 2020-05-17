using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void PossiblePSRTransfer(uint Instruction)
        {
            if ((Instruction & 0x0fbf_0fff) == 0x010f_0000)
            {
                // MRS (transfer PSR contents to a register)
                this.MRS(Instruction);
                return;
            }
            else if ((Instruction & 0x0fb0_fff0) == 0x0120_f000 || (Instruction & 0x0fb0_f000) == 0x0320_f000)
            {
                // MSR (transfer register contents to PSR)
                this.MSR(Instruction);
                return;
            }
            else
            {
#if DEBUG
                this.Log("Ambiguous Dataprocessing reached");
#endif
                this.DataProcessing(Instruction);
                return;
            }
        }

        /*
         From https://problemkaputt.de/gbatek.htm#armopcodespsrtransfermrsmsr:
            
          Bit    Expl.
          31-28  Condition
          27-26  Must be 00b for this instruction
          25     I - Immediate Operand Flag  (0=Register, 1=Immediate) (Zero for MRS)
          24-23  Must be 10b for this instruction
          22     Psr - Source/Destination PSR  (0=CPSR, 1=SPSR_<current mode>)
          21     Opcode
                   0: MRS{cond} Rd,Psr          ;Rd = Psr
                   1: MSR{cond} Psr{_field},Op  ;Psr[field] = Op
          20     Must be 0b for this instruction (otherwise TST,TEQ,CMP,CMN)
          For MRS:
            19-16   Must be 1111b for this instruction (otherwise SWP)
            15-12   Rd - Destination Register  (R0-R14)
            11-0    Not used, must be zero.
          For MSR:
            19      f  write to flags field     Bit 31-24 (aka _flg)
            18      s  write to status field    Bit 23-16 (reserved, don't change)
            17      x  write to extension field Bit 15-8  (reserved, don't change)
            16      c  write to control field   Bit 7-0   (aka _ctl)
            15-12   Not used, must be 1111b.
          For MSR Psr,Rm (I=0)
            11-4    Not used, must be zero. (otherwise BX)
            3-0     Rm - Source Register <op>  (R0-R14)
          For MSR Psr,Imm (I=1)
            11-8    Shift applied to Imm   (ROR in steps of two 0-30)
            7-0     Imm - Unsigned 8bit Immediate
            In source code, a 32bit immediate should be specified as operand.
            The assembler should then convert that into a shifted 8bit value.

          Meaning that we can also transfer the s and x fields. However, we assume that this does not happen,
          as these values shouldn't be changed
          You COULD also set an immediate operand for general MSR, but this was not in the documentation of
          the ARM7TDMI, so I ingored this too...
           */
        private void MRS(uint Instruction)
        {
            this.Log("ARM MRS");
            byte Rd = (byte)((Instruction & 0xf000) >> 12);
            if ((Instruction & 0x0040_0000) > 0)  // source PSR bit
            {
                this.Registers[Rd] = SPSR;
            }
            else
            {
                this.Registers[Rd] = CPSR;
            }
        }

        private void MSR(uint Instruction)
        {
            this.Log("ARM MSR");
            bool ImmediateOperand = (Instruction & 0x0200_0000) > 0;
            uint Operand;
            bool f, s, x, c;

            f = (Instruction & 0x0008_0000) > 0;
            s = (Instruction & 0x0004_0000) > 0;
            x = (Instruction & 0x0002_0000) > 0;
            c = (Instruction & 0x0001_0000) > 0;

#if DEBUG
            if (s || x)
            {
                this.Log(string.Format("Dangerous PSR transfer: {0}, reserved bits ignored", Instruction.ToString("x8")));
            }
#endif
            uint BitMask = 0;
            if (f)
                BitMask |= 0xff00_0000;
            if (c)
                BitMask |= 0x0000_00ff;

            if (ImmediateOperand)
            {
                Operand = Instruction & 0x00ff;
                // Rotate in steps of 2
                byte ShiftAmount = (byte)((Instruction & 0x0f00) >> 7);  // * 2 so >> 7 instead of >> 8
                Operand = (uint)((Operand >> ShiftAmount) | ((Operand & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
            }
            else
            {
                Operand = this.Registers[Instruction & 0x0f];
            }

            if ((Instruction & 0x0040_0000) > 0)  // destination PSR bit
            {
                SPSR = (SPSR & (~BitMask)) | (Operand & BitMask);
            }
            else
            {
                CPSR = (CPSR & (~BitMask)) | (Operand & BitMask);
            }
        }

        /*
        private void MSR_all(uint Instruction)
        {
            this.Log("ARM MSR_all");
            bool ImmediateOperand = (Instruction & 0x0200_0000) > 0;
            uint Operand;

            if (ImmediateOperand)
            {
                Operand = Instruction & 0x00ff;
                // Rotate in steps of 2
                byte ShiftAmount = (byte)((Instruction & 0x0f00) >> 7);  // * 2 so >> 7 instead of >> 8
                Operand = (uint)((Operand >> ShiftAmount) | ((Operand & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
            }
            else
            {
                Operand = this.Registers[Instruction & 0x0f];
            }

            if ((Instruction & 0x0040_0000) > 0)  // destination PSR bit
            {
                SPSR = (SPSR & 0x00ff_ff00) | (Operand & 0xff00_00ff);
            }
            else
            {
                CPSR = (CPSR & 0x00ff_ff00) | (Operand & 0xff00_00ff);
            }
        }

        private void MSR_flags(uint Instruction)
        {
            this.Log("ARM MSR_flags");
            bool ImmediateOperand = (Instruction & 0x0200_0000) > 0;
            uint Operand;

            if (ImmediateOperand)
            {
                Operand = Instruction & 0x00ff;
                // Rotate in steps of 2
                byte ShiftAmount = (byte)((Instruction & 0x0f00) >> 7);  // * 2 so >> 7 instead of >> 8
                Operand = (uint)((Operand >> ShiftAmount) | ((Operand & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
            }
            else
            {
                Operand = this.Registers[Instruction & 0x0f];
            }

            if ((Instruction & 0x0040_0000) > 0)  // destination PSR bit
            {
                SPSR = (SPSR & 0x00ff_ffff) | (Operand & 0xff00_0000);
            }
            else
            {
                CPSR = (CPSR & 0x00ff_ffff) | (Operand & 0xff00_0000);
            }
        }
        */
    }
}
