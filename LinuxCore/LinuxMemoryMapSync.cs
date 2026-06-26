using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryMapSync
{
    Sync       = MS_SYNC,
    Async      = MS_ASYNC,
    Invalidate = MS_INVALIDATE
}