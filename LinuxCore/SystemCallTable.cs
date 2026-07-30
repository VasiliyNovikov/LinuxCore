using System;
using System.Runtime.InteropServices;

namespace LinuxCore;

/// <summary>
/// Provides architecture-specific Linux system call numbers.
/// Use <see cref="Current"/> to obtain the correct table for the running process architecture.
/// </summary>
public abstract class SystemCallTable
{
    /// <summary>
    /// Gets the <see cref="SystemCallTable"/> for the current process architecture.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the process architecture is not supported.
    /// </exception>
    public static readonly SystemCallTable Current = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86 or Architecture.Arm or Architecture.Armv6 or Architecture.S390x or Architecture.Ppc64le => new Legacy(),
        Architecture.X64                                                                                         => new X64(),
        Architecture.Arm64 or Architecture.LoongArch64 or Architecture.RiscV64                                   => new Generic(),
        _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
    };

    /// <summary>Gets the system call number for <c>sched_getscheduler(2)</c>.</summary>
    public abstract SystemCallNumber SchedGetScheduler { get; } // __NR_sched_getscheduler
    /// <summary>Gets the system call number for <c>sched_setscheduler(2)</c>.</summary>
    public abstract SystemCallNumber SchedSetScheduler { get; } // __NR_sched_setscheduler
    /// <summary>Gets the system call number for <c>sched_getparam(2)</c>.</summary>
    public abstract SystemCallNumber SchedGetParam     { get; } // __NR_sched_getparam

    private sealed class Legacy : SystemCallTable
    {
        public override SystemCallNumber SchedGetScheduler => new(157);
        public override SystemCallNumber SchedSetScheduler => new(156);
        public override SystemCallNumber SchedGetParam     => new(155);
    }

    private sealed class X64 : SystemCallTable
    {
        public override SystemCallNumber SchedGetScheduler => new(145);
        public override SystemCallNumber SchedSetScheduler => new(144);
        public override SystemCallNumber SchedGetParam     => new(143);
    }

    private sealed class Generic : SystemCallTable
    {
        public override SystemCallNumber SchedGetScheduler => new(120);
        public override SystemCallNumber SchedSetScheduler => new(119);
        public override SystemCallNumber SchedGetParam     => new(121);
    }
}