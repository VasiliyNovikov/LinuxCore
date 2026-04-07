using System.Runtime.CompilerServices;

namespace LinuxCore;

/// <summary>
/// Provides an <c>eventfd</c>-backed one-shot synchronization primitive.
/// </summary>
public sealed class LinuxEvent(bool isSet = false)
    : LinuxEventBase(isSet ? 1u : 0u, 0)
{
    /// <summary>
    /// Signals the event and makes the descriptor readable.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set() => WriteOne();

    /// <summary>
    /// Waits until the event is signaled, then consumes the signal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait() => Read();
}
