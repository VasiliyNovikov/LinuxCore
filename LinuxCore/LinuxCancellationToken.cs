using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LinuxCore;

public sealed class LinuxCancellationToken : IDisposable
{
    private const LinuxPoll.Event NativePollEvent = LinuxPoll.Event.Readable;
    public static LinuxCancellationToken None => new(CancellationToken.None);

    private readonly CancellationToken _cancellationToken;
    private readonly LinuxEvent? _event;
    private readonly CancellationTokenRegistration? _cancellationRegistration;

    public LinuxCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        if (cancellationToken.CanBeCanceled)
        {
            _event = new();
            _cancellationRegistration = cancellationToken.Register(() => _event.Set());
        }
    }

    public void Dispose()
    {
        _cancellationRegistration?.Dispose();
        _event?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfCancellationRequested() => _cancellationToken.ThrowIfCancellationRequested();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(ReadOnlySpan<IFileObject> objects, ReadOnlySpan<LinuxPoll.Event> events)
    {
        var objectCount = objects.Length;
        Span<LinuxPoll.Query> queries = stackalloc LinuxPoll.Query[objectCount + 1];
        for (var i = 0; i < objectCount; ++i)
            queries[i] = new(objects[i].Descriptor, events[i]);
        if (_event is null)
            queries = queries[..objectCount];
        else
            queries[objectCount] = new(_event.Descriptor, NativePollEvent);

        if (LinuxPoll.Wait(queries, Timeout.Infinite))
        {
            if (_event is not null && (queries[objectCount].ReturnedEvents & NativePollEvent) == NativePollEvent)
                _cancellationToken.ThrowIfCancellationRequested();
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(IFileObject @object, LinuxPoll.Event events) => Wait([@object], [events]);
}