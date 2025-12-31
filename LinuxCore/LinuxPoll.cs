using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static LinuxCore.Interop.Poll;

namespace LinuxCore;

public static unsafe class LinuxPoll
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Wait(Span<Query> queries, int timeoutMilliseconds)
    {
        fixed (Query* queriesPtr = queries)
        {
            var result = poll((pollfd*)queriesPtr, (uint)queries.Length, timeoutMilliseconds);
            if (!result.IsError)
                return result > 0;

            var error = LinuxErrorNumber.Last;
            return error == LinuxErrorNumber.InterruptedSystemCall
                ? false
                : throw new LinuxException(error);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Wait(Span<Query> queries, TimeSpan timeout) => Wait(queries, (int)timeout.TotalMilliseconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Event? Wait(FileDescriptor descriptor, Event @event, int timeoutMilliseconds)
    {
        Span<Query> queries = [new(descriptor, @event)];
        return Wait(queries, timeoutMilliseconds)
            ? queries[0].ReturnedEvents
            : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Event? Wait(FileDescriptor descriptor, Event @event, TimeSpan timeout) => Wait(descriptor, @event, (int)timeout.TotalMilliseconds);

    [StructLayout(LayoutKind.Sequential)]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly struct Query(FileDescriptor descriptor, Event events)
    {
        public readonly FileDescriptor Descriptor = descriptor;
        public readonly Event Events = events;
        public readonly Event ReturnedEvents;
    }

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