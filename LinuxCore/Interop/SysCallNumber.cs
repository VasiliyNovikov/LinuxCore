using System;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal abstract class SysCallNumber
{
    public static readonly SysCallNumber Instance = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86 or Architecture.Arm or Architecture.Armv6 or Architecture.S390x or Architecture.Ppc64le => new Legacy(),
        Architecture.X64                                                                                         => new X64(),
        Architecture.Arm64 or Architecture.LoongArch64 or Architecture.RiscV64                                   => new Generic(),
        _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
    };

    public abstract long SchedGetScheduler { get; } // __NR_sched_getscheduler
    public abstract long SchedSetScheduler { get; } // __NR_sched_setscheduler
    public abstract long SchedGetParam     { get; } // __NR_sched_getparam
    
    private sealed class Legacy : SysCallNumber
    {
        public override long SchedGetScheduler => 157;
        public override long SchedSetScheduler => 156;
        public override long SchedGetParam     => 155;
    }

    private sealed class X64 : SysCallNumber
    {
        public override long SchedGetScheduler => 145;
        public override long SchedSetScheduler => 144;
        public override long SchedGetParam     => 143;
    }

    private sealed class Generic : SysCallNumber
    {
        public override long SchedGetScheduler => 120;
        public override long SchedSetScheduler => 119;
        public override long SchedGetParam     => 121;
    }
}