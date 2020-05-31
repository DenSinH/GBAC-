using System;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        private int SingleDataTransfer(uint Instruction)
        {
            bool RegisterOffset, PreIndex, Up, ByteQuantity, WriteBack, LoadFromMemory;
            byte Rn, Rd;
            uint Offset;
            uint Address;
            int Cycles;

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

                this.Log(string.Format("Single Data Transfer: Mem{0:x8} -> R{1}", Address, Rd));
                if (ByteQuantity)
                {
                    this.Registers[Rd] = this.GetByteAt(Address);
                }
                else
                {
                    // If address is misaligned by a half-word amount, garbage is fetched into the upper 2 bits. (GBATek)
                    uint Result = this.GetWordAt(Address & 0xffff_fffc);
                    byte RotateAmount = (byte)((Address & 0x03) << 3);

                    // ROR result for misaligned adresses
                    if (RotateAmount != 0)
                        Result = this.ROR(Result, RotateAmount);
                    
                    this.Registers[Rd] = Result;
                }

                if (Rd == 15)
                {
                    // LDR PC take 2S + 2N +1I incremental cycles
                    Cycles = (SCycle << 1) + (NCycle << 1) + ICycle;
                    this.PipelineFlush();
                }
                else
                {
                    // Normal LDR instructions take 1S + 1N + 1I (incremental)
                    Cycles = SCycle + NCycle + ICycle;
                }
            }
            else
            {
                this.Log(string.Format("Single Data Transfer: R{1} ->  Mem{0:x8}", Address, Rd));
                /*
                 When R15 is the source register (Rd) of a register store (STR) instruction, the stored
                 value will be address of the instruction plus 12. 
                 (Manual)
                 */

                uint Value = this.Registers[Rd];
                if (Rd == 15)
                    Value += 4;

                if (ByteQuantity)
                {
                    this.SetByteAt(Address, (byte)(Value & 0x00ff));
                }
                else
                {
                    Address &= 0xffff_fffc;  // forced align for STR
                    this.SetWordAt(Address, Value);
                }

                // STR instructions take 2N incremental cycles to execute.
                Cycles = NCycle << 1;
            }

            if ((WriteBack || !PreIndex) && !(Rn == Rd && LoadFromMemory))
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
            return Cycles;
        }
    }
}
