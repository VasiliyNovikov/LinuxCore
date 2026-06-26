using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore;

[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct SystemCallNumber(nint value)
{
    private readonly nint _value = value;
}