using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class MemFd
{
    public const uint MFD_CLOEXEC       = 0x0001;
    public const uint MFD_ALLOW_SEALING = 0x0002;
    public const uint MFD_HUGETLB       = 0x0004;

    // int memfd_create(const char *name, unsigned int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "memfd_create", StringMarshalling = StringMarshalling.Utf8)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int memfd_create_raw(string name, LinuxMemoryFileFlags flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> memfd_create(string name, LinuxMemoryFileFlags flags) => new(new(memfd_create_raw(name, flags)));
}