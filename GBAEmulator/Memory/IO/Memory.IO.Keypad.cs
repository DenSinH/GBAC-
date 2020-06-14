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

        public void CheckInterrupts()
        {
            this.CheckInterrupts((ushort)~this.Get());
        }

        private void CheckInterrupts(ushort state)
        {
            // state as the argument in this method is NOT the value that is returned on read
            // rather, it is ~[the value returned on reads]
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
        }

        public override ushort Get()
        {
            ushort state = (ushort)(this.keyboard.PollKeysPressed() | this.xinput.PollKeysPressed());
            this.CheckInterrupts(state);

            return (ushort)(((ushort)~state) & 0x03ff);
        }

        public override void Set(ushort value, bool setlow, bool sethigh) { }
    }
    #endregion

    #region KEYCNT
    public class cKeyInterruptControl : IORegister2
    {
        private MEM mem;
        public cKeyInterruptControl(MEM mem)
        {
            this.mem = mem;
        }

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

        public override void Set(ushort value, bool setlow, bool sethigh)
        {
            base.Set(value, setlow, sethigh);

            // check for keypad interrupts on writes
            // probably never generally used, but AGS does...
            if (this.IRQEnable)
            {
                this.mem.IORAM.KEYINPUT.CheckInterrupts();
            }
        }
    }
    #endregion
}
