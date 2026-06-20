using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

namespace LinuxCore;

[StructLayout(LayoutKind.Sequential)]
public readonly struct FileDescriptor : IEquatable<FileDescriptor>, IEqualityOperators<FileDescriptor, FileDescriptor, bool>
{
    private readonly int _fd;

    /// <summary>
    /// Closes the file descriptor. Does nothing if this is an invalid (negative) descriptor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close()
    {
        if (_fd >= 0)
            File.close(this);
    }

    /// <summary>
    /// Duplicates this file descriptor via <c>dup(2)</c> and returns the new descriptor.
    /// The caller is responsible for closing the returned descriptor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FileDescriptor Clone() => File.dup(this).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(FileDescriptor other) => _fd == other._fd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is FileDescriptor other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _fd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(FileDescriptor left, FileDescriptor right) => left._fd == right._fd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(FileDescriptor left, FileDescriptor right) => left._fd != right._fd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => _fd.ToString(CultureInfo.InvariantCulture);
}