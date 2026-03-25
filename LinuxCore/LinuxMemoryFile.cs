using System.Runtime.CompilerServices;

using static LinuxCore.Interop.File;
using static LinuxCore.Interop.MemFd;

namespace LinuxCore;

public sealed class LinuxMemoryFile : LinuxFile
{
    public LinuxMemoryFile(FileDescriptor descriptor, bool ownsDescriptor = true)
        : base(descriptor, ownsDescriptor)
    {
    }

    public LinuxMemoryFile(string name, LinuxMemoryFileFlags flags = LinuxMemoryFileFlags.None)
        : base(memfd_create(name, flags | LinuxMemoryFileFlags.CloseOnExec).ThrowIfError())
    {
    }

    public LinuxMemoryFileSeals Seals
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (LinuxMemoryFileSeals)FileControl(F_GET_SEALS);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSeals(LinuxMemoryFileSeals seals) => FileControl(F_ADD_SEALS, (int)seals);
}