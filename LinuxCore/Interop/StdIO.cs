using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class StdIO
{
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "vsnprintf")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial int vsnprintf(byte* str, nuint size, byte* format, void* ap);
}