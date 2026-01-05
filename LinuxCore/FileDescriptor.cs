using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

namespace LinuxCore;

[StructLayout(LayoutKind.Sequential)]
public readonly struct FileDescriptor : IDisposable
{
    private readonly int _fd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_fd >= 0)
            File.close(this).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FileDescriptor Clone() => File.dup(this).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => _fd.ToString(CultureInfo.InvariantCulture);
}