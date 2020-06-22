using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace GBAEmulator.Memory.GPIO
{
    public class RTC : IGPIOChip
    {
        /*
         stat2       control     (1-byte)
         datetime    datetime    (7-byte)
         time        time        (3-byte)
         stat1       force reset (0-byte)
         clkadjust   force irq   (0-byte)
        */

        [Conditional("DEBUG")]
        public void Log(string message)
        {
            Console.WriteLine($"[RTC] {message}");
        }

        private enum Reg : byte
        {
            command     = 255,
            control     = 1,
            datetime    = 2,
            time        = 3,
            force_reset = 0,
            force_irq   = 6
        }

        static byte ReverseByte(byte b)
        {
            b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
            b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
            b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
            return b;
        }

        private int WriteBitCounter;
        private ulong WriteBuffer;
        private const int BitCountAfterCommand = 2 + 8;
        private Reg Param = Reg.command;
        private int ReadBitCounter;
        private ulong ReadBuffer;
        private byte CurrentReadBit;
        private bool Reading;

        private byte SCK_CS;
        private byte ControlRegister = 0x40;  // default value

        private static byte DEC2BCD(byte b)
        {
            byte MSB = (byte)((b / 10) << 4);
            return (byte)(MSB | (b % 10));
        }

        private uint TimeRegister
        {
            get
            {
                DateTime Now = DateTime.Now;
                uint value = 0;
                value |= DEC2BCD((byte)Now.Second);
                value <<= 8;

                value |= DEC2BCD((byte)Now.Minute);
                value <<= 8;

                value |= DEC2BCD((byte)(((ControlRegister & 0x40) > 0) ? (Now.Hour) : (Now.Hour % 12)));
                return value;
            }
        }

        private ulong DateTimeRegister
        {
            get
            {
                DateTime Now = DateTime.Now;

                ulong value = 0;
                value |= DEC2BCD((byte)Now.Second);
                value <<= 8;

                value |= DEC2BCD((byte)Now.Minute);
                value <<= 8;

                value |= DEC2BCD((byte)(((ControlRegister & 0x40) > 0) ? (Now.Hour) : (Now.Hour % 12)));
                value <<= 8;

                value |= DEC2BCD((byte)Now.DayOfWeek);
                value <<= 8;

                value |= DEC2BCD((byte)Now.Day);
                value <<= 8;

                value |= DEC2BCD((byte)Now.Month);
                value <<= 8;

                value |= DEC2BCD((byte)(Now.Year % 100));
                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitReadBuffer(Reg register)
        {
            // Initialize readbuffer (after Read command)
            switch (register)
            {
                case Reg.control:
                    ReadBuffer = ControlRegister;
                    ReadBitCounter = 8;
                    break;
                case Reg.time:
                    ReadBuffer = TimeRegister;
                    this.Log($"Set read buffer to {ReadBuffer:x6}");
                    ReadBitCounter = 24;
                    break;
                case Reg.datetime:
                    ReadBuffer = DateTimeRegister;
                    this.Log($"Set read buffer to {ReadBuffer:x14}");
                    ReadBitCounter = 56;
                    break;
                default:
                    // other RTC registers are unused (always 0xff)
                    ReadBuffer = 0xff;
                    ReadBitCounter = 8;
                    break;
            }
        }

        public byte Read()
        {
            this.Log($"Read {CurrentReadBit}");
            return (byte)(SCK_CS | (CurrentReadBit << 1));
        }

        /*
         GBATek:
         Chipselect and Command/Parameter Sequence:
            Init CS=LOW and /SCK=HIGH, and wait at least 1us
            Switch CS=HIGH, and wait at least 1us
            Send the Command byte (see bit-transfer below)
            Send/receive Parameter byte(s) associated with the command (see below)
            Switch CS to LOW

        Bit transfer (repeat 8 times per cmd/param byte) (bits transferred LSB first):
          Output /SCK=LOW and SIO=databit (when writing), then wait at least 5us
          Output /SCK=HIGH, wait at least 5us, then read SIO=databit (when reading)
          In either direction, data is output on (or immediately after) falling edge.
         */
        public void Write(byte value)
        {
            if (!Reading)
            {
                // ====================================== Write mode ==========================================
                switch (++WriteBitCounter)
                {
                    case 1:
                        // Init CS=LOW and /SCK=HIGH, and wait at least 1us
                        if ((value & 0x05) != 0x01)
                        {
                            Console.Error.WriteLine($"RTC Error: Expected 0b001/0b011, got {value:x2}");
                            WriteBitCounter = 0;
                        }
                        break;
                    case 2:
                        // Switch CS=HIGH, and wait at least 1us
                        if ((value & 0x05) != 0x05)
                        {
                            // games will write 1 to RTC many times in a row before writing 5 (setting CS=HIGH)
                            if ((value & 0x01) != 1)
                                Console.Error.WriteLine($"RTC Error: Expected 0b101/0b111, got {value:x2}");
                            // undo writebit increment
                            WriteBitCounter--;
                        }
                        break;
                    default:
                        byte DataBit = (byte)((value & 0x02) >> 1);
                        if ((value & 0x01) == 1)
                        {
                            WriteBuffer <<= 1;
                            WriteBuffer |= DataBit;
                            this.Log($"Buffered {DataBit}");
                        }
                        else
                        {
                            // undo BitCount increase
                            WriteBitCounter--;
                        }

                        switch (Param)
                        {
                            case Reg.command:
                                // Send the Command byte (see bit-transfer below)
                                // 1 byte
                                if (WriteBitCounter == BitCountAfterCommand)
                                {
                                    byte Command;
                                    // command might be sent forward, reverse it in this case
                                    if ((WriteBuffer & 0x0f) == 0b0000_0110)
                                    {
                                        // FWD
                                        Command = (byte)(ReverseByte((byte)WriteBuffer) & 0x0f);
                                    }
                                    else
                                    {
                                        // REV
                                        Command = (byte)(WriteBuffer & 0x0f);
                                    }

                                    Param = (Reg)(Command >> 1);
                                    Reading = (Command & 1) > 0;

                                    this.Log($"Read: {Reading} with {Param}");
                                    if (Param == Reg.force_reset || Param == Reg.force_irq)
                                    {
                                        // immediate commands (0 byte registers)
                                        // reset to initial mode
                                        this.Log("Doing " + Param);
                                        Param = Reg.command;  // await next command
                                        WriteBitCounter = 0;
                                        Reading = false;
                                    } 
                                    else if (Reading)
                                    {
                                        // we might be in reading mode
                                        InitReadBuffer(Param);
                                        Param = Reg.command;  // await next command
                                        WriteBitCounter = 0;
                                    }
                                }
                                break;
                            case Reg.control:
                                // 1 byte register 
                                if (WriteBitCounter == BitCountAfterCommand + 8)
                                {
                                    // do write and return to initial state
                                    ControlRegister = ReverseByte((byte)WriteBuffer);
                                    this.Log($"Set control to {ControlRegister:x2}");
                                    Param = Reg.command;
                                    WriteBitCounter = 0;
                                }
                                break;
                            default:
                                /*
                                 I am assuming no game will write to any other register but command/control
                                (or force reset/irq, but we won't get here then)
                                So no game would write to time/datetime
                                If this is the case, we just reset
                                 */
                                Console.Error.WriteLine($"GPIO Error, write to {Param} attempted");
                                WriteBitCounter = 0;
                                Param = Reg.command;
                                break;
                        }
                        break;
                }
            }
            else
            {
                // ====================================== Read mode ==========================================
                if ((value & 1) == 1)
                {
                    // /SCK set
                    CurrentReadBit = (byte)(ReadBuffer & 1);
                    ReadBuffer >>= 1;
                    // done reading
                    if (--ReadBitCounter == 0)
                    {
                        Reading = false;
                        if (ReadBuffer != 0)
                        {
                            Console.Error.WriteLine("RTC: Readbuffer not emptied");
                        }
                        ReadBuffer = 0;
                    }
                }
            }

            SCK_CS = (byte)(value & 0x5);
        }
            
    }
}
