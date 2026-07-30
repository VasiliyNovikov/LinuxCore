using System;
using System.Buffers;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.MemoryMap;

namespace LinuxCore;

/// <summary>
/// Base class for memory-mapped regions created via <c>mmap(2)</c>.
/// Derived classes provide read-only (<see cref="LinuxReadOnlyMemoryMap"/>) and
/// read-write (<see cref="LinuxMemoryMap"/>) views.
/// </summary>
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

    /// <summary>Gets a read-only view of the mapped region.</summary>
    public ReadOnlySpan<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeSpan;
    }

    /// <summary>Gets a read-only <see cref="System.Memory{T}"/> view of the mapped region.</summary>
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

    /// <summary>Flushes changes in the mapped region back to the underlying file via <c>msync(2)</c>.</summary>
    /// <param name="sync">Specifies whether to flush synchronously, asynchronously, or with invalidation.</param>
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