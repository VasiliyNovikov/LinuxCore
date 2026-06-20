using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.File;

namespace LinuxCore;

public unsafe class LinuxFile(FileDescriptor descriptor, bool ownsDescriptor = true)
    : FileObject(descriptor, ownsDescriptor)
{
    private bool _immutableCached;

    /// <summary>
    /// Opens the file at <paramref name="path"/> with the given <paramref name="flags"/> and optional <paramref name="mode"/>.
    /// <see cref="LinuxFileFlags.CloseOnExec"/> is always added; the descriptor is owned by this instance.
    /// </summary>
    /// <param name="path">Path to the file to open.</param>
    /// <param name="flags">Access mode and open-time options (e.g. <see cref="LinuxFileFlags.ReadOnly"/>).</param>
    /// <param name="mode">Permission bits applied when a new file is created (e.g. with <see cref="LinuxFileFlags.Create"/>).</param>
    /// <exception cref="LinuxException">The underlying <c>open(2)</c> call failed.</exception>
    public LinuxFile(string path, LinuxFileFlags flags, LinuxFileMode mode = LinuxFileMode.None)
        : this(open(path, flags | LinuxFileFlags.CloseOnExec, mode).ThrowIfError())
    {
    }

    public long Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Stat(out var stat);
            return stat.st_size;
        }
    }

    public ulong DeviceId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            EnsureImmutableCached();
            return field;
        }
        private set;
    }

    public ulong INode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            EnsureImmutableCached();
            return field;
        }
        private set;
    }

    public long Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Seek(0, LinuxSeekOrigin.Current);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Seek(value, LinuxSeekOrigin.Begin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long Seek(long offset, LinuxSeekOrigin origin) => lseek(Descriptor, offset, origin).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(Span<byte> buffer)
    {
        fixed (byte* ptr = buffer)
            return (int)base.Read(ptr, (nuint)buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* ptr = buffer)
            return (int)base.Write(ptr, (nuint)buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureImmutableCached()
    {
        if (_immutableCached)
            return;
        Stat(out var stat);
        DeviceId = stat.st_dev;
        INode = stat.st_ino;
        _immutableCached = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stat(out stat buf) => fstat(Descriptor, out buf).ThrowIfError();
}