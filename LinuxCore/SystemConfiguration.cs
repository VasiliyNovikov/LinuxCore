using System.Runtime.CompilerServices;

using static LinuxCore.Interop.SysConf;

namespace LinuxCore;

public static class SystemConfiguration
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Get(SysConfName name) => sysconf(name).ThrowIfError();
}