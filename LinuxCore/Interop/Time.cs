using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class Time
{
    public const int CLOCK_MONOTONIC = 1;

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct timespec
    {
        public readonly long tv_sec;  // seconds
        public readonly long tv_nsec; // nanoseconds
    }

    // int clock_gettime(clockid_t clockid, struct timespec *tp);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "clock_gettime")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult clock_gettime(int clockid, out timespec tp);
}