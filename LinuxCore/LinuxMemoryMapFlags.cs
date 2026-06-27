using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

/// <summary>
/// Flags controlling the visibility, placement, and behaviour of <c>mmap(2)</c> mappings.
/// </summary>
[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxMemoryMapFlags
{
    None           = 0,
    Shared         = MAP_SHARED,
    Private        = MAP_PRIVATE,
    SharedValidate = MAP_SHARED_VALIDATE,
    Droppable      = MAP_DROPPABLE,

    Fixed          = MAP_FIXED,
    Anonymous      = MAP_ANONYMOUS,
    GrowsDown      = MAP_GROWSDOWN,
    Locked         = MAP_LOCKED,
    NoReserve      = MAP_NORESERVE,
    Populate       = MAP_POPULATE,
    NonBlocking    = MAP_NONBLOCK,
    FixedNoReplace = MAP_FIXED_NOREPLACE,
    Uninitialized  = MAP_UNINITIALIZED,
    HugeTLB        = MAP_HUGETLB,
    Huge2M         = MAP_HUGE_2M | MAP_HUGETLB,
    Huge1G         = MAP_HUGE_1G | MAP_HUGETLB
}