using static LinuxCore.Interop.Resource;

namespace LinuxCore;

/// <summary>
/// Provides access to the Linux resource limit (rlimit) API.
/// </summary>
public static class LinuxResourceLimit
{
    /// <summary>
    /// Represents a resource whose limits can be queried or modified.
    /// </summary>
    public enum Resource
    {
        /// <summary>CPU time limit in seconds.</summary>
        Cpu = RLIMIT_CPU,
        /// <summary>Maximum file size in bytes.</summary>
        FileSize = RLIMIT_FSIZE,
        /// <summary>Maximum data segment size.</summary>
        Data = RLIMIT_DATA,
        /// <summary>Maximum stack size.</summary>
        Stack = RLIMIT_STACK,
        /// <summary>Maximum core file size.</summary>
        Core = RLIMIT_CORE,
        /// <summary>Maximum resident set size.</summary>
        Rss = RLIMIT_RSS,
        /// <summary>Maximum number of processes.</summary>
        NumProcesses = RLIMIT_NPROC,
        /// <summary>Maximum number of open file descriptors.</summary>
        NumOpenFiles = RLIMIT_NOFILE,
        /// <summary>Maximum locked memory.</summary>
        MemoryLock = RLIMIT_MEMLOCK,
        /// <summary>Maximum address space size.</summary>
        AddressSpace = RLIMIT_AS,
        /// <summary>Maximum number of file locks.</summary>
        Locks = RLIMIT_LOCKS,
        /// <summary>Maximum number of pending signals.</summary>
        SignalsPending = RLIMIT_SIGPENDING,
        /// <summary>Maximum bytes in POSIX message queues.</summary>
        MessageQueue = RLIMIT_MSGQUEUE,
        /// <summary>Nice priority ceiling.</summary>
        Nice = RLIMIT_NICE,
        /// <summary>Real-time scheduling priority ceiling.</summary>
        RealtimePriority = RLIMIT_RTPRIO,
        /// <summary>Timeout for real-time tasks in microseconds.</summary>
        RealtimeTimeout = RLIMIT_RTTIME
    }

    /// <summary>
    /// Value representing no limit (RLIM_INFINITY).
    /// </summary>
    public const ulong Infinity = RLIM_INFINITY;

    /// <summary>
    /// Gets the current (soft) and maximum (hard) limits for the specified resource.
    /// </summary>
    public static (ulong Soft, ulong Hard) Get(Resource resource)
    {
        getrlimit((int)resource, out var limit).ThrowIfError();
        return (limit.rlim_cur, limit.rlim_max);
    }

    /// <summary>
    /// Sets the current (soft) and maximum (hard) limits for the specified resource.
    /// </summary>
    public static void Set(Resource resource, ulong soft, ulong hard)
    {
        setrlimit((int)resource, new rlimit { rlim_cur = soft, rlim_max = hard }).ThrowIfError();
    }
}
