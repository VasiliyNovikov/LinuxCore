using System;
using System.Diagnostics.CodeAnalysis;

using LinuxCore.Interop;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryMapSync
{
    Sync       = MemoryMap.MS_SYNC,
    Async      = MemoryMap.MS_ASYNC,
    Invalidate = MemoryMap.MS_INVALIDATE
}