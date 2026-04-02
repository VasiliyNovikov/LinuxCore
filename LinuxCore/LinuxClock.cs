using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.Time;

namespace LinuxCore;

public static class LinuxClock
{
    /// <summary>
    /// Gets the current time from the monotonic clock, which is not affected by system time changes.
    /// </summary>
    public static long MonotonicNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_MONOTONIC);
    }

    /// <summary>
    /// Gets the current time from the monotonic clock, which is not affected by system time changes.
    /// </summary>
    public static TimeSpan Monotonic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClock(CLOCK_MONOTONIC);
    }

    /// <summary>
    /// Gets the current wall-clock time as a <see cref="DateTimeOffset"/> (UTC) from <c>CLOCK_REALTIME</c>.
    /// </summary>
    public static DateTimeOffset Realtime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DateTimeOffset.UnixEpoch + GetClock(CLOCK_REALTIME);
    }

    /// <summary>
    /// Gets the current wall-clock time as nanoseconds since the Unix epoch from <c>CLOCK_REALTIME</c>.
    /// </summary>
    public static long RealtimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_REALTIME);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan GetClock(int clockId) => TimeSpan.FromTicks(GetClockNanoseconds(clockId) / TimeSpan.NanosecondsPerTick);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe long GetClockNanoseconds(int clockId)
    {
        Unsafe.SkipInit(out timespec time);
        clock_gettime(clockId, &time); // Deliberately not checking for errors as CLOCK_MONOTONIC and CLOCK_REALTIME should always be supported
        return time.tv_sec * 1_000_000_000L + time.tv_nsec;
    }
}