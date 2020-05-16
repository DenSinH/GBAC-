using System;
using System.IO;
using System.Linq;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        public void TestGBASuite(string state)
        {
            this.LoadRom(string.Format("../../Tests/GBASuite/{0}.gba", state));
            this.SkipBios();

            StreamReader file = new StreamReader(string.Format("../../Tests/GBASuite/{0}.log", state));
            string line;
            string[] splitline;
            // address,instruction,cpsr,r0,r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12,r13,r14,r15,cycles
            bool[] equal = new bool[19];  // don't care about cycles yet
            equal[0] = true;  // PC is slightly off for me as I don't track the address of the current instruction
            
            int step = 0;
            while (true)
            {
                this.Step();
                if (this.Pipeline.Count > 0)
                {
                    line = file.ReadLine();
                    if (line == null)
                    {
                        return;
                    }

                    splitline = line.Split(',');
                    // equal[0] = splitline[0].Equals(this.PC.ToString("X8"));
                    equal[1] = splitline[1].Equals("0x" + this.Pipeline.Peek().ToString("X8"));
                    equal[2] = splitline[2].Equals("0x" + this.CPSR.ToString("X8"));
                    for (int i = 0; i < 16; i++)
                        equal[3 + i] = splitline[3 + i].Equals("0x" + this.Registers[i].ToString("X8"));

                    for (int i = 0; i < 19; i++)
                    {
                        if (!equal[i])
                        {
                            Console.WriteLine(          "ERROR logfile: " + line);
                            Console.Write(string.Format("Mistake in {0}: ", i.ToString("d2")));
                            break;
                        }
                    }

                    Console.WriteLine(string.Format("0x{0:X8},0x{1:X8},0x{2:X8},", this.PC, this.Pipeline.Peek(), this.CPSR)
                    + string.Join(",", this.Registers.Select(x => "0x" + x.ToString("X8")).ToArray()));

                    Console.WriteLine(step++);
                    Console.ReadKey();
                }
            }
        }
    }
}
