using System;
using System.Runtime.CompilerServices;
using System.Threading;

using LinuxCore.Interop;

namespace LinuxCore;

/// <summary>
/// Provides an <c>eventfd</c>-backed semaphore that batches increments in managed state
/// and only toggles kernel readiness when transitioning between zero and non-zero counts.
/// </summary>
public sealed class LinuxSemaphoreSlim(uint initialValue = 0)
    : LinuxEventBase(initialValue == 0 ? 0u : 1u, EventFd.EFD_SEMAPHORE)
{
    private ulong _count = initialValue;

    /// <summary>
    /// Increments the semaphore by one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Increment()
    {
        if (Interlocked.Increment(ref _count) == 1)
            WriteOne();
    }

    /// <summary>
    /// Adds the specified number of tokens to the semaphore.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(uint value)
    {
        if (value > 0 && Interlocked.Add(ref _count, value) == value)
            WriteOne();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryDecrement()
    {
        ulong count;
        while ((count = Volatile.Read(ref _count)) > 0)
        {
            if (Interlocked.CompareExchange(ref _count, count - 1, count) == count)
            {
                if (count == 1)
                    Read();
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint TryRemove(uint value)
    {
        if (value == 0)
            return 0;
        ulong count;
        while ((count = Volatile.Read(ref _count)) > 0)
        {
            var toRemove = (uint)Math.Min(count, value);
            if (Interlocked.CompareExchange(ref _count, count - toRemove, count) == count)
            {
                if (count == toRemove)
                    Read();
                return toRemove;
            }
        }
        return 0;
    }

    /// <summary>
    /// Removes one token from the semaphore, blocking until one becomes available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decrement()
    {
        while (!TryDecrement())
            LinuxPoll.Wait(Descriptor, LinuxPoll.Event.Readable, Timeout.Infinite);
    }

    /// <summary>
    /// Removes up to <paramref name="value"/> tokens, blocking until they become available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(uint value)
    {
        var remaining = value;
        while ((remaining -= TryRemove(remaining)) > 0)
            LinuxPoll.Wait(Descriptor, LinuxPoll.Event.Readable, Timeout.Infinite);
    }
}
