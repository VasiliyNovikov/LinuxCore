using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Errno
{
    // int * __errno_location(void);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "__errno_location")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxErrorNumber* __errno_location();

    // char *strerror(int errnum);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "strerror")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial byte* strerror(LinuxErrorNumber errnum);
}