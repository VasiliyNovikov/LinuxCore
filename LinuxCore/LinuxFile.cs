using System;
using System.IO;
using System.Runtime.CompilerServices;

using LinuxCore.Interop;

namespace LinuxCore;

public sealed unsafe class LinuxFile(string path, LinuxFileFlags flags, UnixFileMode mode = UnixFileMode.None)
    : FileObject(LibC.open(path, flags | LinuxFileFlags.CloseOnExec, mode).ThrowIfError())
{
    private bool _immutableCached;

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
    private void Stat(out LibC.stat buf) => LibC.fstat(Descriptor, out buf).ThrowIfError();
}