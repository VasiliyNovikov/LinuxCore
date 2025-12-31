using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class EventFd
{
    public const int EFD_SEMAPHORE = 0x00001; // Semaphore semantics for eventfd
    public const int EFD_NONBLOCK  = 0x00800; // Set non-blocking mode
    public const int EFD_CLOEXEC   = 0x80000; // Set close-on-exec flag

    // int eventfd(unsigned int initval, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "eventfd")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<FileDescriptor> eventfd(uint initval, int flags);
}