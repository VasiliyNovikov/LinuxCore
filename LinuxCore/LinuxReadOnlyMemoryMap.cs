namespace LinuxCore;

/// <summary>
/// A read-only memory-mapped region created via <c>mmap(2)</c> with <see cref="LinuxMemoryProtection.Read"/>.
/// </summary>
public sealed class LinuxReadOnlyMemoryMap : LinuxMemoryMapBase
{
    /// <summary>
    /// Creates a read-only file-backed memory map.
    /// </summary>
    /// <param name="descriptor">File descriptor to map.</param>
    /// <param name="length">Number of bytes to map.</param>
    /// <param name="flags">Mapping flags; defaults to <see cref="LinuxMemoryMapFlags.Shared"/>.</param>
    /// <param name="offset">Offset within the file to start the mapping.</param>
    public LinuxReadOnlyMemoryMap(FileDescriptor descriptor, int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Shared, long offset = 0)
        : base(descriptor, length, LinuxMemoryProtection.Read, flags, offset)
    {
    }

    /// <summary>
    /// Creates an anonymous read-only private memory map (not backed by any file).
    /// </summary>
    /// <param name="length">Number of bytes to map.</param>
    /// <param name="flags">Mapping flags; defaults to <see cref="LinuxMemoryMapFlags.Private"/>.</param>
    public LinuxReadOnlyMemoryMap(int length, LinuxMemoryMapFlags flags = LinuxMemoryMapFlags.Private)
        : base(length, LinuxMemoryProtection.Read, flags)
    {
    }
}