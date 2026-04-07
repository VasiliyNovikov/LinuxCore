using System;

using static LinuxCore.Interop.Sched;

namespace LinuxCore;

/// <summary>
/// Provides access to Linux process scheduling policies and priorities.
/// </summary>
public static class LinuxScheduler
{
    /// <summary>
    /// Linux scheduler policies understood by <c>sched_setscheduler(2)</c>.
    /// </summary>
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

    /// <summary>
    /// Sets the scheduling policy and priority for the specified process.
    /// Elevated privileges may be required for real-time policies.
    /// </summary>
    public static void Set(int pid, Policy policy, int priority) => sched_setscheduler(pid, (int)policy, new sched_param { sched_priority = priority }).ThrowIfError();

    /// <summary>
    /// Sets the scheduling policy and priority for the current process.
    /// </summary>
    public static void Set(Policy policy, int priority) => Set(Environment.ProcessId, policy, priority);

    /// <summary>
    /// Gets the scheduling policy and priority for the specified process.
    /// </summary>
    public static (Policy Policy, int Priority) Get(int pid)
    {
        var rawPolicy = (int)sched_getscheduler(pid).ThrowIfError();
        var policy = (Policy)(rawPolicy & ~SCHED_RESET_ON_FORK);
        sched_getparam(pid, out var param).ThrowIfError();
        return (policy, param.sched_priority);
    }

    /// <summary>
    /// Gets the scheduling policy and priority for the current process.
    /// </summary>
    public static (Policy Policy, int Priority) Get() => Get(Environment.ProcessId);
}
