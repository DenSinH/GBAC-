using System;

namespace GBAEmulator.CPU
{
    partial class CPU
    {
        /*
            3.9.10 Exception priorities
        When multiple exceptions arise at the same time, a fixed priority system determines
        the order in which they are handled:

        Highest priority:
        1 Reset
        2 Data abort
        3 FIQ
        4 IRQ
        5 Prefetch abort

        Lowest priority:
        6 Undefined Instruction, Software interrupt.

            Not all exceptions can occur at once:
        Undefined Instruction and Software Interrupt are mutually exclusive, since they each
        correspond to particular (non-overlapping) decodings of the current instruction.
        If a data abort occurs at the same time as a FIQ, and FIQs are enabled (ie the CPSR’s
        F flag is clear), ARM7TDMI enters the data abort handler and then immediately
        proceeds to the FIQ vector. A normal return from FIQ will cause the data abort handler
        to resume execution. Placing data abort at a higher priority than FIQ is necessary to
        ensure that the transfer error does not escape detection. The time for this exception
        entry should be added to worst-case FIQ latency calculations.
        */
        uint ResetVector = 0x0;
        uint UndefVector = 0x4;
        uint SWIVector = 0x8;
        uint AbortPrefetchVector = 0xc;
        uint AbortDataVector = 0x10;
        uint ReservedVector = 0x14;
        uint IRQVector = 0x18;
        uint FIQVector = 0x1c;
    }
}
