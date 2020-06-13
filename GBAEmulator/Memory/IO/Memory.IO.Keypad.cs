using System;

namespace GBAEmulator.Memory.IO
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
                // attempt to read controller state
                this.xinput.UpdateState();
            }
            catch (SharpDX.SharpDXException)
            {
                // if there is no controller connected, an exception will be thrown and we can instead
                // initialize the register with only the keyboardcontroller
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
                        this.mem.IORAM.IF.Request(Interrupt.Keypad);
                }
                else                            // OR
                {
                    if ((state & this.KEYCNT.Mask) > 0)
                        this.mem.IORAM.IF.Request(Interrupt.Keypad);
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
    #endregion
}
