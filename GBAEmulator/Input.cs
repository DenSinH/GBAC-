using System;
using System.Windows.Input;
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

            _A = (ushort)(((state & (int)GamepadButtonFlags.A) > 0) ? 0b0000_0001 : 0);
            _B = (ushort)(((state & ((int)GamepadButtonFlags.B | (int)GamepadButtonFlags.X)) > 0) ? 0b0000_0010 : 0);
            _Select = (ushort)(((state & (int)GamepadButtonFlags.Back) > 0) ? 0b0000_0100 : 0);
            _Start = (ushort)(((state & (int)GamepadButtonFlags.Start) > 0) ? 0b0000_1000 : 0);
            _Right = (ushort)(((state & (int)GamepadButtonFlags.DPadRight) > 0) ? 0b0001_0000 : 0);
            _Left = (ushort)(((state & (int)GamepadButtonFlags.DPadLeft) > 0) ? 0b0010_0000 : 0);
            _Up = (ushort)(((state & (int)GamepadButtonFlags.DPadUp) > 0) ? 0b0100_0000 : 0);
            _Down = (ushort)(((state & (int)GamepadButtonFlags.DPadDown) > 0) ? 0b1000_0000 : 0);
            _R = (ushort)(((state & ((int)GamepadButtonFlags.RightThumb | (int)GamepadButtonFlags.RightShoulder)) > 0) ? 0b0001_0000_0000 : 0);
            _L = (ushort)(((state & ((int)GamepadButtonFlags.LeftThumb | (int)GamepadButtonFlags.LeftShoulder)) > 0) ? 0b0010_0000_0000 : 0);

            return (ushort)(_A | _B | _Start | _Select | _Up | _Down | _Left | _Right | _R | _L);
        }

    }

    public class KeyboardController : Controller
    {
        Key A, B, Start, Select, Up, Down, Left, Right, R, L;

        public override ushort PollKeysPressed()
        {
            ushort _A, _B, _Start, _Select, _Up, _Down, _Left, _Right, _R, _L;
            _A = (ushort)(Keyboard.IsKeyDown(A) ? 0b0000_0001 : 0);
            _B = (ushort)(Keyboard.IsKeyDown(B) ? 0b0000_0010 : 0);
            _Select = (ushort)(Keyboard.IsKeyDown(Select) ? 0b0000_0100 : 0);
            _Start = (ushort)(Keyboard.IsKeyDown(Start) ? 0b0000_1000 : 0);
            _Right = (ushort)(Keyboard.IsKeyDown(Right) ? 0b0001_0000 : 0);
            _Left = (ushort)(Keyboard.IsKeyDown(Left) ? 0b0010_0000 : 0);
            _Up = (ushort)(Keyboard.IsKeyDown(Up) ? 0b0100_0000 : 0);
            _Down = (ushort)(Keyboard.IsKeyDown(Down) ? 0b1000_0000 : 0);
            _R = (ushort)(Keyboard.IsKeyDown(R) ? 0b0001_0000_0000 : 0);
            _L = (ushort)(Keyboard.IsKeyDown(L) ? 0b0010_0000_0000 : 0);

            return (ushort)(_A | _B | _Start | _Select | _Up | _Down | _Left | _Right | _R | _L);
        }

        public KeyboardController()
        {
            // Can be used to map keys to other keys later
            A = Key.Z;
            B = Key.X;
            L = Key.LeftShift;
            R = Key.C;
            Start = Key.A;
            Select = Key.S;
            Up = Key.Up;
            Down = Key.Down;
            Left = Key.Left;
            Right = Key.Right;
        }

    }
}
