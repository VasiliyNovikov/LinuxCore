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

    /// <summary>
    /// Gets the system uptime including time spent suspended from <c>CLOCK_BOOTTIME</c>.
    /// </summary>
    public static TimeSpan BootTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClock(CLOCK_BOOTTIME);
    }

    /// <summary>
    /// Gets the system uptime including time spent suspended as nanoseconds from <c>CLOCK_BOOTTIME</c>.
    /// </summary>
    public static long BootTimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_BOOTTIME);
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
    /// Gets the CPU time consumed by the current process as nanoseconds from <c>CLOCK_PROCESS_CPUTIME_ID</c>.
    /// </summary>
    public static long ProcessCpuTimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_PROCESS_CPUTIME_ID);
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
    /// Gets the CPU time consumed by the calling thread as nanoseconds from <c>CLOCK_THREAD_CPUTIME_ID</c>.
    /// </summary>
    public static long ThreadCpuTimeNanoseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetClockNanoseconds(CLOCK_THREAD_CPUTIME_ID);
    }

    /// <summary>
    /// Sleeps for the specified duration using <c>clock_nanosleep(CLOCK_MONOTONIC)</c>.
    /// More precise than <see cref="System.Threading.Thread.Sleep(TimeSpan)"/>; automatically retried on signal interruption.
    /// </summary>
    public static void Sleep(TimeSpan duration)
    {
        var totalNs = (long)duration.TotalNanoseconds;
        if (totalNs <= 0)
            return;
        unsafe
        {
            var request = new timespec(totalNs / 1_000_000_000L, totalNs % 1_000_000_000L);
            timespec remain;
            while (true)
            {
                var error = clock_nanosleep(CLOCK_MONOTONIC, 0, &request, &remain);
                if (error == 0)
                    break;
                if (error != (int)LinuxErrorNumber.InterruptedSystemCall)
                    throw new LinuxException((LinuxErrorNumber)error);
                request = remain;
            }
        }
    }

    /// <summary>
    /// Sleeps until the specified absolute UTC timestamp using <c>clock_nanosleep(CLOCK_REALTIME, TIMER_ABSTIME)</c>.
    /// Automatically retried on signal interruption. Returns immediately if the timestamp is in the past.
    /// </summary>
    public static void SleepUntil(DateTimeOffset timestamp)
    {
        var ns = (timestamp - DateTimeOffset.UnixEpoch).Ticks * TimeSpan.NanosecondsPerTick;
        unsafe
        {
            var request = new timespec(ns / 1_000_000_000L, ns % 1_000_000_000L);
            while (true)
            {
                var error = clock_nanosleep(CLOCK_REALTIME, TIMER_ABSTIME, &request, null);
                if (error == 0)
                    break;
                if (error != (int)LinuxErrorNumber.InterruptedSystemCall)
                    throw new LinuxException((LinuxErrorNumber)error);
            }
        }
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