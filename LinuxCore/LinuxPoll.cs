using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

namespace LinuxCore;

public static unsafe class LinuxPoll
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Wait(Span<Query> queries, int timeoutMilliseconds)
    {
        fixed (Query* queriesPtr = queries)
        {
            var result = LibC.poll((LibC.pollfd*)queriesPtr, (uint)queries.Length, timeoutMilliseconds);
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
    [SuppressMessage("Style", "IDE0032: Use auto property", Justification = "Struct layout")]
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
        Readable = LibC.POLLIN,
        Urgent   = LibC.POLLPRI,
        Writable = LibC.POLLOUT,
        Error    = LibC.POLLERR,
        HangUp   = LibC.POLLHUP,
        Invalid  = LibC.POLLNVAL
    }
}