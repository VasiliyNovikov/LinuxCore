using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.File;

namespace LinuxCore;

/// <summary>
/// File seals for <see cref="LinuxMemoryFile"/>, applied via <c>fcntl(F_ADD_SEALS)</c>.
/// Seals permanently restrict allowed operations on the memory file.
/// </summary>
[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryFileSeals
{
    None        = 0,
    Seal        = F_SEAL_SEAL,         // F_SEAL_SEAL: Prevent further seals from being added
    Shrink      = F_SEAL_SHRINK,       // F_SEAL_SHRINK: Prevent shrinking the file
    Grow        = F_SEAL_GROW,         // F_SEAL_GROW: Prevent growing the file
    Write       = F_SEAL_WRITE,        // F_SEAL_WRITE: Prevent writes to the file
    FutureWrite = F_SEAL_FUTURE_WRITE  // F_SEAL_FUTURE_WRITE: Prevent future writes and writable mappings
}