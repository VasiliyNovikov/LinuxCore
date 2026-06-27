using System.Runtime.CompilerServices;

using static LinuxCore.Interop.File;
using static LinuxCore.Interop.MemFd;

namespace LinuxCore;

/// <summary>
/// Provides access to an anonymous Linux memory-backed file created via <c>memfd_create(2)</c>.
/// </summary>
public sealed class LinuxMemoryFile : LinuxFile
{
    /// <summary>
    /// Wraps an existing file descriptor as a <see cref="LinuxMemoryFile"/>.
    /// </summary>
    /// <param name="descriptor">The file descriptor to wrap.</param>
    /// <param name="ownsDescriptor">
    /// When <see langword="true"/> (default), the descriptor is closed when this instance is disposed.
    /// </param>
    public LinuxMemoryFile(FileDescriptor descriptor, bool ownsDescriptor = true)
        : base(descriptor, ownsDescriptor)
    {
    }

    /// <summary>
    /// Creates a new anonymous memory file with the given <paramref name="name"/> and optional <paramref name="flags"/>.
    /// <see cref="LinuxMemoryFileFlags.CloseOnExec"/> is always added.
    /// </summary>
    /// <param name="name">A human-readable label used in <c>/proc/self/fd</c> and error messages (max 249 bytes).</param>
    /// <param name="flags">Optional flags such as <see cref="LinuxMemoryFileFlags.AllowSealing"/>.</param>
    /// <exception cref="LinuxException">The underlying <c>memfd_create(2)</c> call failed.</exception>
    public LinuxMemoryFile(string name, LinuxMemoryFileFlags flags = LinuxMemoryFileFlags.None)
        : base(memfd_create(name, flags | LinuxMemoryFileFlags.CloseOnExec).ThrowIfError())
    {
    }

    /// <summary>
    /// Gets the set of seals currently applied to this memory file via <c>fcntl(F_GET_SEALS)</c>.
    /// </summary>
    public LinuxMemoryFileSeals Seals
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (LinuxMemoryFileSeals)FileControl(F_GET_SEALS);
    }

    /// <summary>
    /// Applies one or more <paramref name="seals"/> to this memory file via <c>fcntl(F_ADD_SEALS)</c>.
    /// The file must have been created with <see cref="LinuxMemoryFileFlags.AllowSealing"/>.
    /// </summary>
    /// <exception cref="LinuxException">
    /// Thrown with <see cref="LinuxErrorNumber.OperationNotPermitted"/> if sealing is not allowed.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSeals(LinuxMemoryFileSeals seals) => FileControl(F_ADD_SEALS, (int)seals);
}