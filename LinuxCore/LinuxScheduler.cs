using System;

using static LinuxCore.Interop.Sched;

namespace LinuxCore;

public static class LinuxScheduler
{
    public enum Policy
    {
        Other      = SCHED_OTHER,
        FIFO       = SCHED_FIFO,
        RoundRobin = SCHED_RR,
        Batch      = SCHED_BATCH,
        ISO        = SCHED_ISO,
        Idle       = SCHED_IDLE,
        Deadline   = SCHED_DEADLINE,
        Extensible = SCHED_EXT
    }

    public static void Set(int pid, Policy policy, int priority) => sched_setscheduler(pid, (int)policy, new sched_param { sched_priority = priority }).ThrowIfError();

    public static void Set(Policy policy, int priority) => Set(Environment.ProcessId, policy, priority);

    public static (Policy Policy, int Priority) Get(int pid)
    {
        var rawPolicy = (int)sched_getscheduler(pid).ThrowIfError();
        var policy = (Policy)(rawPolicy & ~SCHED_RESET_ON_FORK);
        sched_getparam(pid, out var param).ThrowIfError();
        return (policy, param.sched_priority);
    }

    public static (Policy Policy, int Priority) Get() => Get(Environment.ProcessId);
}
