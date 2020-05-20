using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace GBAEmulator.CPU
{
    public partial class ARM7TDMI
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        State state;
        readonly Queue<uint> Pipeline = new Queue<uint>(3);
        GBA gba;

        public ARM7TDMI(GBA gba)
        {
            this.gba = gba;

            this.SystemBank = new uint[16];
            this.FIQBank = new uint[16];
            this.SupervisorBank = new uint[16];
            this.AbortBank = new uint[16];
            this.IRQBank = new uint[16];
            this.UndefinedBank = new uint[16];
            this.state = State.ARM;

            // Initialize Register banks
            this.BankedRegisters = new Dictionary<Mode, uint[]>
            {
                { Mode.System, this.SystemBank },
                { Mode.User, this.SystemBank },
                { Mode.FIQ, this.FIQBank },
                { Mode.Supervisor, this.SupervisorBank },
                { Mode.Abort, this.AbortBank },
                { Mode.IRQ, this.IRQBank },
                { Mode.Undefined, this.UndefinedBank }
            };

            // need banked registers for CPSR initialization
            this.CPSR = 0x0000005F;

            this.__MemoryRegions__ = new byte[15][]
            {
                this.BIOS, this.BIOS, this.eWRAM, this.iWRAM, null, this.PaletteRAM, null, this.OAM,
                this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePakSRAM
            };

            this.InitARM();
            this.InitTHUMB();
            this.InitRegisters();
        }

        public void LoadRom(string FileName)
        {
            FileStream fs = File.OpenRead(FileName);
            int current = fs.ReadByte();
            uint i = 0;

            while (current != -1)
            {
                this.GamePak[i] = (byte)current;
                current = fs.ReadByte();
                i++;
            }
            Console.WriteLine(string.Format("{0:x8} Bytes loaded (hex)", i));
        }

        private void PipelineFlush()
        {
            this.Pipeline.Clear();
        }

        public void Step()
        {
            if (this.state == State.ARM)
            {
                this.Pipeline.Enqueue(this.GetWordAt(this.PC));
                this.PC += 4;

                if (this.Pipeline.Count == 2)
                {
                    this.ExecuteARM(this.Pipeline.Dequeue());
                }
                else
                {
                    this.Log("Filling Pipeline");
                }
            }
            else
            {
                this.Pipeline.Enqueue(this.GetHalfWordAt(this.PC));
                this.PC += 2;

                if (this.Pipeline.Count == 2)
                {
                    this.ExecuteTHUMB((ushort)this.Pipeline.Dequeue());
                }
                else
                {
                    this.Log("Filling Pipeline");
                }
            }

            if (this.Registers[1] < 0x100 || this.Registers[1] > 0x400000)
            {
                this.ShowInfo();
                // Console.ReadKey();
            }
        }
        
        [Conditional("DEBUG")]
        private void Error(string message)
        {
            Console.Error.WriteLine("Error: " + message);
        }
        
        [Conditional("DEBUG")]
        private void Log(string message)
        {
            if (this.Registers[1] < 0x100 || this.Registers[1] > 0x400000)
            {
                Console.WriteLine(message);
            }
            // Console.WriteLine(message);
        }

        [Conditional("DEBUG")]
        private void ShowInfo()
        {
            Console.WriteLine(string.Join(",", this.Registers.Select(x => "0x" + x.ToString("X8")).ToArray()));
        }
    }
}
