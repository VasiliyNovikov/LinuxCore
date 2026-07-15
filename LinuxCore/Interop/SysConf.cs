using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class SysConf
{
    // long sysconf(int name);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sysconf")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial nint sysconf_raw(SystemConfigurationName name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long sysconf(SystemConfigurationName name) => sysconf_raw(name);
}