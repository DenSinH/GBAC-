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

            this.InitBIOS();
            this.InitARM();
            this.InitTHUMB();
            this.InitRegisters();

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
            this.Log(string.Format("{0:x8} Bytes loaded (hex)", i));
        }

        private void PipelineFlush()
        {
            this.Pipeline.Clear();
        }
        
        public void Step()
        {
            this.HandleIRQs();

            //if (this.HALTCNT.Halt || this.HALTCNT.Stop)
            //{
            //    Console.ReadKey();
            //    return;
            //}
            //else if ((this.IF.raw & this.IE.raw) != 0)
            //{
            //    this.HALTCNT.Halt = false;
            //    this.HALTCNT.Stop = false;
            //}
            //else

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

            // Console.ReadKey();

            //if (this.Registers[15] != 0x0800_0190 && this.Registers[15] != 0x0800_0192 && this.Registers[15] != 0x0800_0194 && this.Registers[15] != 0x0800_0196)
            //{
            //    if (this.Registers[15] != 0x0800_01a4 && this.Registers[15] != 0x0800_01a6 && this.Registers[15] != 0x0800_01a8 && this.Registers[15] != 0x0800_01aa && this.Registers[15] != 0x0800_01ac)
            //    {
            //        this.ShowInfo();
            //        Console.ReadKey();
            //    }
            //}
        }
    }
}
