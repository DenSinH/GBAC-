using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        #region KEYINPUT
        private class cKeyInput : IORegister2
        {
            Controller controller = new XInputController();
            cKeyInterruptControl KEYCNT;
            MEM mem;

            public cKeyInput(cKeyInterruptControl KEYCNT, MEM cpu)
            {
                this.KEYCNT = KEYCNT;
                this.mem = cpu;

                try
                {
                    this.controller.PollKeysPressed();
                }
                catch (SharpDX.SharpDXException)
                {
                    this.controller = new KeyboardController();
                }
            }

            public override ushort Get()
            {
                ushort state = this.controller.PollKeysPressed();
                if (this.KEYCNT.IRQEnable)
                {
                    if (this.KEYCNT.IRQCondition)   // AND
                    {
                        if ((state & this.KEYCNT.Mask) == this.KEYCNT.Mask)
                            this.mem.IF.Request(Interrupt.Keypad);
                    }
                    else                            // OR
                    {
                        if ((state & this.KEYCNT.Mask) > 0)
                            this.mem.IF.Request(Interrupt.Keypad);
                    }
                }

                return (ushort)(((ushort)~state) & 0x03ff);
            }

            public override void Set(ushort value, bool setlow, bool sethigh) { }
        }

        public class cKeyInterruptControl : IORegister2
        {
            public ushort Mask
            {
                get => (ushort)(this._raw & 0x03ff);
            }

            public bool IRQEnable
            {
                get => (this._raw & 0x4000) > 0;
            }

            public bool IRQCondition
            {
                get => (this._raw & 0x8000) > 0;
            }
        }

        public cKeyInterruptControl KEYCNT = new cKeyInterruptControl();
        #endregion
    }
}
