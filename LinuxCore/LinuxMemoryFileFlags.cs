using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.MemFd;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryFileFlags : uint
{
    None         = 0,
    CloseOnExec  = MFD_CLOEXEC,       // MFD_CLOEXEC: Set close-on-exec for the new file descriptor
    AllowSealing = MFD_ALLOW_SEALING, // MFD_ALLOW_SEALING: Allow seals to be added to the file
    HugeTLB      = MFD_HUGETLB        // MFD_HUGETLB: Create the file in hugetlbfs
}