using System.Runtime.CompilerServices;

using LinuxCore.Interop;

namespace LinuxCore;

public sealed class LinuxSemaphore(uint initialValue = 0)
    : LinuxEventBase(initialValue, LibC.EFD_SEMAPHORE)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Increment() => WriteOne();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decrement() => Read();
}