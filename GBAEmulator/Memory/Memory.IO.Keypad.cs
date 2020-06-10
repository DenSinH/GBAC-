using System;

namespace GBAEmulator.Memory
{
    partial class MEM
    {
        #region KEYINPUT
        public class cKeyInput : IORegister2
        {
            public XInputController xinput = new XInputController();
            public KeyboardController keyboard = new KeyboardController();
            cKeyInterruptControl KEYCNT;
            MEM mem;

            public cKeyInput(cKeyInterruptControl KEYCNT, MEM mem)
            {
                this.KEYCNT = KEYCNT;
                this.mem = mem;

                try
                {
                    this.xinput.PollKeysPressed();
                }
                catch (SharpDX.SharpDXException)
                {
                    this.xinput = new NoXInputController();
                }
            }

            public override ushort Get()
            {
                ushort state = (ushort)(this.keyboard.PollKeysPressed() | this.xinput.PollKeysPressed());
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

        public cKeyInput KEYINPUT;

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
