using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static partial class Sched
{
    public const int SCHED_OTHER    = 0;
    public const int SCHED_FIFO     = 1;
    public const int SCHED_RR       = 2;
    public const int SCHED_BATCH    = 3;
    public const int SCHED_IDLE     = 5;
    public const int SCHED_DEADLINE = 6;

    [StructLayout(LayoutKind.Sequential)]
    public struct sched_param
    {
        public int sched_priority;
    }

    // int sched_setscheduler(pid_t pid, int policy, const struct sched_param *param);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sched_setscheduler")]
    public static partial LinuxResult sched_setscheduler(int pid, int policy, in sched_param param);
}