using System;

using GBAEmulator.Memory;
using GBAEmulator.CPU.SWI;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
#if BIOS_HLE
        const int SWIHandlerCycles = 43;
#endif

        private int SWIInstruction(uint Instruction)
        {
            this.Log(string.Format("SWI: {0:x8}", Instruction));
#if BIOS_HLE
            byte SWICode = (byte)(Instruction >> 16);
            if (SWICode < 0x2b && HLE.Functions[SWICode] != null)
            {
                // PUSH BEFORE
                uint r2  = this.Registers[2];
                uint r11 = this.Registers[11];
                uint r12 = this.Registers[12];
                int cycles = HLE.Functions[SWICode](this.Registers, this);

                // POP AFTER
                this.Registers[2]  = r2;
                this.Registers[11] = r11;
                this.Registers[12] = r12;

                this.mem.CurrentBIOSReadState = MEM.BIOSReadState.AfterSWI;
                return SWIHandlerCycles + cycles;
            }
            else
            {
                Console.WriteLine("UnHLEable SWI: " + SWICode.ToString("x2"));
            }
#endif

            this.SPSR_svc = this.CPSR;
            this.ChangeMode(Mode.Supervisor);
            this.I = 1;
            LR = this.PC - (uint)((this.state == State.THUMB) ? 2 : 4);  // which is now LR_svc

            this.state = State.ARM;
            this.PC = SWIVector;
            this.PipelineFlush();

            // Software interrupt instructions take 2S + 1N incremental cycles to execute
            return 0;
        }

        private int SWIInstruction(ushort Instruction)
        {
            return this.SWIInstruction((uint)(Instruction << 16));
        }
    }
}