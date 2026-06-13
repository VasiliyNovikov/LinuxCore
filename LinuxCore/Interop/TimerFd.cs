using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class TimerFd
{
    public const int TFD_NONBLOCK = 0x00800; // O_NONBLOCK - open non-blocking
    public const int TFD_CLOEXEC  = 0x80000; // O_CLOEXEC - close-on-exec

    // Settable bit in timerfd_settime flags
    public const int TFD_TIMER_ABSTIME = 1 << 0; // Interpret new_value as absolute time

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct itimerspec(Time.timespec value, Time.timespec interval)
    {
        public readonly Time.timespec it_interval = interval; // Interval for periodic timer (0 = one-shot)
        public readonly Time.timespec it_value    = value;    // Initial expiration time

        public static readonly itimerspec Disarmed;
    }

    // int timerfd_create(clockid_t clockid, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "timerfd_create")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<FileDescriptor> timerfd_create(int clockid, int flags);

    // int timerfd_settime(int fd, int flags, const struct itimerspec *new_value, struct itimerspec *old_value);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "timerfd_settime")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult timerfd_settime(FileDescriptor fd, int flags, in itimerspec new_value, itimerspec* old_value);

    // int timerfd_gettime(int fd, struct itimerspec *curr_value);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "timerfd_gettime")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult timerfd_gettime(FileDescriptor fd, out itimerspec curr_value);
}
