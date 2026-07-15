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
    /// <remarks>
    /// This instance and every object must remain strongly reachable and undisposed, and their
    /// descriptors must remain open until this method returns.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public bool Wait(ReadOnlySpan<IFileObject> objects, ReadOnlySpan<LinuxPoll.Event> events)
    {
        var objectCount = objects.Length;
        Span<LinuxPoll.Query> buffer = stackalloc LinuxPoll.Query[objectCount + 1];
        for (var i = 0; i < objectCount; ++i)
            buffer[i] = new(objects[i].Descriptor, events[i]);
        return WaitHelper(buffer);
    }

    /// <summary>
    /// Waits until the requested file object becomes ready or the wrapped token is canceled.
    /// </summary>
    /// <remarks>
    /// This instance and the object must remain strongly reachable and undisposed, and their
    /// descriptors must remain open until this method returns.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public bool Wait(IFileObject @object, LinuxPoll.Event events)
    {
        Span<LinuxPoll.Query> buffer = stackalloc LinuxPoll.Query[2];
        buffer[0] = new(@object.Descriptor, events);
        return WaitHelper(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool WaitHelper(Span<LinuxPoll.Query> buffer)
    {
        if (_event is null)
            return LinuxPoll.Wait(buffer[..^1], Timeout.Infinite);

        ref var eventQuery = ref buffer[^1];
        eventQuery = new(_event.Descriptor, NativePollEvent);
        if (!LinuxPoll.Wait(buffer, Timeout.Infinite))
            return false;

        if ((eventQuery.ReturnedEvents & NativePollEvent) == NativePollEvent)
            _cancellationToken.ThrowIfCancellationRequested();

        return true;
    }
}