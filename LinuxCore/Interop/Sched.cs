using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

// Uses direct syscalls to bypass musl libc stub that returns ENOSYS.
internal static unsafe class Sched
{
    public const int SCHED_OTHER         = 0;
    public const int SCHED_FIFO          = 1;
    public const int SCHED_RR            = 2;
    public const int SCHED_BATCH         = 3;
    public const int SCHED_ISO           = 4; // reserved but not implemented yet
    public const int SCHED_IDLE          = 5;
    public const int SCHED_DEADLINE      = 6;
    public const int SCHED_EXT           = 7;
    public const int SCHED_RESET_ON_FORK = 0x40000000;

    [StructLayout(LayoutKind.Sequential)]
    public struct sched_param
    {
        public int sched_priority;
    }

    // int sched_setscheduler(pid_t pid, int policy, const struct sched_param *param);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult sched_setscheduler(int pid, int policy, in sched_param param)
    {
        fixed (sched_param* p = &param)
            return SystemCall.NonBlocking.Invoke(SystemCallTable.Current.SchedSetScheduler, pid, policy, (nint)p);
    }

    // int sched_getscheduler(pid_t pid);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<int> sched_getscheduler(int pid) => SystemCall.NonBlocking.Invoke<int, int>(SystemCallTable.Current.SchedGetScheduler, pid);

    // int sched_getparam(pid_t pid, struct sched_param *param);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult sched_getparam(int pid, out sched_param param)
    {
        fixed (sched_param* p = &param)
            return SystemCall.NonBlocking.Invoke(SystemCallTable.Current.SchedGetParam, pid, (nint)p);
    }
}