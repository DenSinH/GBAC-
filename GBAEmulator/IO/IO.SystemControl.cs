using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.IO
{
    #region WAITCNT
    public class cWAITCNT : IORegister2
    {
        private static readonly int[] SRAMWaitCycles = new int[4] { 4, 3, 2, 8 };

        // waitstate N cycles are the same for all ROM waitstates
        private static readonly int[] WaitStateNCycles = new int[4] { 4, 3, 2, 8 };

        private static readonly int[] WaitState0SCycles = new int[2] { 2, 1 };
        private static readonly int[] WaitState1SCycles = new int[2] { 4, 1 };
        private static readonly int[] WaitState2SCycles = new int[2] { 8, 1 };

        public int GetWaitStateNCycles(uint section)
        {
            switch (section)
            {
                // ROM waitstate 0 (0x0800_0000)
                case 0x8:
                case 0x9:
                    return WaitStateNCycles[(this._raw & 0x000c) >> 2];
                // ROM waitstate 1 (0x0a00_0000)
                case 0xa:
                case 0xb:
                    return WaitStateNCycles[(this._raw & 0x0060) >> 5];
                // ROM waitstate 1 (0x0a00_0000)
                case 0xc:
                case 0xd:
                    return WaitStateNCycles[(this._raw & 0x0300) >> 8];
                // SRAM 
                case 0xe:
                case 0xf:
                    return SRAMWaitCycles[this._raw & 0x0003];
                default:
                    throw new IndexOutOfRangeException($"Waitstate {section} is invalid");
            }
        }
        public int GetWaitStateSCycles(uint section)
        {
            switch (section)
            {
                // ROM waitstate 0 (0x0800_0000)
                case 0x8:
                case 0x9:
                    return WaitState0SCycles[(this._raw & 0x0010) >> 4];
                // ROM waitstate 1 (0x0a00_0000)
                case 0xa:
                case 0xb:
                    return WaitState1SCycles[(this._raw & 0x0080) >> 7];
                // ROM waitstate 1 (0x0a00_0000)
                case 0xc:
                case 0xd:
                    return WaitState2SCycles[(this._raw & 0x0400) >> 10];
                // SRAM 
                case 0xe:
                case 0xf:
                    // SRAM has no S cycles
                    return 0;
                default:
                    throw new IndexOutOfRangeException($"Waitstate {section} is invalid");
            }
        }

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set((ushort)(value & 0xdfff), setlow, sethigh);
        }
    }
    #endregion

    #region HALTCNT
    public class cPOSTFLG_HALTCNT : IORegister2
    {
        // 2 1 byte registers combined
        public bool Halt;

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);
            if (sethigh)
            {
                // "games never enable stop mode" - EmuDev Discord
                Halt = true;
            }
        }
    }
    #endregion
}
