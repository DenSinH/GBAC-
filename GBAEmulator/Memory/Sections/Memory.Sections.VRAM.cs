using GBAEmulator.CPU;
using GBAEmulator.Memory.IO;
using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.Memory.Sections
{
    public class VRAMSection : MemorySection
    {
        private readonly cDISPCNT DISPCNT;
        public VRAMSection(cDISPCNT DISPCNT) : base(0x18000) 
        {
            this.DISPCNT = DISPCNT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint MaskAddress(uint address)
        {
            if ((address & 0x1ffff) < 0x10000)
                return address & 0xffff;
            return 0x10000 | (address & 0x7fff);
        }

        public override byte? GetByteAt(uint address) => base.GetByteAt(MaskAddress(address));

        public override ushort? GetHalfWordAt(uint address) => base.GetHalfWordAt(MaskAddress(address));

        public override uint? GetWordAt(uint address) => base.GetWordAt(MaskAddress(address));

        public override void SetByteAt(uint address, byte value)
        {
            /*
             GBATek:
            Writing 8bit Data to Video Memory
            Video Memory (BG, OBJ, OAM, Palette) can be written to in 16bit and 32bit units only.
            Attempts to write 8bit data (by STRB opcode) won't work:

            Writes to OBJ (6010000h-6017FFFh) (or 6014000h-6017FFFh in Bitmap mode)
            ... are ignored

            Writes to BG (6000000h-600FFFFh) (or 6000000h-6013FFFh in Bitmap mode)
            ... are writing the new 8bit value to BOTH upper and
            lower 8bits of the addressed halfword, ie. "[addr AND NOT 1]=data*101h".
             */
            address = MaskAddress(address);
            if (this.DISPCNT.BGMode >= 3 && address >= 0x14000)
            {
                return;
            }
            else if (this.DISPCNT.BGMode < 3 && address >= 0x10000)
            {
                return;
            }
            this.Storage[address & 0x00ff_fffe] = value;
            this.Storage[(address & 0x00ff_fffe) + 1] = value;
        }

        public override void SetHalfWordAt(uint address, ushort value) => base.SetHalfWordAt(MaskAddress(address), value);

        public override void SetWordAt(uint address, uint value) => base.SetWordAt(MaskAddress(address), value);
    }

}
