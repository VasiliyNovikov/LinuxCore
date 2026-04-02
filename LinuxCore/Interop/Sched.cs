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
    public static LinuxResult<long> sched_setscheduler(int pid, int policy, in sched_param param)
    {
        fixed (sched_param* p = &param)
            return SysCall.syscall_noblock(SysCallNumber.Instance.SchedSetScheduler, pid, policy, p);
    }

    // int sched_getscheduler(pid_t pid);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<long> sched_getscheduler(int pid) => SysCall.syscall_noblock(SysCallNumber.Instance.SchedGetScheduler, pid);

    // int sched_getparam(pid_t pid, struct sched_param *param);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<long> sched_getparam(int pid, out sched_param param)
    {
        fixed (sched_param* p = &param)
            return SysCall.syscall_noblock(SysCallNumber.Instance.SchedGetParam, pid, p);
    }
}