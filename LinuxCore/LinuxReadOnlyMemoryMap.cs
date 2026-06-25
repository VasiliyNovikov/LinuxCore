namespace LinuxCore;

public sealed class LinuxReadOnlyMemoryMap : LinuxMemoryMapBase
{
    public LinuxReadOnlyMemoryMap(FileDescriptor descriptor, int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Shared, long offset = 0)
        : base(descriptor, length, LinuxMemoryProtection.Read, flags, offset)
    {
    }

    public LinuxReadOnlyMemoryMap(int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Private)
        : base(length, LinuxMemoryProtection.Read, flags)
    {
    }
}