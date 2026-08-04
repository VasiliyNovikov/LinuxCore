using System.Runtime.CompilerServices;

namespace LinuxCore;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct SystemCallNumber(nint value)
{
    internal nint Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => value;
    }
}