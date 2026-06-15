using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LinuxCore;

/// <summary>
/// Bridges a managed <see cref="CancellationToken"/> into an <c>eventfd</c> so it can be
/// waited on together with Linux file descriptors via <see cref="LinuxPoll"/>.
/// </summary>
public sealed class LinuxCancellationToken : IDisposable
{
    private const LinuxPoll.Event NativePollEvent = LinuxPoll.Event.Readable;

    /// <summary>
    /// A non-cancelable token wrapper that can be reused when cancellation is not needed.
    /// </summary>
    public static LinuxCancellationToken None { get; } = new(CancellationToken.None);

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

    /// <summary>
    /// Throws <see cref="OperationCanceledException"/> if cancellation has been requested.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfCancellationRequested() => _cancellationToken.ThrowIfCancellationRequested();

    /// <summary>
    /// Waits until one of the requested file objects becomes ready or the wrapped token is canceled.
    /// </summary>
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

    /// <summary>
    /// Waits until the requested file object becomes ready or the wrapped token is canceled.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(IFileObject @object, LinuxPoll.Event events)
    {
        Span<LinuxPoll.Query> queries = stackalloc LinuxPoll.Query[2];
        queries[0] = new(@object.Descriptor, events);
        if (_event is null)
        {
            return LinuxPoll.Wait(queries[..1], Timeout.Infinite);
        }
        else
        {
            queries[1] = new(_event.Descriptor, NativePollEvent);
            if (LinuxPoll.Wait(queries, Timeout.Infinite))
            {
                if ((queries[1].ReturnedEvents & NativePollEvent) == NativePollEvent)
                    _cancellationToken.ThrowIfCancellationRequested();
                return true;
            }
            return false;
        }
    }
}
