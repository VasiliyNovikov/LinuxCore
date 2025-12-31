using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.Time;

namespace LinuxCore;

public static class LinuxClock
{
    public static long MonotonicNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            GetMonotonic(out var time);
            return time.tv_sec * 1_000_000_000L + time.tv_nsec;
        }
    }

    public static TimeSpan Monotonic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            GetMonotonic(out var time);
            return TimeSpan.FromTicks(time.tv_sec * TimeSpan.TicksPerSecond + time.tv_nsec / TimeSpan.NanosecondsPerTick);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetMonotonic(out timespec time) => clock_gettime(CLOCK_MONOTONIC, out time);
}