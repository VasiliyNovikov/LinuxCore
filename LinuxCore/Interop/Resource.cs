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
    public static partial LinuxResult getrlimit(int resource, out rlimit rlim);

    // int setrlimit(int resource, const struct rlimit *rlim);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "setrlimit")]
    public static partial LinuxResult setrlimit(int resource, in rlimit rlim);
}