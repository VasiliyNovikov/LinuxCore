using System;
using System.Buffers;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

public abstract unsafe class LinuxMemoryMapBase : NativeObject
{
    private readonly byte* _address;
    private readonly int _length;

    private protected Span<byte> UnsafeSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return new(_address, _length);
        }
    }

    private protected Memory<byte> UnsafeMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return field;
        }
    }

    public ReadOnlySpan<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeSpan;
    }

    public ReadOnlyMemory<byte> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeMemory;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected LinuxMemoryMapBase(FileDescriptor descriptor, int length, LinuxMemoryProtection protection = LinuxMemoryProtection.Read | LinuxMemoryProtection.Write, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Shared, long offset = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        var manager = new MappedMemoryManager(this);
        var address = mmap(null, (nuint)length, protection, flags, descriptor, offset).ThrowIfError();
        _address = (byte*)address;
        _length = length;

        try
        {
            UnsafeMemory = manager.Memory;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    protected LinuxMemoryMapBase(int length, LinuxMemoryProtection protection = LinuxMemoryProtection.Read | LinuxMemoryProtection.Write, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Private)
        : this(Unsafe.BitCast<int, FileDescriptor>(-1), length, protection, flags | LinuxMemoryMapFlags.Anonymous)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sync(LinuxMemoryMapSync sync = LinuxMemoryMapSync.Sync)
    {
        ThrowIfDisposed();
        msync(_address, (nuint)_length, (int)sync).ThrowIfError();
    }

    protected override void ReleaseUnmanagedResources() => munmap(_address, (nuint)_length);

    private sealed class MappedMemoryManager(LinuxMemoryMapBase owner) : MemoryManager<byte>
    {
        public override Span<byte> GetSpan() => owner.UnsafeSpan;

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            owner.ThrowIfDisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(elementIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(elementIndex, owner._length);

            return new MemoryHandle(owner._address + elementIndex, default, this);
        }

        public override void Unpin()
        {
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}