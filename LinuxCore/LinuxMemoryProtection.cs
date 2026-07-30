using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

/// <summary>
/// Memory protection flags for <c>mmap(2)</c> mappings, controlling the allowed access on the mapped region.
/// </summary>
[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryProtection
{
    None    = PROT_NONE,
    Read    = PROT_READ,
    Write   = PROT_WRITE,
    Execute = PROT_EXEC
}