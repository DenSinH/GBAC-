﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace GBAEmulator.CPU
{
    public partial class ARM7TDMI
    {
        /*
         Emulation of the ARM7TDMI CPU
        */
        State state;
        readonly Queue<uint> Pipeline = new Queue<uint>(3);
        public ARM7TDMI()
        {
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
                this.BIOS, this.BIOS, this.eWRAM, this.iWRAM, this.IORAM, this.PaletteRAM, this.VRAM, this.OAM,
                this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePak, this.GamePakSRAM
            };

            this.InitARM();
            this.InitTHUMB();
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

        private void Step()
        {
            if (this.state == State.ARM)
            {
                this.Pipeline.Enqueue(this.GetWordAt(this.PC));
                this.PC += 4;

                if (this.Pipeline.Count == 2)
                {
                    this.ExecuteARM(this.Pipeline.Dequeue());
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
            Console.WriteLine(message);
        }
    }
}
