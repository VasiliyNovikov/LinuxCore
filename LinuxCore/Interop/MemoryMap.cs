using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe class MemoryMap
{
    private const long Mmap2Unit = 4096;

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

    private const int MAP_HUGE_SHIFT     = 26;
    public const int MAP_HUGE_2M         = 21 << MAP_HUGE_SHIFT;
    public const int MAP_HUGE_1G         = 30 << MAP_HUGE_SHIFT;

    public const int MS_ASYNC            = 0x1;
    public const int MS_INVALIDATE       = 0x2;
    public const int MS_SYNC             = 0x4;

    // void *mmap(void *addr, size_t length, int prot, int flags, int fd, off_t offset);
    // void *mmap2(void *addr, size_t length, int prot, int flags, int fd, unsigned long pgoffset);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nint> mmap(void* addr, nuint length, LinuxMemoryProtection prot, int flags, FileDescriptor fd, long offset)
    {
        if (NativeAbi.Is64Bit)
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.S390x)
            {
                switch (NativeAbi.LibCImplementation)
                {
                    case LibCImplementation.Glibc:
                        // On s390x, the kernel ABI accepts a pointer to a six-native-word argument block instead of six scalar arguments.
                        var args = stackalloc nint[6] { (nint)addr, (nint)length, (int)prot, flags, fd.Value, (nint)offset };
                        return SystemCall.Invoke<nint, nint>(SystemCallTable.Current.Mmap, (nint)args);
                    case LibCImplementation.Musl:
                        break;
                    default:
                        throw new PlatformNotSupportedException("Unknown s390x libc mmap syscall ABI.");
                }
            }
            return SystemCall.Invoke<nint, nuint, LinuxMemoryProtection, int, FileDescriptor, long, nint>(SystemCallTable.Current.Mmap, (nint)addr, length, prot, flags, fd, offset);
        }

        // __NR_mmap2 offset argument is an unsigned count of 4096-byte units.
        if (offset >= 0 && (offset & (Mmap2Unit - 1)) == 0 && (ulong)offset / Mmap2Unit <= uint.MaxValue)
            return SystemCall.Invoke<nint, nuint, LinuxMemoryProtection, int, FileDescriptor, uint, nint>(SystemCallTable.Current.Mmap2, (nint)addr, length, prot, flags, fd, (uint)(offset / Mmap2Unit));

        LinuxErrorNumber.Last = LinuxErrorNumber.InvalidArgument;
        return new(-1);
    }

    // int munmap(void *addr, size_t length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult munmap(void* addr, nuint length) => SystemCall.NonBlocking.Invoke(SystemCallTable.Current.Munmap, (nint)addr, length);

    // int msync(void *addr, size_t length, int flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult msync(void* addr, nuint length, int flags) => SystemCall.Invoke(SystemCallTable.Current.Msync, (nint)addr, length, flags);
}