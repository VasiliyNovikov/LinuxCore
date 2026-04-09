using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Time
{
    public const int CLOCK_REALTIME           = 0;
    public const int CLOCK_MONOTONIC          = 1;
    public const int CLOCK_PROCESS_CPUTIME_ID = 2;
    public const int CLOCK_THREAD_CPUTIME_ID  = 3;
    public const int CLOCK_BOOTTIME           = 7;

    public const int TIMER_ABSTIME = 1;

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct timespec
    {
        public readonly long tv_sec;  // seconds
        public readonly long tv_nsec; // nanoseconds

        public timespec(long sec, long nsec)
        {
            tv_sec  = sec;
            tv_nsec = nsec;
        }
    }

    // int clock_gettime(clockid_t clockid, struct timespec *tp);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "clock_gettime")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult clock_gettime(int clockid, timespec* tp);

    // int clock_nanosleep(clockid_t clockid, int flags, const struct timespec *request, struct timespec *remain);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "clock_nanosleep")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int clock_nanosleep(int clockid, int flags, timespec* request, timespec* remain);
}