using System;
using System.Windows.Forms;
using SharpDX.XInput;


namespace GBAEmulator
{
    public abstract class Controller
    {
        public abstract ushort PollKeysPressed();
    }

    public class NoController : Controller
    {
        public override ushort PollKeysPressed()
        {
            return 0;
        }
    }

    public class XInputController : Controller
    {
        SharpDX.XInput.Controller controller;

        public XInputController()
        {
            this.controller = new SharpDX.XInput.Controller(UserIndex.One);
        }

        public override ushort PollKeysPressed()
        {
            int state = (int)controller.GetState().Gamepad.Buttons;
            ushort _A, _B, _Start, _Select, _Up, _Down, _Left, _Right, _R, _L;

            _A =        (ushort)(((state &  (int)GamepadButtonFlags.A) > 0)              ? 0b00_0000_0001 : 0);
            _B =        (ushort)(((state & ((int)GamepadButtonFlags.B |
                                            (int)GamepadButtonFlags.X)) > 0)            ? 0b00_0000_0010 : 0);
            _Select =   (ushort)(((state &  (int)GamepadButtonFlags.Back) > 0)           ? 0b00_0000_0100 : 0);
            _Start =    (ushort)(((state &  (int)GamepadButtonFlags.Start) > 0)          ? 0b00_0000_1000 : 0);
            _Right =    (ushort)(((state &  (int)GamepadButtonFlags.DPadRight) > 0)      ? 0b00_0001_0000 : 0);
            _Left =     (ushort)(((state &  (int)GamepadButtonFlags.DPadLeft) > 0)       ? 0b00_0010_0000 : 0);
            _Up =       (ushort)(((state &  (int)GamepadButtonFlags.DPadUp) > 0)         ? 0b00_0100_0000 : 0);
            _Down =     (ushort)(((state &  (int)GamepadButtonFlags.DPadDown) > 0)       ? 0b00_1000_0000 : 0);
            _R =        (ushort)(((state & ((int)GamepadButtonFlags.RightThumb |
                                            (int)GamepadButtonFlags.RightShoulder)) > 0)? 0b01_0000_0000 : 0);
            _L =        (ushort)(((state & ((int)GamepadButtonFlags.LeftThumb |
                                            (int)GamepadButtonFlags.LeftShoulder)) > 0) ? 0b10_0000_0000 : 0);

            return (ushort)(_A | _B | _Start | _Select | _Up | _Down | _Left | _Right | _R | _L);
        }

    }

    public class KeyboardController : Controller
    {
        private Keys A, B, Start, Select, Up, Down, Left, Right, R, L;
        private ushort KeyboardState;

        public override ushort PollKeysPressed()
        {
            return KeyboardState;
        }

        public KeyboardController()
        {
            // Can be used to map keys to other keys later
            A = Keys.Z;
            B = Keys.X;
            L = Keys.LShiftKey;
            R = Keys.C;
            Start = Keys.A;
            Select = Keys.S;
            Up = Keys.Up;
            Down = Keys.Down;
            Left = Keys.Left;
            Right = Keys.Right;
        }

        private ushort KeyMask(Keys KeyPressed)
        {
            if (KeyPressed == A) return 0b00_0000_0001;
            else if (KeyPressed == B) return 0b00_0000_0010;
            else if (KeyPressed == Select) return 0b00_0000_0100;
            else if (KeyPressed == Start) return 0b00_0000_1000;
            else if (KeyPressed == Right) return 0b00_0001_0000;
            else if (KeyPressed == Left) return 0b00_0010_0000;
            else if (KeyPressed == Up) return 0b00_0100_0000;
            else if (KeyPressed == Down) return 0b00_1000_0000;
            else if (KeyPressed == R) return 0b01_0000_0000;
            else if (KeyPressed == L) return 0b10_0000_0000;
            return 0;
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            this.KeyboardState |= this.KeyMask(e.KeyCode);
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            this.KeyboardState &= (ushort)~this.KeyMask(e.KeyCode);
        }
    }
}
