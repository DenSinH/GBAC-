using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        // todo: SWI, Coprocessor instructions, Undefined

        

        private bool ARMCondition(byte field)
        {
            switch (field)
            {
                case 0b0000:  // EQ
                    return Z == 1;
                case 0b0001:  // NE
                    return Z == 0;
                case 0b0010:  // CS
                    return C == 1;
                case 0b0011:  // CC
                    return C == 0;
                case 0b0100:  // MI
                    return N == 1;
                case 0b0101:  // PL
                    return N == 0;
                case 0b0110:  // VS
                    return V == 1;
                case 0b0111:  // VC
                    return V == 0;
                case 0b1000:  // HI
                    return (C == 1) && (Z == 0);
                case 0b1001:  // LS
                    return (C == 0) && (Z == 1);
                case 0b1010:  // GE
                    return N == V;
                case 0b1011:  // LT
                    return N != V;
                case 0b1100:  // GT
                    return (Z == 0) && (N == V);
                case 0b1101:  // LE
                    return (Z == 1) || (N != V);
                case 0b1110:  // AL
                    return true;
                default:
                    throw new Exception(string.Format("Condition field {0:b4} reserved/invalid", field));
            }
        }

        private void ExecuteARM(uint Instruction)
        {
            if (!ARMCondition((byte)((Instruction & 0xf000_0000) >> 28)))
            {
                this.Log("Condition false");
                return;
            }
            
            // todo: array of length 4096 based on bits 27-20 and 7-4 with enum then switch the enum for instruction
            switch ((Instruction & 0x0c00_0000) >> 26)
            {
                case 0b00:
                    // Data Processing / Multiply / Multiply Long / Single Data Swap / Branch & Exchange / Halfword Data Transfer
                    if ((Instruction & 0x0fc0_00f0) == 0x0000_0090)
                    {
                        // Multiply
                        this.Multiply(Instruction);
                    }
                    else if ((Instruction & 0x0f80_00f0) == 0x0080_0090)
                    {
                        // Multiply Long
                        this.MultiplyLong(Instruction);
                    }
                    else if ((Instruction & 0x0fb0_0ff0) == 0x0100_0090)
                    {
                        // Single Data Swap
                        this.SWP(Instruction);
                    }
                    else if ((Instruction & 0x0fff_fff0) == 0x012f_ff10)
                    {
                        // Branch and Exchange
                        this.BX(Instruction);
                    }
                    else if ((Instruction & 0x0e40_0f90) == 0x0000_0090)
                    {
                        // Halfword Data Transfer: Register Offset
                        this.Halfword_SignedDataTransfer(Instruction);
                    }
                    else if ((Instruction & 0x0e40_0090) == 0x0040_0090)
                    {
                        // Halfword Data Transfer: Immediate Offset
                        this.Halfword_SignedDataTransfer(Instruction);
                    }
                    else if ((Instruction & 0x0fbf_0fff) == 0x010f_0000)
                    {
                        // MRS (transfer PSR contents to a register)
                        this.MRS(Instruction);
                    }
                    else if ((Instruction & 0x0fbf_fff0) == 0x0129_f000)
                    {
                        // MSR (transfer register contents to PSR)
                        this.MSR_all(Instruction);
                    }
                    else if (((Instruction & 0x0fbf_fff0) == 0x0128_f000) || ((Instruction & 0x0fbf_f000) == 0x0328_f000))
                    {
                        // MSR (transfer register contents or immediate value to PSR flag bits only)
                        // I is not set / I is set
                        this.MSR_flags(Instruction);
                    }
                    else
                    {
                        // Data Processing
                        this.DataProcessing(Instruction);
                    }
                    return;

                case 0b01:
                    // Single Data Transfer / Undefined
                    if ((Instruction & 0x0e00_0010) == 0x0600_0010)
                    {
                        // Undefined
                        throw new NotImplementedException();
                    }
                    else
                    {
                        // Single Data Transfer
                        this.SingleDataTransfer(Instruction);
                    }
                    return;

                case 0b10:
                    // Block Data Transfer / Branch
                    if ((Instruction & 0x0e00_0000) == 0x0800_0000)
                    {
                        // Block Data Transfer
                        this.BlockDataTransfer(Instruction);
                    }
                    else
                    {
                        // Branch
                        this.Branch(Instruction);
                    }
                    return;

                case 0b11:
                    // Coprocessor Data (Transfer/Operation) / Coprocessor Register Transfer / SWI
                    if ((Instruction & 0x0e00_0000) == 0x0c00_0000)
                    {
                        // Coprocessor Data Transfer
                        throw new NotImplementedException();
                    }
                    else if ((Instruction & 0x0f00_0010) == 0x0e00_0000)
                    {
                        // Coprocessor Data Operation
                        throw new NotImplementedException();
                    }
                    else if ((Instruction & 0x0f00_0010) == 0x0e00_0010)
                    {
                        // Coprocessor Register Transfer
                        throw new NotImplementedException();
                    }
                    else
                    {
                        //SWI
                        throw new NotImplementedException();
                    }
                    return;
            }
        }
    }
}
