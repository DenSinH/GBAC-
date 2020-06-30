using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GBAEmulator.Memory;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        /* ARM / THUMB state */
        public readonly uint[] Registers = new uint[16]; // active registers; R15 = PC
        private readonly uint[] SystemBank, SupervisorBank, IRQBank, FIQBank; // , FIQBank, AbortBank, UndefinedBank;
        // private readonly Dictionary<Mode, uint[]> BankedRegisters;
        private uint SPSR_svc, SPSR_irq, SPSR_fiq;                    //, SPSR_fiq, SPSR_abt, SPSR_und;  // Saved Processor Status Registers

        private byte N, Z, C, V, I, F;
        public Mode mode { get; private set; } = Mode.User;
        private uint SR_RESERVED;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint[] BankedRegisters(Mode mode)
        {
            switch (mode)
            {
                case Mode.User:
                case Mode.System:
                    return this.SystemBank;
                case Mode.Supervisor:
                    return this.SupervisorBank;
                case Mode.IRQ:
                    return this.IRQBank;
                case Mode.FIQ:
                    return this.FIQBank;
                default:
                    throw new Exception($"Invalid mode {mode}");
            }
        }

        private void ChangeMode(Mode NewMode)
        {
            if (NewMode == this.mode)
                return;

            this.Log("new mode: " + NewMode);

            bool FIQInvolved = this.mode == Mode.FIQ || NewMode == Mode.FIQ;
            // Bank current registers
            uint[] OldBank = this.BankedRegisters(this.mode);
            uint[] NewBank = this.BankedRegisters(NewMode);
            for (int i = FIQInvolved ? 8 : 13; i <= 14; i++)
            {
                OldBank[i] = this.Registers[i];
                this.Registers[i] = NewBank[i];
            }

            if (this.mode == Mode.IRQ)
                // return from IRQ
                this.mem.CurrentBIOSReadState = MEM.BIOSReadState.AfterIRQ;
            else if (this.mode == Mode.Supervisor)
                // return from SWI
                this.mem.CurrentBIOSReadState = MEM.BIOSReadState.AfterSWI;

            if (NewMode == Mode.FIQ)
                SPSR_fiq = this.CPSR;

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
                    case Mode.Supervisor:
                        return SPSR_svc;
                    case Mode.IRQ:
                        return SPSR_irq;
                    case Mode.FIQ:
                        return SPSR_fiq;
                }

                this.Error(string.Format("No SPSR for mode {0}", this.mode));
                return this.CPSR;
            }
            set
            {
                switch (this.mode)
                {
                    case Mode.Supervisor:
                        SPSR_svc = value;
                        break;
                    case Mode.IRQ:
                        SPSR_irq = value;
                        break;
                    case Mode.FIQ:
                        SPSR_fiq = value;
                        break;
                }
            }
        }

        public uint PC  // same for ARM and THUMB
        {
            get => Registers[15];
            private set => Registers[15] = value;
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
