using System;
using System.IO;
using System.Collections.Generic;

using System.Linq;

using GBAEmulator.Memory;
using GBAEmulator.Bus;


namespace GBAEmulator.CPU
{
    public partial class ARM7TDMI
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        public State state { get; private set; }
        public readonly cPipeline Pipeline;
        private GBA gba;
        public MEM mem;
        public BUS bus;
        
        public int SCycle = 1;
        public int NCycle = 1;
        public const int ICycle = 1;

        public ARM7TDMI(GBA gba)
        {
            this.gba = gba;
            this.Pipeline = new cPipeline(this);

            this.InitARM();
            this.InitTHUMB();
            this.InitTimers();

            // cpu AND bus are required to be initialized before memory is
            this.bus = new BUS(this);
            this.mem = new MEM(this);

            this.SystemBank     = new uint[16];
            this.FIQBank        = new uint[16];
            this.SupervisorBank = new uint[16];
            this.AbortBank      = new uint[16];
            this.IRQBank        = new uint[16];
            this.UndefinedBank  = new uint[16];
            this.state          = State.ARM;

            // Initialize Register banks
            this.BankedRegisters = new Dictionary<Mode, uint[]>
            {
                { Mode.System,     this.SystemBank },
                { Mode.User,       this.SystemBank },
                { Mode.FIQ,        this.FIQBank },
                { Mode.Supervisor, this.SupervisorBank },
                { Mode.Abort,      this.AbortBank },
                { Mode.IRQ,        this.IRQBank },
                { Mode.Undefined,  this.UndefinedBank }
            };

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
            this.FIQBank[13]        = 0x03007F00;  // mode does not exist
            this.SupervisorBank[13] = 0x03007FE0;
            this.AbortBank[13]      = 0x03007F00;  // mode does not exist
            this.IRQBank[13]        = 0x03007FA0;
            this.UndefinedBank[13]  = 0x03007F00;  // mode does not exist

            this.PC = 0x08000000;
            this.CPSR = 0x6000001F;

            this.mem.IORAM.SetHalfWordAt(0x134, 0x8000);  // set RCNT to 8000 to prevent Sonic glitch
        }

        //bool COMPLOG = true;
        //StreamReader LOGFILE = new StreamReader("../../../Tests/arm.log");
        public int InstructionCycles;
        public int Step()
        {
            InstructionCycles = 0;
            int StepCycles;

            //if (this.PC == 0x0800_ce54)
            //{
            //    Console.WriteLine("Start DMA0 func");
            //    Console.WriteLine("BREAKPOINT!");
            //    COMPLOG = true;
            //}

            if (pause)
                return 1;

            this.HandleIRQs();

            if (this.mem.IORAM.HALTCNT.Halt)
            {
                this.Log("Halted");
                StepCycles = 1;  // just one to be sure that we do not exceed the amount before HBlank/VBlank/VCount
            }
            else
            {
                if ((StepCycles = this.HandleDMAs()) > 0)
                {
                    this.Log("DMAing");
                }
                else if (this.state == State.ARM)
                {
                    this.Pipeline.Enqueue(this.mem.GetWordAt(this.PC));
                    this.PC += 4;

                    if (this.Pipeline.Count == 2)
                    {
                        StepCycles = this.ExecuteARM(this.Pipeline.Dequeue());
                    }
                    else
                    {
                        StepCycles = 0;  // cycles already accounted for
                    }
                }
                else
                {
                    this.Pipeline.Enqueue(this.mem.GetHalfWordAt(this.PC));
                    this.PC += 2;

                    if (this.Pipeline.Count == 2)
                    {
                        StepCycles = this.ExecuteTHUMB((ushort)this.Pipeline.Dequeue());
                    }
                    else
                    {
                        StepCycles = 0;  // cycles already accounted for
                    }
                }
            }

            for (int i = 0; i < 4; i++) this.Timers[i].Tick(StepCycles);

            //if (COMPLOG)
            //{
            //    if (!this.mem.IORAM.HALTCNT.Halt)
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
