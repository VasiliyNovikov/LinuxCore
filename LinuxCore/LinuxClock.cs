using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.Time;

namespace LinuxCore;

public static class LinuxClock
{
    private const long NanosecondsPerSecond = 1_000_000_000L;

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

    /// <summary>
    /// Gets the current time from <c>CLOCK_BOOTTIME</c>, which includes time spent suspended.
    /// </summary>
    public static long BootTimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_BOOTTIME);
    }

    /// <summary>
    /// Gets the current time from <c>CLOCK_BOOTTIME</c>, which includes time spent suspended.
    /// </summary>
    public static TimeSpan BootTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClock(CLOCK_BOOTTIME);
    }

    /// <summary>
    /// Gets the CPU time consumed by the current process from <c>CLOCK_PROCESS_CPUTIME_ID</c>.
    /// </summary>
    public static long ProcessCpuTimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_PROCESS_CPUTIME_ID);
    }

    /// <summary>
    /// Gets the CPU time consumed by the current process from <c>CLOCK_PROCESS_CPUTIME_ID</c>.
    /// </summary>
    public static TimeSpan ProcessCpuTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClock(CLOCK_PROCESS_CPUTIME_ID);
    }

    /// <summary>
    /// Gets the CPU time consumed by the calling thread from <c>CLOCK_THREAD_CPUTIME_ID</c>.
    /// </summary>
    public static long ThreadCpuTimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_THREAD_CPUTIME_ID);
    }

    /// <summary>
    /// Gets the CPU time consumed by the calling thread from <c>CLOCK_THREAD_CPUTIME_ID</c>.
    /// </summary>
    public static TimeSpan ThreadCpuTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClock(CLOCK_THREAD_CPUTIME_ID);
    }

    /// <summary>
    /// Sleeps for the specified duration using <c>clock_nanosleep</c> with <c>CLOCK_MONOTONIC</c>.
    /// </summary>
    /// <param name="duration">The amount of time to sleep.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is negative.</exception>
    /// <exception cref="LinuxException">The native sleep call failed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Sleep(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);

        if (duration == TimeSpan.Zero)
            return;

        var request = ToTimespec(duration);
        Unsafe.SkipInit(out timespec remaining);

        while (true)
        {
            var error = clock_nanosleep(CLOCK_MONOTONIC, 0, &request, &remaining);
            if (error == LinuxErrorNumber.OK)
                return;

            if (error != LinuxErrorNumber.InterruptedSystemCall)
                throw new LinuxException(error);

            request = remaining;
        }
    }

    /// <summary>
    /// Sleeps until the specified UTC timestamp using <c>clock_nanosleep</c> with <c>CLOCK_REALTIME</c>.
    /// </summary>
    /// <param name="timestamp">The absolute timestamp to sleep until.</param>
    /// <exception cref="LinuxException">The native sleep call failed.</exception>
    public static unsafe void SleepUntil(DateTimeOffset timestamp)
    {
        var request = ToTimespec(timestamp - DateTimeOffset.UnixEpoch);

        while (true)
        {
            var error = clock_nanosleep(CLOCK_REALTIME, TIMER_ABSTIME, &request, null);
            if (error == LinuxErrorNumber.OK)
                return;

            if (error != LinuxErrorNumber.InterruptedSystemCall)
                throw new LinuxException(error);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan GetClock(int clockId) => TimeSpan.FromTicks(GetClockNanoseconds(clockId) / TimeSpan.NanosecondsPerTick);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe long GetClockNanoseconds(int clockId)
    {
        Unsafe.SkipInit(out timespec time);
        clock_gettime(clockId, &time).ThrowIfError();
        return time.tv_sec * NanosecondsPerSecond + time.tv_nsec;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static timespec ToTimespec(TimeSpan time)
    {
        long seconds = time.Ticks / TimeSpan.TicksPerSecond;
        long nanoseconds = time.Ticks % TimeSpan.TicksPerSecond * TimeSpan.NanosecondsPerTick;
        return new timespec(seconds, nanoseconds);
    }
}