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
        public long GlobalCycleCount;

        public ARM7TDMI(GBA gba, Scheduler.Scheduler scheduler)
        {
            this.gba = gba;
            this.Pipeline = new cPipeline(this);

            this.InitARM();
            this.InitTHUMB();
            this.InitTimers(scheduler);

            // IO requires bus to be initialized
            this.bus = new BUS(this);
            this.IO = new IORAMSection(this.bus);

            // DMAChannels require bus AND IO to be initialized
            this.DMAChannels[0] = new DMAChannel(this, 0);
            this.DMAChannels[1] = new DMAChannel(this, 1);
            this.DMAChannels[2] = new DMAChannel(this, 2);
            this.DMAChannels[3] = new DMAChannel(this, 3);

            // mem requires IO AND DMAChannels to be initialized
            this.mem = new MEM(this);

            this.SystemBank     = new uint[16];
            this.FIQBank        = new uint[16];
            this.SupervisorBank = new uint[16];
            // this.AbortBank      = new uint[16];
            this.IRQBank        = new uint[16];
            // this.UndefinedBank  = new uint[16];
            this.state          = State.ARM;

            // need banked registers for CPSR initialization
            this.CPSR = 0x0000005F;

            this.PipelineFlush();
            this.PC += 4;

            LOGFILE.ReadLine();
        }

        private void PipelineFlush()
        {
            // Console.WriteLine($"PipelineFlush from {PC:x8}");
            this.Pipeline.Clear();

            if (this.state == State.ARM)
            {
                this.Pipeline.Enqueue(this.mem.GetWordAt(this.PC));
                this.Pipeline.Enqueue(this.mem.GetWordAt(this.PC += 4));
            }
            else
            {
                this.Pipeline.Enqueue(this.mem.GetHalfWordAt(this.PC));
                this.Pipeline.Enqueue(this.mem.GetHalfWordAt(this.PC += 2));
            }
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
            this.FIQBank[13]        = 0x03007F00;
            this.SupervisorBank[13] = 0x03007FE0;
            // this.AbortBank[13]      = 0x03007F00;
            this.IRQBank[13]        = 0x03007FA0;
            // this.UndefinedBank[13]  = 0x03007F00;

            this.PC = 0x0800_0000;
            this.CPSR = 0x6000_001F;

            this.IO.RCNT.Set(0x8000, true, true);  // set RCNT to 8000 to prevent Sonic glitch

            this.PipelineFlush();
            this.PC += 4;
        }

        public void Reset()
        {
            Array.Clear(this.SystemBank, 0, 16);
            Array.Clear(this.SupervisorBank, 0, 16);
            Array.Clear(this.IRQBank, 0, 16);

            this.SkipBios();
        }

        bool COMPLOG = true;
        StreamReader LOGFILE = new StreamReader("../../../Tests/thumb.log");
        public int InstructionCycles;
        public int Step()
        {
            InstructionCycles = 0;

            this.HandleIRQs();
            if (this.DMAActive)
            {
                this.Log("DMAing");
                this.HandleDMAs();
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
                    //Console.WriteLine($"Fetch from {this.PC:x8}");
                    //Console.ReadKey();
                    this.Pipeline.Enqueue(this.mem.GetWordAt(this.PC));

                    if (this.Pipeline.Count != 3)
                    {
                        Console.WriteLine($"Something is wrong: {this.Pipeline.Count} in pipeline");
                        Console.ReadKey();
                    }
                    InstructionCycles += this.ExecuteARM(this.Pipeline.Dequeue());
                }
                else
                {
                    //Console.WriteLine($"Fetch from {this.PC:x8}");
                    //Console.ReadKey();
                    this.Pipeline.Enqueue(this.mem.GetHalfWordAt(this.PC));

                    if (this.Pipeline.Count != 3)
                    {
                        Console.WriteLine($"Something is wrong: {this.Pipeline.Count} in pipeline");
                        Console.ReadKey();
                    }
                    InstructionCycles += this.ExecuteTHUMB((ushort)this.Pipeline.Dequeue());
                }

                this.PC += (uint)((this.state == State.ARM) ? 4 : 2);
            }

            //if (COMPLOG)
            //{
            //    if (!this.IO.HALTCNT.Halt)
            //    {
            //        if (this.Pipeline.Count == 2)
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
            //            if (!Line.Contains((this.Registers[15] - (uint)((this.state == State.ARM) ? 4 : 2)).ToString("X8") + $" cpsr: {this.CPSR.ToString("X8")}"))
            //            {
            //                Console.ReadKey();
            //            }
            //        }
            //    }
            //}

            this.GlobalCycleCount += InstructionCycles;
            return InstructionCycles;
        }
    }
}
