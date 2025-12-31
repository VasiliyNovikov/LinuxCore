using System;

using static LinuxCore.Interop.Sched;

namespace LinuxCore;

public static class LinuxScheduler
{
    public enum Policy
    {
        Other = SCHED_OTHER,
        Fifo = SCHED_FIFO,
        RoundRobin = SCHED_RR,
        Batch = SCHED_BATCH,
        Idle = SCHED_IDLE,
        Deadline = SCHED_DEADLINE
    }

    public static void Set(int pid, Policy policy, int priority)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pid);
        if (priority is < 0 or > 99)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between 0 and 99.");
        sched_setscheduler(pid, (int)policy, new sched_param { sched_priority = priority }).ThrowIfError();
    }

    public static void Set(Policy policy, int priority) => Set(Environment.ProcessId, policy, priority);
}