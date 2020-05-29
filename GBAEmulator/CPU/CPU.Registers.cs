using System;
using System.Collections.Generic;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
		/* ARM / THUMB state */
        public uint[] Registers = new uint[16];  // active registers; R15 = PC
        private uint[] SystemBank, FIQBank, SupervisorBank, AbortBank, IRQBank, UndefinedBank;
        private Dictionary<Mode, uint[]> BankedRegisters;
        private uint SPSR_fiq, SPSR_svc, SPSR_abt, SPSR_irq, SPSR_und;  // Saved Processor Status Registers

        private byte N, Z, C, V, I, F;
        Mode mode = Mode.User;
        private uint SR_RESERVED;

        private void ChangeMode(Mode NewMode)
        {
            if (NewMode == this.mode)
                return;

            //if (!Enum.IsDefined(typeof(Mode), NewMode))
            //    throw new Exception($"Invalid mode {NewMode}");

            bool FIQInvolved = (this.mode == Mode.FIQ) || (NewMode == Mode.FIQ);

            this.Log("new mode: " + NewMode);

            // Bank current registers
            for (int i = FIQInvolved ? 8 : 13; i <= 14; i++)
            {
                this.BankedRegisters[this.mode][i] = this.Registers[i];
                this.Registers[i] = this.BankedRegisters[NewMode][i];
            }

            switch (NewMode)
            {
                case Mode.FIQ:
                    SPSR_fiq = this.CPSR;
                    break;
                case Mode.Supervisor:
                    SPSR_svc = this.CPSR;
                    break;
                case Mode.Abort:
                    SPSR_abt = this.CPSR;
                    break;
                case Mode.IRQ:
                    SPSR_irq = this.CPSR;
                    break;
                case Mode.Undefined:
                    SPSR_und = this.CPSR;
                    break;
            }
            
            this.mode = NewMode;
        }

        private uint CPSR
        {
            get => (SR_RESERVED << 8) | (uint)(
                    (N << 31) |
                    (Z << 30) |
                    (C << 29) |
                    (V << 28) |
                    (I << 7) |
                    (F << 6) |
                    ((int)(this.state) << 5) |
                    (byte)this.mode
                    );
            set
            {
                N = (byte)(value >> 31);
                Z = (byte)((value >> 30) & 0x01);
                C = (byte)((value >> 29) & 0x01);
                V = (byte)((value >> 28) & 0x01);
                SR_RESERVED = ((value >> 8) & 0x0f_ffff);
                I = (byte)((value >> 7) & 0x01);
                F = (byte)((value >> 6) & 0x01);
                this.state = (State)((value >> 5) & 0x01);

                if ((value & 0x1f) != (byte)this.mode)
                    this.ChangeMode((Mode)(value & 0x1f));
            }
        }

        private void SetNZ(uint Result)
        {
            this.N = (byte)(((Result & 0x8000_0000) > 0) ? 1 : 0);
            this.Z = (byte)((Result == 0) ? 1 : 0);
        }

        private uint SPSR
        {
            get
            {
                switch (this.mode)
                {
                    case Mode.FIQ:
                        return SPSR_fiq;
                    case Mode.Supervisor:
                        return SPSR_svc;
                    case Mode.Abort:
                        return SPSR_abt;
                    case Mode.IRQ:
                        return SPSR_irq;
                    case Mode.Undefined:
                        return SPSR_und;
                    default:
                        this.Error(string.Format("No SPSR for mode {0}", this.mode));
                        return this.CPSR;
                }
            }
            set
            {
                switch (this.mode)
                {
                    case Mode.FIQ:
                        SPSR_fiq = value;
                        return;
                    case Mode.Supervisor:
                        SPSR_svc = value;
                        return;
                    case Mode.Abort:
                        SPSR_abt = value;
                        return;
                    case Mode.IRQ:
                        SPSR_irq = value;
                        return;
                    case Mode.Undefined:
                        SPSR_und = value;
                        return;
                    default:
                        return;
                }
            }
        }

        private uint PC  // same for ARM and THUMB
        {
            get => Registers[15];
            set => Registers[15] = value;
        }

        /* THUMB state */
        /*
		 The THUMB state registers relate to the ARM state registers in the following way:
			• THUMB state R0-R7 and ARM state R0-R7 are identical
			• THUMB state CPSR and SPSRs and ARM state CPSR and SPSRs are
			  identical
			• THUMB state SP maps onto ARM state R13
			• THUMB state LR maps onto ARM state R14
			• The THUMB state Program Counter maps onto the ARM state Program
			  Counter (R15)
		*/
        private uint SP
        {
            get => Registers[13];
            set => Registers[13] = value;
        }

		private uint LR
        {
            get => Registers[14];
            set => Registers[14] = value;
        }
		
    }
}
