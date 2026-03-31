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

    /// <summary>
    /// Gets the current wall-clock time as a <see cref="DateTimeOffset"/> (UTC) from <c>CLOCK_REALTIME</c>.
    /// </summary>
    public static DateTimeOffset Realtime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            GetRealtime(out var time);
            return DateTimeOffset.UnixEpoch.AddTicks(time.tv_sec * TimeSpan.TicksPerSecond + time.tv_nsec / TimeSpan.NanosecondsPerTick);
        }
    }

    /// <summary>
    /// Gets the current wall-clock time as nanoseconds since the Unix epoch from <c>CLOCK_REALTIME</c>.
    /// </summary>
    public static long RealtimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            GetRealtime(out var time);
            return time.tv_sec * 1_000_000_000L + time.tv_nsec;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetMonotonic(out timespec time) => clock_gettime(CLOCK_MONOTONIC, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetRealtime(out timespec time) => clock_gettime(CLOCK_REALTIME, out time);
}
