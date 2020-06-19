using System;
using System.IO;
using System.Collections.Generic;

using System.Linq;

using GBAEmulator.Memory;
using GBAEmulator.IO;
using GBAEmulator.Bus;
using GBAEmulator.Audio.Channels;

namespace GBAEmulator.CPU
{
    public partial class ARM7TDMI
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        public const int Frequency = 1 << 24;
        public State state { get; private set; }
        public readonly cPipeline Pipeline;
        private readonly GBA gba;
        public readonly IORAMSection IO;
        public readonly MEM mem;
        public readonly BUS bus;
        
        public const int ICycle = 1;

        public ARM7TDMI(GBA gba)
        {
            this.gba = gba;
            this.Pipeline = new cPipeline(this);

            this.InitARM();
            this.InitTHUMB();
            this.InitTimers();

            // IO requires bus to be initialized
            this.bus = new BUS(this);

            // mem requires IO to be initialized
            this.IO = new IORAMSection(this.bus);
            this.mem = new MEM(this);

            this.DMAChannels[0] = new DMAChannel(this.IO.DMACNT_H[0], this.IO.DMACNT_L[0], this.IO.DMASAD[0], this.IO.DMADAD[0], this.IO.IF, 0);
            this.DMAChannels[1] = new DMAChannel(this.IO.DMACNT_H[1], this.IO.DMACNT_L[1], this.IO.DMASAD[1], this.IO.DMADAD[1], this.IO.IF, 1);
            this.DMAChannels[2] = new DMAChannel(this.IO.DMACNT_H[2], this.IO.DMACNT_L[2], this.IO.DMASAD[2], this.IO.DMADAD[2], this.IO.IF, 2);
            this.DMAChannels[3] = new DMAChannel(this.IO.DMACNT_H[3], this.IO.DMACNT_L[3], this.IO.DMASAD[3], this.IO.DMADAD[3], this.IO.IF, 3);

            this.SystemBank     = new uint[16];
            // this.FIQBank        = new uint[16];
            this.SupervisorBank = new uint[16];
            // this.AbortBank      = new uint[16];
            this.IRQBank        = new uint[16];
            // this.UndefinedBank  = new uint[16];
            this.state          = State.ARM;

            // need banked registers for CPSR initialization
            this.CPSR = 0x0000005F;
        }

        private void PipelineFlush()
        {
            this.Pipeline.Clear();
        }
        
        public void SkipBios()
        {
            // From Dillon
            this.Registers[0] = 0x08000000;
            this.Registers[1] = 0xEA;

            /*
            The three stack pointers are initially initialized at the TOP of the respective areas:
                  SP_svc=03007FE0h
                  SP_irq=03007FA0h
                  SP_usr=03007F00h
            (GBATek)
             */
            this.SP                 = 0x03007F00;
            // this.FIQBank[13]        = 0x03007F00;
            this.SupervisorBank[13] = 0x03007FE0;
            // this.AbortBank[13]      = 0x03007F00;
            this.IRQBank[13]        = 0x03007FA0;
            // this.UndefinedBank[13]  = 0x03007F00;

            this.PC = 0x08000000;
            this.CPSR = 0x6000001F;

            this.IO.SetHalfWordAt(0x134, 0x8000);  // set RCNT to 8000 to prevent Sonic glitch
        }

        //bool COMPLOG = true;
        //StreamReader LOGFILE = new StreamReader("../../../Tests/arm.log");
        public int InstructionCycles;
        public int Step()
        {
            InstructionCycles = 0;

            //if (this.PC == 0x0800_ce54)
            //{
            //    Console.WriteLine("Start DMA0 func");
            //    Console.WriteLine("BREAKPOINT!");
            //    COMPLOG = true;
            //}

            if (pause)
                return 1;

            this.HandleIRQs();
            this.HandleDMAs();
            // Handling DMAs automatically causes InstructionCycles to no longer be 0 because of the memory accesses
            if (InstructionCycles > 0)
            {
                this.Log("DMAing");
            }
            else if (this.IO.HALTCNT.Halt)
            {
                this.Log("Halted");
                InstructionCycles = 1;  // just one to be sure that we do not exceed the amount before HBlank/VBlank/VCount
            }
            else
            {
                
                if (this.state == State.ARM)
                {
                    this.Pipeline.Enqueue(this.mem.GetWordAt(this.PC));
                    this.PC += 4;

                    if (this.Pipeline.Count == 2)
                    {
                        InstructionCycles += this.ExecuteARM(this.Pipeline.Dequeue());
                    }
                    else
                    {
                        // cycles already accounted for
                    }
                }
                else
                {
                    this.Pipeline.Enqueue(this.mem.GetHalfWordAt(this.PC));
                    this.PC += 2;

                    if (this.Pipeline.Count == 2)
                    {
                        InstructionCycles += this.ExecuteTHUMB((ushort)this.Pipeline.Dequeue());
                    }
                    else
                    {
                        // cycles already accounted for
                    }
                }
            }

            this.TickTimers(InstructionCycles);

            //if (COMPLOG)
            //{
            //    if (!this.IORAM.HALTCNT.Halt)
            //    {
            //        if (this.Pipeline.Count == 1)
            //        {
            //            string Line = LOGFILE.ReadLine();
            //            Console.WriteLine("LOG " + Line);
            //            Console.Write(" ACT ");
            //            this.ShowInfo();

            //            //// all registers
            //            //if (!Line.StartsWith(string.Join(" ", this.Registers.Select(x => x.ToString("X8")).ToArray()) + $" cpsr: {this.CPSR.ToString("X8")}"))
            //            //{
            //            //    Console.ReadKey();
            //            //}

            //            // wrong branch
            //            if (!Line.Contains(this.Registers[15].ToString("X8") + $" cpsr: {this.CPSR.ToString("X8")}"))
            //            {
            //                Console.ReadKey();
            //            }
            //        }
            //    }
            //}

            return InstructionCycles;
        }
    }
}
