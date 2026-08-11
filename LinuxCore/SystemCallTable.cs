using System;
using System.Runtime.InteropServices;

namespace LinuxCore;

public abstract class SystemCallTable
{
    public static readonly SystemCallTable Current = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86 or Architecture.Arm or Architecture.Armv6 or Architecture.S390x or Architecture.Ppc64le => new Legacy(),
        Architecture.X64                                                                                         => new X64(),
        Architecture.Arm64 or Architecture.LoongArch64 or Architecture.RiscV64                                   => new Generic(),
        _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
    };

    public static SystemCallNumber PidFdOpen => new(434); // __NR_pidfd_open

    public abstract SystemCallNumber SchedGetScheduler { get; } // __NR_sched_getscheduler
    public abstract SystemCallNumber SchedSetScheduler { get; } // __NR_sched_setscheduler
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