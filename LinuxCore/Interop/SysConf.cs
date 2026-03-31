using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class SysConf
{


    // long sysconf(int name);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sysconf")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<long> sysconf(SysConfName name);
}