using System;
using System.Runtime.CompilerServices;

namespace GBAEmulator.CPU
{
    partial class ARM7TDMI
    {
        /*
         Contains some useful methods used in different places in the CPU implementation
        */

        public enum State : byte
        {
            ARM = 0,
            THUMB = 1
        }

        public enum Mode : byte
        {
            User = 0b10000,
            FIQ = 0b10001,
            IRQ = 0b10010,
            Supervisor = 0b10011,
            Abort = 0b10111,
            Undefined = 0b11011,
            System = 0b11111
        }

        private struct sRegisterList  // used in Block Data Transfer / Push, Pop instructions
        {
            // made to be very similar to a Queue<byte>
            // this is because that is what I initially used to perform this task
            // however, after implementing my own very short queue for the pipeline,
            // I realized that this may be faster

            // After all, we only enqueue and dequeue the full thing once, and it is
            // only a short lived thing

            private byte[] Registers;
            public int Count { get; private set; }
            private int Offset;

            public sRegisterList(int MaxRegisters)
            {
                this.Registers = new byte[MaxRegisters];
                this.Count = 0;
                this.Offset = 0;
            }

            public void Enqueue(byte value)
            {
                this.Registers[Offset + Count++] = value;
            }

            public byte Dequeue()
            {
                Count--;
                return this.Registers[Offset++];
            }

            public byte Peek()
            {
                return this.Registers[Offset];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Condition(byte field)  // used in ARM methods (always) and in some THUMB methods
        {
            switch (field)
            {
                case 0b0000:  // EQ
                    return Z == 1;
                case 0b0001:  // NE
                    return Z == 0;
                case 0b0010:  // CS
                    return C == 1;
                case 0b0011:  // CC
                    return C == 0;
                case 0b0100:  // MI
                    return N == 1;
                case 0b0101:  // PL
                    return N == 0;
                case 0b0110:  // VS
                    return V == 1;
                case 0b0111:  // VC
                    return V == 0;
                case 0b1000:  // HI
                    return (C == 1) && (Z == 0);
                case 0b1001:  // LS
                    return (C == 0) || (Z == 1);
                case 0b1010:  // GE
                    return N == V;
                case 0b1011:  // LT
                    return N != V;
                case 0b1100:  // GT
                    return (Z == 0) && (N == V);
                case 0b1101:  // LE
                    return (Z == 1) || (N != V);
                case 0b1110:  // AL
                    return true;
                default:
                    throw new Exception(string.Format("Condition field {0} reserved/invalid", field));
            }
        }

        // Used in most arithmetic instructions
        private uint ShiftOperand(uint Op, bool Special0Cases, byte ShiftType, byte ShiftAmount, bool SetConditions)
        {
            // Special cases for 0 shift
            if (ShiftAmount == 0 && Special0Cases)
            {
                switch (ShiftType)  // Shift type
                {
                    case 0b00:  // Logical Left
                                // No shift applied
                        break;
                    case 0b01:  // Logical Right
                                // Interpreted as LSR#32
                        ShiftAmount = 32;
                        break;
                    case 0b10:  // Arithmetic Right
                                // Interpreted as ASR#32
                        ShiftAmount = 32;
                        break;
                    case 0b11:  // Rotate Right
                                // Interpreted as RRX#1
                        byte newC = (byte)(Op & 0x01);
                        Op = (Op >> 1) | (uint)(this.C << 31);
                        if (SetConditions)
                            this.C = newC;
                        // Leave ShiftAmount = 0 so that no additional shift is applied
                        break;
                }
            }

            // We have set the shift amount accordingly above if necessary
            // if the shift amount was specified by a register that was 0 in the last byte, no shift was applied, 
            //     and the flags are not affected
            if (ShiftAmount != 0)
            {
                byte newC = this.C;
                switch (ShiftType)  // Shift type
                {
                    case 0b00:  // Logical Left
                        if (ShiftAmount > 32)
                        {
                            Op = 0;
                            newC = 0;
                        }
                        else if (ShiftAmount == 32)
                        {
                            newC = (byte)(Op & 0x01);  // bit 0
                            Op = 0;
                        }
                        else
                        {
                            newC = (byte)((Op >> (32 - ShiftAmount)) & 0x01);  // Bit (32 - ShiftAmount) of contents of Rm
                            Op <<= ShiftAmount;
                        }
                        break;
                    case 0b01:  // Logical Right
                        if (ShiftAmount < 32)
                        {
                            newC = (byte)((Op >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm
                            Op >>= ShiftAmount;
                        }
                        else if (ShiftAmount == 32)
                        {
                            newC = (byte)((Op >> 31) & 0x01);
                            Op = 0;
                        }
                        else
                        {
                            newC = 0;
                            Op = 0;
                        }
                        break;
                    case 0b10:  // Arithmetic Right
                        if (ShiftAmount < 32)
                        {
                            newC = (byte)((int)(Op >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                            Op = (uint)((int)Op >> ShiftAmount);
                        }
                        else
                        {
                            Op = (Op & 0x8000_0000) > 0 ? 0xffff_ffff : 0;
                            newC = (byte)(Op & 0x01);
                        }

                        break;
                    case 0b11:  // Rotate Right
                        ShiftAmount &= 0x1f;  // mod 32 gives same result
                        newC = (byte)((Op >> (ShiftAmount - 1)) & 0x01);  // Bit (ShiftAmount - 1) of contents of Rm, similar to LSR
                        Op = (uint)((Op >> ShiftAmount) | ((Op & ((1 << ShiftAmount) - 1)) << (32 - ShiftAmount)));
                        break;
                }

                if (SetConditions)
                {
                    C = newC;
                }
            }
            return Op;
        }

        // used in most arithmetic instructions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCVAdd(ulong Op1, ulong Op2, uint Result)
        {
            this.C = (byte)(Op1 + Op2 > 0xffff_ffff ? 1 : 0);
            this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ Op2)) >> 31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCVAddC(ulong Op1, ulong Op2, uint C, uint Result)
        {
            // for ADC
            this.C = (byte)(Op1 + Op2 + C> 0xffff_ffff ? 1 : 0);
            this.V = (byte)(((Op1 ^ Result) & (~Op1 ^ Op2)) >> 31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCVSub(ulong Op1, ulong Op2, uint Result)
        {
            // for Op1 - Op2
            this.C = (byte)(Op2 <= Op1 ? 1 : 0);
            this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCVSubC(ulong Op1, ulong Op2, uint C, uint Result)
        {
            // for Op1 - Op2
            this.C = (byte)(Op2 + 1 - C <= Op1 ? 1 : 0);
            this.V = (byte)(((Op1 ^ Op2) & (~Op2 ^ Result)) >> 31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ROR(uint Operand, byte RotateAmount)
        {
            RotateAmount &= 0x1f;
            return (uint)((Operand >> RotateAmount) |
                                           ((Operand & ((1 << RotateAmount) - 1)) << (32 - RotateAmount)));
        }
    }
}
