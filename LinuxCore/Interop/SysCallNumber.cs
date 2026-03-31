using System;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal abstract class SysCallNumber
{
    public static readonly SysCallNumber Instance = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64   => new X64(),
        Architecture.Arm64 => new Arm64(),
        _                  => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
    };

    public abstract long SchedGetScheduler { get; }
    public abstract long SchedSetScheduler { get; }
    public abstract long SchedGetParam     { get; }

    private sealed class X64 : SysCallNumber
    {
        public override long SchedGetScheduler => 145;
        public override long SchedSetScheduler => 144;
        public override long SchedGetParam     => 143;
    }

    private sealed class Arm64 : SysCallNumber
    {
        public override long SchedGetScheduler => 120;
        public override long SchedSetScheduler => 119;
        public override long SchedGetParam     => 121;
    }
}