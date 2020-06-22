using GBAEmulator.Memory.Sections;
using System;

namespace GBAEmulator.IO
{
    #region KEYINPUT
    public class cKeyInput : IORegister2
    {
        public XInputController xinput = new XInputController();
        public KeyboardController keyboard = new KeyboardController();
        private readonly cKeyInterruptControl KEYCNT;
        private readonly cIF IF;

        public cKeyInput(cKeyInterruptControl KEYCNT, cIF IF)
        {
            this.KEYCNT = KEYCNT;
            this.IF = IF;

            // attempt to update controller state
            if (!this.xinput.UpdateState())
            {
                // if there is no controller connected, an exception will be thrown and we can instead
                // initialize the register with only the keyboardcontroller
                this.xinput = new NoXInputController();

                // todo: recognize new controller if one is plugged in
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
                        this.IF.Request(Interrupt.Keypad);
                }
                else                            // OR
                {
                    if ((state & this.KEYCNT.Mask) > 0)
                        this.IF.Request(Interrupt.Keypad);
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
        private readonly IORAMSection IO;
        public cKeyInterruptControl(IORAMSection IO)
        {
            this.IO = IO;
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
                this.IO.KEYINPUT.CheckInterrupts();
            }
        }
    }
    #endregion
}
