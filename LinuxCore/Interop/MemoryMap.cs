using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class MemoryMap
{
    public const int PROT_NONE  = 0x0;
    public const int PROT_READ  = 0x1;
    public const int PROT_WRITE = 0x2;
    public const int PROT_EXEC  = 0x4;

    public const int MAP_SHARED          = 0x01;
    public const int MAP_PRIVATE         = 0x02;
    public const int MAP_SHARED_VALIDATE = 0x03;
    public const int MAP_DROPPABLE       = 0x08;

    public const int MAP_FIXED           = 0x10;
    public const int MAP_ANONYMOUS       = 0x20;

    public const int MAP_GROWSDOWN       = 0x0100;
    public const int MAP_LOCKED          = 0x2000;
    public const int MAP_NORESERVE       = 0x4000;

    public const int MAP_POPULATE        = 0x0008000;
    public const int MAP_NONBLOCK        = 0x0010000;
    public const int MAP_HUGETLB         = 0x0040000;
    public const int MAP_FIXED_NOREPLACE = 0x0100000;
    public const int MAP_UNINITIALIZED   = 0x4000000;
    
    private const int MAP_HUGE_SHIFT      = 26;
    public const int MAP_HUGE_2M         = 21 << MAP_HUGE_SHIFT;
    public const int MAP_HUGE_1G         = 30 << MAP_HUGE_SHIFT;

    public const int MS_SYNC             = 0x0;
    public const int MS_ASYNC            = 0x1;
    public const int MS_INVALIDATE       = 0x2;

    // void *mmap(void *addr, size_t length, int prot, int flags, int fd, off_t offset);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "mmap")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nint> mmap(void* addr, nuint length, LinuxMemoryProtection prot, LinuxMemoryMapFlags flags, FileDescriptor fd, long offset);

    // int munmap(void *addr, size_t length);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "munmap")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult munmap(void* addr, nuint length);

    // int msync(void *addr, size_t length, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "msync")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult msync(void* addr, nuint length, int flags);
}