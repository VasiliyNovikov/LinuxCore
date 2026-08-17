using System.Runtime.CompilerServices;

namespace LinuxCore.Interop;

internal static class EventFd
{
    public const int EFD_SEMAPHORE = 0x00001; // Semaphore semantics for eventfd
    public const int EFD_NONBLOCK  = 0x00800; // Set non-blocking mode
    public const int EFD_CLOEXEC   = 0x80000; // Set close-on-exec flag

    // int eventfd2(unsigned int initval, int flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> eventfd(uint initval, int flags) => SystemCall.NonBlocking.Invoke<uint, int, FileDescriptor>(SystemCallTable.Current.EventFd2, initval, flags);
}