using System.Runtime.CompilerServices;

using LinuxCore.Interop;

namespace LinuxCore;

/// <summary>
/// Provides an <c>eventfd</c>-backed counting semaphore.
/// </summary>
public sealed class LinuxSemaphore(uint initialValue = 0)
    : LinuxEventBase(initialValue, EventFd.EFD_SEMAPHORE)
{
    /// <summary>
    /// Increments the semaphore by one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Increment() => WriteOne();

    /// <summary>
    /// Decrements the semaphore by one, blocking until a token is available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decrement() => Read();
}