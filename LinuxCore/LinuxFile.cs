using System;
using System.IO;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.File;

namespace LinuxCore;

/// <summary>
/// Provides file operations over a Linux file descriptor.
/// </summary>
/// <param name="descriptor">The descriptor to wrap. It is not duplicated.</param>
/// <param name="ownsDescriptor">
/// Whether disposal closes <paramref name="descriptor"/>. When <see langword="false"/>, the external
/// owner must keep the descriptor open and prevent concurrent closure while this object is in use.
/// </param>
public unsafe class LinuxFile(FileDescriptor descriptor, bool ownsDescriptor = true)
    : FileObject(descriptor, ownsDescriptor)
{
    private bool _immutableCached;

    public LinuxFile(string path, LinuxFileFlags flags, LinuxFileMode mode = LinuxFileMode.None)
        : this(open(path, NativeLinuxFileFlags.ToNative(flags | LinuxFileFlags.LargeFile | LinuxFileFlags.CloseOnExec), mode).ThrowIfError())
    {
    }

    public long Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Stat(out var stat);
            return checked((long)stat.stx_size);
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
    public void ReadExactly(Span<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            var read = Read(buffer);
            if (read == 0)
                throw new EndOfStreamException();
            buffer = buffer[read..];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* ptr = buffer)
            return (int)base.Write(ptr, (nuint)buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteExactly(ReadOnlySpan<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            var written = Write(buffer);
            if (written == 0)
                throw new EndOfStreamException();
            buffer = buffer[written..];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureImmutableCached()
    {
        if (_immutableCached)
            return;
        Stat(out var stat);
        DeviceId = ((ulong)(stat.stx_dev_major & 0x00000fffU) << 8)
                 | ((ulong)(stat.stx_dev_major & 0xfffff000U) << 32)
                 | (stat.stx_dev_minor & 0x000000ffU)
                 | ((ulong)(stat.stx_dev_minor & 0xffffff00U) << 12);
        INode = stat.stx_ino;
        _immutableCached = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stat(out statx buf) => statx_fd(Descriptor, out buf).ThrowIfError();
}