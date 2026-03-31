using System.Runtime.CompilerServices;

using static LinuxCore.Interop.SysConf;

namespace LinuxCore;

public static class SystemConfiguration
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Get(SysConfName name)
    {
        LinuxErrorNumber.Last = LinuxErrorNumber.OK;
        var result = sysconf(name);
        if (result == -1)
        {
            var error = LinuxErrorNumber.Last;
            if (error != LinuxErrorNumber.OK)
                throw new LinuxException(error);
        }
        return result;
    }
}