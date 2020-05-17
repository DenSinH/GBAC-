using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private void SingleDataTransfer(uint Instruction)
        {
            this.Log("ARM Single Data Transfer");

            bool RegisterOffset, PreIndex, Up, ByteQuantity, WriteBack, LoadFromMemory;
            byte Rn, Rd;
            uint Offset;
            uint Address;

            RegisterOffset = (Instruction & 0x0200_0000) > 0;
            PreIndex = (Instruction & 0x0100_0000) > 0;
            Up = (Instruction & 0x0080_0000) > 0;
            ByteQuantity = (Instruction & 0x0040_0000) > 0;
            WriteBack = (Instruction & 0x0020_0000) > 0;
            LoadFromMemory = (Instruction & 0x0010_0000) > 0;

            Rn = (byte)((Instruction & 0x000f_0000) >> 16);
            Rd = (byte)((Instruction & 0x0000_f000) >> 12);

            Address = this.Registers[Rn];  // Base register

            /*
             The 8 shift control bits are described in the data processing instructions section.
             However, the register specified shift amounts are not available in this instruction class
             (Manual)
            */

            if (RegisterOffset)
            {
                Offset = this.Registers[Instruction & 0x0f];
                // However, the register specified shift amounts are not available in this instruction class
                byte ShiftAmount = (byte)((Instruction & 0xf80) >> 7);

                // CPSR flags are not affected. (GBATek)
                Offset = ShiftOperand(Offset, true, (byte)((Instruction & 0x60) >> 5), ShiftAmount, false);
            }
            else
            {
                Offset = (ushort)(Instruction & 0x0000_0fff);
            }

            if (PreIndex)
            {
                if (Up)
                {
                    Address += Offset;
                }
                else
                {
                    Address -= Offset;
                }
            }

            if (LoadFromMemory)
            {
                if (ByteQuantity)
                {
                    this.Registers[Rd] = this.GetAt<byte>(Address);
                }
                else
                {
                    // If address is misaligned by a half-word amount, garbage is fetched into the upper 2 bits. (GBATek)
                    uint Result = this.GetAt<uint>(Address & 0xffff_fffc);
                    byte RotateAmount = (byte)((Address & 0x03) << 3);

                    // ROR result for misaligned adresses
                    if (RotateAmount != 0)
                        Result = this.ROR(Result, RotateAmount);

                    this.Registers[Rd] = Result;
                }

                if (Rd == 15)
                    this.PipelineFlush();
            }
            else
            {
                /*
                 When R15 is the source register (Rd) of a register store (STR) instruction, the stored
                 value will be address of the instruction plus 12. (Manual)

                 We reset this to 8 at the beginning, so we will have to undo that now here
                 */
                if (Rn == 15)
                {
                    Address += 4;
                }

                if (ByteQuantity)
                {
                    this.SetAt<byte>(Address, (byte)(this.Registers[Rd] & 0x00ff));
                }
                else
                {
                    Address &= 0xffff_fffc;  // forced align for STR
                    this.SetAt<uint>(Address, this.Registers[Rd]);
                }

            }

            if (WriteBack || !PreIndex)
            {
                if (!PreIndex)
                {
                    if (Up)
                    {
                        Address += Offset;
                    }
                    else
                    {
                        Address -= Offset;
                    }
                }

                // Write-back must not be specified if R15 is specified as the base register (Rn). (Manual)
                this.Registers[Rn] = Address;
            }
        }
    }
}
