using System;
using System.Diagnostics.CodeAnalysis;

namespace LinuxCore;

[SuppressMessage("Style", "IDE0055:Fix formatting")]
[Flags]
public enum LinuxSocketType
{
    Stream      = 1,                         // SOCK_STREAM
    Datagram    = 2,                         // SOCK_DGRAM
    Raw         = 3,                         // SOCK_RAW
    RDM         = 4,                         // SOCK_RDM
    SeqPacket   = 5,                         // SOCK_SEQPACKET
    NonBlocking = LinuxFileFlags.NonBlock,   // SOCK_NONBLOCK
    CloseOnExec = LinuxFileFlags.CloseOnExec // SOCK_CLOEXEC
}