using System.Runtime.CompilerServices;

namespace LinuxCore.Interop;

internal static unsafe class MemFd
{
    public const uint MFD_CLOEXEC       = 0x0001;
    public const uint MFD_ALLOW_SEALING = 0x0002;
    public const uint MFD_HUGETLB       = 0x0004;

    // int memfd_create(const char *name, unsigned int flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static LinuxResult<FileDescriptor> memfd_create(string name, LinuxMemoryFileFlags flags)
    {
        LinuxResult<FileDescriptor> result;
        var error = LinuxErrorNumber.OK;
        using (var nameScope = new NativeStringScope(name, stackalloc byte[NativeStringScope.BufferSize]))
        {
            result = SystemCall.NonBlocking.Invoke<nint, LinuxMemoryFileFlags, FileDescriptor>(SystemCallTable.Current.MemFdCreate, (nint)nameScope.NativeValue, flags);
            if (result.IsError)
                error = LinuxErrorNumber.Last;
        }
        if (error != LinuxErrorNumber.OK)
            LinuxErrorNumber.Last = error;
        return result;
    }
}