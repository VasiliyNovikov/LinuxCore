using System.Runtime.CompilerServices;

using static LinuxCore.Interop.SysConf;

namespace LinuxCore;

/// <summary>
/// Provides access to Linux <c>sysconf(3)</c> values exposed by the running system.
/// </summary>
public static class SystemConfiguration
{
    /// <summary>
    /// Gets the requested system configuration value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Get(SystemConfigurationName name)
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