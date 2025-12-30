using System;

using LinuxCore.Interop;

namespace LinuxCore;

[Flags]
public enum LinuxSocketMessageFlags
{
    None = 0,
    OutOfBand = LibC.MSG_OOB,
    Peek = LibC.MSG_PEEK,
    DontWait = LibC.MSG_DONTWAIT,
    WaitAll = LibC.MSG_WAITALL
}