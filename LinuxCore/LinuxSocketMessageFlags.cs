using System;

using LinuxCore.Interop;

namespace LinuxCore;

[Flags]
public enum LinuxSocketMessageFlags
{
    None = 0,
    OutOfBand = Socket.MSG_OOB,
    Peek = Socket.MSG_PEEK,
    DontWait = Socket.MSG_DONTWAIT,
    WaitAll = Socket.MSG_WAITALL
}