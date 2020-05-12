namespace GBAEmulator.CPU
{
    public enum State:byte
    {
        ARM = 0,
        THUMB = 1
    }

    public enum Mode:byte
    {
        User = 0b10000,
        FIQ = 0b10001,
        IRQ = 0b10010,
        Supervisor = 0b10011,
        Abort = 0b10111,
        Undefined = 0b11011,
        System = 0b11111
    }
}
