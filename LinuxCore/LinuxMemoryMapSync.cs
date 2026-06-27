using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

/// <summary>
/// Synchronization flags for <see cref="LinuxMemoryMapBase.Sync"/>, passed to <c>msync(2)</c>.
/// </summary>
[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryMapSync
{
    Sync       = MS_SYNC,
    Async      = MS_ASYNC,
    Invalidate = MS_INVALIDATE
}