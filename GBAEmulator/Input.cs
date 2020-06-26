using System;
using System.Windows.Forms;
using SharpDX.XInput;


namespace GBAEmulator
{
    public interface Controller
    {
        public abstract ushort PollKeysPressed();
    }

    public class XInputController : Controller
    {
        private SharpDX.XInput.Controller controller;
        private volatile ushort ControllerState;  // updated in different thread as well
        private const int JoystickThreshold = 1 << 14;  // 2 ** 14

        public XInputController()
        {
            this.controller = new SharpDX.XInput.Controller(UserIndex.One);
        }

        public virtual bool UpdateState()
        {
            try
            {
                Gamepad gamepad = controller.GetState().Gamepad;
                int ButtonState = (int)gamepad.Buttons;
                int JoystickX = gamepad.LeftThumbX;
                int JoystickY = gamepad.LeftThumbY;

                ushort _A, _B, _Start, _Select, _Up, _Down, _Left, _Right, _R, _L;

                _A = (ushort)(((ButtonState & (int)GamepadButtonFlags.A) > 0) ? 0b00_0000_0001 : 0);
                _B = (ushort)(((ButtonState & ((int)GamepadButtonFlags.B |
                                                (int)GamepadButtonFlags.X)) > 0) ? 0b00_0000_0010 : 0);
                _Select = (ushort)(((ButtonState & (int)GamepadButtonFlags.Back) > 0) ? 0b00_0000_0100 : 0);
                _Start = (ushort)(((ButtonState & (int)GamepadButtonFlags.Start) > 0) ? 0b00_0000_1000 : 0);
                _Right = (ushort)(((ButtonState & (int)GamepadButtonFlags.DPadRight) > 0) ||
                                                             JoystickX > JoystickThreshold ? 0b00_0001_0000 : 0);
                _Left = (ushort)(((ButtonState & (int)GamepadButtonFlags.DPadLeft) > 0) ||
                                                             JoystickX < -JoystickThreshold ? 0b00_0010_0000 : 0);
                _Up = (ushort)(((ButtonState & (int)GamepadButtonFlags.DPadUp) > 0) ||
                                                             JoystickY > JoystickThreshold ? 0b00_0100_0000 : 0);
                _Down = (ushort)(((ButtonState & (int)GamepadButtonFlags.DPadDown) > 0) ||
                                                             JoystickY < -JoystickThreshold ? 0b00_1000_0000 : 0);
                _R = (ushort)(((ButtonState & ((int)GamepadButtonFlags.RightShoulder)) > 0) ? 0b01_0000_0000 : 0);
                _L = (ushort)(((ButtonState & ((int)GamepadButtonFlags.LeftShoulder)) > 0) ? 0b10_0000_0000 : 0);

                ControllerState = (ushort)(_A | _B | _Start | _Select | _Up | _Down | _Left | _Right | _R | _L);
                return true;
            }
            catch (SharpDX.SharpDXException)
            {
                // otherwise our keys will be stuck on disconnection...
                ControllerState = 0;
                return false;
            }
        }

        public ushort PollKeysPressed()
        {
            return ControllerState;
        }
    }

    public class NoXInputController : XInputController
    {
        public NoXInputController() { }

        public override bool UpdateState() { return false; }
    }

    public class KeyboardController : Controller
    {
        private Keys A, B, Start, Select, Up, Down, Left, Right, R, L;
        private volatile ushort KeyboardState;  // update functions called from the visual thread

        public ushort PollKeysPressed()
        {
            return KeyboardState;
        }

        public KeyboardController()
        {
            // Can be used to map keys to other keys later
            A =      Keys.Z;
            B =      Keys.X;
            L =      Keys.ShiftKey;
            R =      Keys.C;
            Start =  Keys.A;
            Select = Keys.S;
            Up =     Keys.Up;
            Down =   Keys.Down;
            Left =   Keys.Left;
            Right =  Keys.Right;
        }

        private ushort KeyMask(Keys KeyPressed)
        {
            // switch cases don't allow dynamic keys
            // todo: dict
            if      (KeyPressed == A)       return 0b00_0000_0001;
            else if (KeyPressed == B)       return 0b00_0000_0010;
            else if (KeyPressed == Select)  return 0b00_0000_0100;
            else if (KeyPressed == Start)   return 0b00_0000_1000;
            else if (KeyPressed == Right)   return 0b00_0001_0000;
            else if (KeyPressed == Left)    return 0b00_0010_0000;
            else if (KeyPressed == Up)      return 0b00_0100_0000;
            else if (KeyPressed == Down)    return 0b00_1000_0000;
            else if (KeyPressed == R)       return 0b01_0000_0000;
            else if (KeyPressed == L)       return 0b10_0000_0000;
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
