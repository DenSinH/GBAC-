using System;

namespace GBAEmulator.CPU
{
    partial class CPU
    {
        private void SingleDataTransfer(uint Instruction)
        {
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
            if (Rn == 15)
            {
                Address -= 4;  // PC is always 12 bits ahead instead of 8 for mine.
            }

            /*
             The 8 shift control bits are described in the data processing instructions section.
             However, the register specified shift amounts are not available in this instruction class
             (Manual)
            */

            if (RegisterOffset)
            {
                Offset = this.Registers[Instruction & 0x0f];
                // Shift amount is either bottom byte of register or immediate value
                byte ShiftAmount = (byte)(this.Registers[(Instruction & 0xf00) >> 8] & 0xff);

                // todo: are cpsr flags affected?
                if (ShiftAmount == 0)
                {
                    switch ((Instruction & 0x60) >> 4)  // Shift type
                    {
                        case 0b00:  // Logical Left
                            // No shift applied
                            break;
                        case 0b01:  // Logical Right
                            // Interpreted as LSR#32
                            ShiftAmount = 32;
                            break;
                        case 0b10:  // Arithmetic Right
                            // Interpreted as ASR#32
                            ShiftAmount = 32;
                            break;
                        case 0b11:  // Rotate Right
                            // Interpreted as RRX#1
                            byte newC = (byte)(Offset & 0x01);
                            Offset = (Offset >> 1) | (uint)(this.C << 31);
                            // this.C = newC;
                            // Leave ShiftAmount = 0 so that no additional shift is applied
                            break;
                    }
                }

                // We have set the shift amount accordingly above if necessary
                // if the shift amount was specified by a register that was 0 in the last byte, no shift was applied, 
                //     and the flags are not affected
                if (ShiftAmount != 0)
                {
                    switch ((Instruction & 0x60) >> 4)  // Shift type
                    {
                        case 0b00:  // Logical Left
                            Offset <<= ShiftAmount;
                            break;
                        case 0b01:  // Logical Right
                            Offset >>= ShiftAmount;
                            break;
                        case 0b10:  // Arithmetic Right
                            bool Bit31 = (Offset & 0x8000_0000) > 0;
                            Offset >>= ShiftAmount;
                            if (Bit31)
                            {
                                Offset |= (uint)(((1 << ShiftAmount) - 1) << (32 - ShiftAmount));
                            }
                            break;
                        case 0b11:  // Rotate Right
                            ShiftAmount &= 0x0f;  // mod 32 gives same result
                            Offset = (uint)((Offset >> ShiftAmount) | ((Offset & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
                            break;
                    }
                }
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
                    // todo: actually garbage?
                    this.Registers[Rd] = this.GetAt<uint>(Address);
                }
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
