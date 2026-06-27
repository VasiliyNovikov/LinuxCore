using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore;

/// <summary>
/// Identifies a Linux system call by its architecture-specific numeric identifier.
/// Obtain instances from <see cref="SystemCallTable.Current"/> rather than constructing them directly.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct SystemCallNumber(nint value)
{
    private readonly nint _value = value;
}