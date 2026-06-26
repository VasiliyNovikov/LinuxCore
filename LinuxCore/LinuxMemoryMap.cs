using System;
using System.Runtime.CompilerServices;

namespace LinuxCore;

public sealed class LinuxMemoryMap : LinuxMemoryMapBase
{
    public new Span<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeSpan;
    }

    public new Memory<byte> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => UnsafeMemory;
    }

    public LinuxMemoryMap(FileDescriptor descriptor, int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Shared, long offset = 0)
        : base(descriptor, length, LinuxMemoryProtection.Read | LinuxMemoryProtection.Write, flags, offset)
    {
    }

    public LinuxMemoryMap(int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Private)
        : base(length, LinuxMemoryProtection.Read | LinuxMemoryProtection.Write, flags)
    {
    }
}