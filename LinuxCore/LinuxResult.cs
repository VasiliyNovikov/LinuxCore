using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore;

[StructLayout(LayoutKind.Sequential)]
public readonly struct LinuxResult
{
    private readonly int _value;

    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value == -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfError()
    {
        if (IsError)
            throw LinuxException.FromLastError();
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LinuxResult<T> where T : unmanaged
{
    private readonly T _value;

    // JIT is expected to optimize this switch away
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => sizeof(T) switch
        {
            4 => Unsafe.BitCast<T, int>(_value) == -1,
            8 => Unsafe.BitCast<T, long>(_value) == -1,
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ThrowIfError() => IsError ? throw LinuxException.FromLastError() : _value;

    public static implicit operator T(LinuxResult<T> result) => result.ThrowIfError();
}