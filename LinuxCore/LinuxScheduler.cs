using System;

using LinuxCore.Interop;

namespace LinuxCore;

public static class LinuxScheduler
{
    public enum Policy
    {
        Other = LibC.SCHED_OTHER,
        Fifo = LibC.SCHED_FIFO,
        RoundRobin = LibC.SCHED_RR,
        Batch = LibC.SCHED_BATCH,
        Idle = LibC.SCHED_IDLE,
        Deadline = LibC.SCHED_DEADLINE
    }

    public static void Set(int pid, Policy policy, int priority)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pid);
        if (priority is < 0 or > 99)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between 0 and 99.");
        LibC.sched_setscheduler(pid, (int)policy, new LibC.sched_param { sched_priority = priority }).ThrowIfError();
    }

    public static void Set(Policy policy, int priority) => Set(Environment.ProcessId, policy, priority);
}