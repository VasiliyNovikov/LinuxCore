using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryProtection
{
    None    = PROT_NONE,
    Read    = PROT_READ,
    Write   = PROT_WRITE,
    Execute = PROT_EXEC
}