using System;
using System.IO;
using System.Collections.Generic;


namespace GBAEmulator.CPU
{
    public partial class ARM7TDMI
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        public State state { get; private set; }
        private readonly Queue<uint> Pipeline = new Queue<uint>(3);
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

            this.__MemoryRegions__ = new byte[16][]
            {
                this.BIOS, this.BIOS, this.eWRAM, this.iWRAM, null, this.PaletteRAM, null, this.OAM,
                this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePakSRAM, this.GamePakSRAM
            };
        }

        int SCycle = 1;
        int NCycle = 1;
        const byte ICycle = 1;

        public string RomName { get; private set; }
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
            this.RomName = Path.GetFileName(FileName);
        }

        private void PipelineFlush()
        {
            this.Pipeline.Clear();
        }
        
        public int Step()
        {
            this.HandleIRQs();

            if (this.HALTCNT.Halt)
            {
                this.Log("Halted");
                return 1;  // just one to be sure that we do not exceed the amount before HBlank/VBlank/VCount
            }

            int DMACycles = this.HandleDMAs();
            if (DMACycles > 0)
            {
                this.Log("DMAing");
                return DMACycles;
            }

            if (this.state == State.ARM)
            {
                this.Pipeline.Enqueue(this.GetWordAt(this.PC));
                this.PC += 4;

                if (this.Pipeline.Count == 2)
                {
                    return this.ExecuteARM(this.Pipeline.Dequeue());
                }
                else
                {
                    return 1;  // how many cycles?
                }
            }
            else
            {
                this.Pipeline.Enqueue(this.GetHalfWordAt(this.PC));
                this.PC += 2;

                if (this.Pipeline.Count == 2)
                {
                    return this.ExecuteTHUMB((ushort)this.Pipeline.Dequeue());
                }
                else
                {
                    return 1;  // how many cycles?
                }
            }
        }
    }
}
