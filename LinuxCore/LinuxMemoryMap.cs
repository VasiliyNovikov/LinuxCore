using System;
using System.Runtime.CompilerServices;

namespace LinuxCore;

/// <summary>
/// A read-write memory-mapped region created via <c>mmap(2)</c>.
/// For anonymous mappings (no backing file), use <see cref="LinuxMemoryMap(int, LinuxMemoryMapFlags)"/>.
/// For file-backed mappings, use <see cref="LinuxMemoryMap(FileDescriptor, int, LinuxMemoryMapFlags, long)"/>.
/// </summary>
public sealed class LinuxMemoryMap : LinuxMemoryMapBase
{
    /// <summary>Gets a read-write view of the mapped region.</summary>
    public new Span<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeSpan;
    }

    /// <summary>Gets a read-write <see cref="System.Memory{T}"/> view of the mapped region.</summary>
    public new Memory<byte> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeMemory;
    }

    /// <summary>
    /// Creates a read-write file-backed shared memory map.
    /// </summary>
    /// <param name="descriptor">File descriptor to map.</param>
    /// <param name="length">Number of bytes to map.</param>
    /// <param name="flags">Mapping flags; defaults to <see cref="LinuxMemoryMapFlags.Shared"/>.</param>
    /// <param name="offset">Offset within the file to start the mapping.</param>
    public LinuxMemoryMap(FileDescriptor descriptor, int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Shared, long offset = 0)
        : base(descriptor, length, LinuxMemoryProtection.Read | LinuxMemoryProtection.Write, flags, offset)
    {
    }

    /// <summary>
    /// Creates an anonymous read-write private memory map (not backed by any file).
    /// </summary>
    /// <param name="length">Number of bytes to map.</param>
    /// <param name="flags">Mapping flags; defaults to <see cref="LinuxMemoryMapFlags.Private"/>.</param>
    public LinuxMemoryMap(int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Private)
        : base(length, LinuxMemoryProtection.Read | LinuxMemoryProtection.Write, flags)
    {
    }
}