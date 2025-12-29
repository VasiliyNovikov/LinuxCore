using System.Runtime.CompilerServices;

namespace LinuxCore;

public sealed class LinuxEvent(bool isSet = false)
    : LinuxEventBase(isSet ? 1u : 0u, 0)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set() => WriteOne();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait() => Read();
}