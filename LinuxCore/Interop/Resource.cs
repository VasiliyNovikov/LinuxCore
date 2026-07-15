using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class Resource
{
    public const int RLIMIT_CPU        = 0;
    public const int RLIMIT_FSIZE      = 1;
    public const int RLIMIT_DATA       = 2;
    public const int RLIMIT_STACK      = 3;
    public const int RLIMIT_CORE       = 4;
    public const int RLIMIT_RSS        = 5;
    public const int RLIMIT_NPROC      = 6;
    public const int RLIMIT_NOFILE     = 7;
    public const int RLIMIT_MEMLOCK    = 8;
    public const int RLIMIT_AS         = 9;
    public const int RLIMIT_LOCKS      = 10;
    public const int RLIMIT_SIGPENDING = 11;
    public const int RLIMIT_MSGQUEUE   = 12;
    public const int RLIMIT_NICE       = 13;
    public const int RLIMIT_RTPRIO     = 14;
    public const int RLIMIT_RTTIME     = 15;

    public const ulong RLIM_INFINITY = ulong.MaxValue;

    [StructLayout(LayoutKind.Sequential)]
    public struct rlimit
    {
        public ulong rlim_cur;
        public ulong rlim_max;
    }

    // int getrlimit(int resource, struct rlimit *rlim);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getrlimit")]
    private static partial int getrlimit_raw(int resource, out rlimit rlim);

    // int getrlimit64(int resource, struct rlimit64 *rlim);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getrlimit64")]
    private static partial int getrlimit64_raw(int resource, out rlimit rlim);

    public static LinuxResult getrlimit(int resource, out rlimit rlim)
    {
        return new(NativeAbi.IsArm32Glibc
            ? getrlimit64_raw(resource, out rlim)
            : getrlimit_raw(resource, out rlim));
    }

    // int setrlimit(int resource, const struct rlimit *rlim);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "setrlimit")]
    private static partial int setrlimit_raw(int resource, in rlimit rlim);

    // int setrlimit64(int resource, const struct rlimit64 *rlim);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "setrlimit64")]
    private static partial int setrlimit64_raw(int resource, in rlimit rlim);

    public static LinuxResult setrlimit(int resource, in rlimit rlim)
    {
        return new(NativeAbi.IsArm32Glibc
            ? setrlimit64_raw(resource, in rlim)
            : setrlimit_raw(resource, in rlim));
    }
}