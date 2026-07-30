using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using static LinuxCore.Interop.Poll;

namespace LinuxCore;

/// <summary>
/// Provides access to <c>poll(2)</c> for waiting on Linux file descriptors.
/// </summary>
public static unsafe class LinuxPoll
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToMilliseconds(TimeSpan timeout) =>
        (int)Math.Clamp(timeout.TotalMilliseconds, Timeout.Infinite, int.MaxValue);

    /// <summary>
    /// Waits for one or more file descriptors to become ready.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Wait(Span<Query> queries, int timeoutMilliseconds)
    {
        fixed (Query* queriesPtr = queries)
        {
            var result = poll((pollfd*)queriesPtr, (nuint)queries.Length, timeoutMilliseconds);
            if (!result.IsError)
                return result > 0;

            var error = LinuxErrorNumber.Last;
            return error == LinuxErrorNumber.InterruptedSystemCall
                ? false
                : throw new LinuxException(error);
        }
    }

    /// <summary>
    /// Waits for one or more file descriptors to become ready.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Wait(Span<Query> queries, TimeSpan timeout) => Wait(queries, ToMilliseconds(timeout));

    /// <summary>
    /// Waits for a single file descriptor to become ready and returns the resulting events.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Event? Wait(FileDescriptor descriptor, Event @event, int timeoutMilliseconds)
    {
        Span<Query> queries = [new(descriptor, @event)];
        return Wait(queries, timeoutMilliseconds)
            ? queries[0].ReturnedEvents
            : null;
    }

    /// <summary>
    /// Waits for a single file descriptor to become ready and returns the resulting events.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Event? Wait(FileDescriptor descriptor, Event @event, TimeSpan timeout) => Wait(descriptor, @event, ToMilliseconds(timeout));

    /// <summary>
    /// Represents a single file descriptor poll request.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly struct Query(FileDescriptor descriptor, Event events)
    {
        /// <summary>
        /// The descriptor being polled.
        /// </summary>
        public readonly FileDescriptor Descriptor = descriptor;

        /// <summary>
        /// The events of interest for the descriptor.
        /// </summary>
        public readonly Event Events = events;

        /// <summary>
        /// The events returned by the last poll call.
        /// </summary>
        public readonly Event ReturnedEvents;
    }

    /// <summary>
    /// Poll event flags returned by <c>poll(2)</c>.
    /// </summary>
    [Flags]
    [SuppressMessage("Style", "IDE0055:Fix formatting")]
    public enum Event : short
    {
        None = 0,
        Readable = POLLIN,
        Urgent   = POLLPRI,
        Writable = POLLOUT,
        Error    = POLLERR,
        HangUp   = POLLHUP,
        Invalid  = POLLNVAL
    }
}