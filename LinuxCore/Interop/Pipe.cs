using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Pipe
{
    // int pipe2(int pipefd[2], int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "pipe2")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult pipe2(FileDescriptor* pipefd, LinuxFileFlags flags);
}
