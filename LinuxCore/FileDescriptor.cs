using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

namespace LinuxCore;

/// <summary>
/// Represents a non-owning Linux file descriptor value.
/// </summary>
/// <remarks>
/// Copying this value neither duplicates the descriptor nor keeps its native resource open.
/// Use <see cref="Clone"/> to create an independent descriptor. A closed or stale value must
/// not be used because Linux may recycle its numeric descriptor for an unrelated resource.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct FileDescriptor : IEquatable<FileDescriptor>, IEqualityOperators<FileDescriptor, FileDescriptor, bool>
{
    private readonly int _fd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal FileDescriptor(int fd) => _fd = fd;

    internal int Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _fd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close()
    {
        if (_fd >= 0)
            File.close(this);
    }

    /// <summary>
    /// Creates an independent duplicate of this descriptor.
    /// </summary>
    /// <remarks>
    /// The caller must close the returned descriptor or transfer it to an owning wrapper.
    /// The duplicate shares the underlying open-file description, including its offset and status flags.
    /// </remarks>
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