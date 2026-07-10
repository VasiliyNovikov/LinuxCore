using System;
using System.Runtime.CompilerServices;
using System.Threading;

using static LinuxCore.Interop.Time;
using static LinuxCore.Interop.TimerFd;

namespace LinuxCore;

/// <summary>
/// Provides a Linux <c>timerfd</c>-backed timer that integrates with <see cref="LinuxPoll"/>.
/// </summary>
/// <remarks>
/// <para>A <see cref="LinuxTimer"/> exposes a file descriptor that becomes readable each time
/// the timer expires. It can be polled with <see cref="LinuxPoll"/> alongside other descriptors,
/// making it suitable for event-driven and I/O-multiplexing loops.</para>
/// <para>Create a timer with one of the static factory methods (<see cref="Monotonic"/>,
/// <see cref="Realtime"/>, or <see cref="BootTime"/>), arm it with <see cref="SetOneShot"/>
/// or <see cref="SetPeriodic(TimeSpan)"/>, then call <see cref="Wait()"/> or <see cref="TryWait"/> to
/// consume expirations.</para>
/// </remarks>
public sealed class LinuxTimer : FileObject
{
    /// <summary>
    /// Creates a timer backed by <c>CLOCK_MONOTONIC</c>, which is not affected by system time
    /// changes or NTP adjustments.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxTimer Monotonic() => new(CLOCK_MONOTONIC);

    /// <summary>
    /// Creates a timer backed by <c>CLOCK_REALTIME</c> (wall-clock time).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxTimer Realtime() => new(CLOCK_REALTIME);

    /// <summary>
    /// Creates a timer backed by <c>CLOCK_BOOTTIME</c>, which includes time spent suspended.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxTimer BootTime() => new(CLOCK_BOOTTIME);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LinuxTimer(int clockId)
        : base(timerfd_create(clockId, TFD_CLOEXEC).ThrowIfError())
    {
    }

    /// <summary>
    /// Arms the timer to fire once after the specified <paramref name="delay"/>.
    /// Any previously armed setting is replaced.
    /// </summary>
    /// <param name="delay">Time to wait before the timer fires. Must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is not positive.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetOneShot(TimeSpan delay)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(delay, TimeSpan.Zero);
        Arm(ToTimespec(delay), default);
    }

    /// <summary>
    /// Arms the timer to fire at regular <paramref name="period"/> intervals.
    /// The first expiration also occurs after one <paramref name="period"/>.
    /// Any previously armed setting is replaced.
    /// </summary>
    /// <param name="period">Interval between expirations. Must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is not positive.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPeriodic(TimeSpan period)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(period, TimeSpan.Zero);
        var ts = ToTimespec(period);
        Arm(ts, ts);
    }

    /// <summary>
    /// Arms the timer to fire after <paramref name="initialDelay"/> and then repeatedly
    /// at <paramref name="period"/> intervals. Any previously armed setting is replaced.
    /// </summary>
    /// <param name="initialDelay">Delay before the first expiration. Must be positive.</param>
    /// <param name="period">Interval between subsequent expirations. Must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="initialDelay"/> or <paramref name="period"/> is not positive.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPeriodic(TimeSpan initialDelay, TimeSpan period)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(initialDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(period, TimeSpan.Zero);
        Arm(ToTimespec(initialDelay), ToTimespec(period));
    }

    /// <summary>
    /// Disarms the timer, cancelling any pending expiration. Does nothing if already disarmed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Disarm() => Arm(default, default);

    /// <summary>
    /// Blocks until the timer expires and returns the number of expirations since the last read.
    /// </summary>
    /// <returns>
    /// The count of expirations that occurred since the last successful <see cref="Wait()"/> or
    /// <see cref="TryWait"/> call. Normally 1, but may be greater if the caller was slow to read.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ulong Wait()
    {
        ulong count;
        Read(&count, sizeof(ulong));
        return count;
    }

    /// <summary>
    /// Attempts to read the expiration count without blocking.
    /// </summary>
    /// <param name="expirations">
    /// When this method returns <see langword="true"/>, contains the number of expirations
    /// since the last read. Otherwise, contains 0.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the timer had expired and the count was consumed;
    /// <see langword="false"/> if the timer had not yet fired.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool TryWait(out ulong expirations)
    {
        if (LinuxPoll.Wait(Descriptor, LinuxPoll.Event.Readable, 0) is null)
        {
            expirations = 0;
            return false;
        }
        ulong count;
        Read(&count, sizeof(ulong));
        expirations = count;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void Arm(timespec value, timespec interval)
    {
        var spec = new itimerspec(value, interval);
        timerfd_settime(Descriptor, 0, in spec, null).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static timespec ToTimespec(TimeSpan time)
    {
        long seconds = time.Ticks / TimeSpan.TicksPerSecond;
        long nanoseconds = time.Ticks % TimeSpan.TicksPerSecond * TimeSpan.NanosecondsPerTick;
        return new timespec(seconds, nanoseconds);
    }
}
