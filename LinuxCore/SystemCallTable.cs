using System;
using System.Runtime.InteropServices;

namespace LinuxCore;

public abstract class SystemCallTable
{
    public static readonly SystemCallTable Current = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86         => new X86(),
        Architecture.X64         => new X64(),
        Architecture.Arm         => new Arm(),
        Architecture.Armv6       => new Arm(),
        Architecture.Arm64       => new Generic(),
        Architecture.S390x       => new S390x(),
        Architecture.LoongArch64 => new Generic(),
        Architecture.Ppc64le     => new Ppc64le(),
        Architecture.RiscV64     => new Generic(),
        _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
    };

    public static SystemCallNumber PidFdOpen => new(434); // __NR_pidfd_open

    public abstract SystemCallNumber SchedGetScheduler { get; } // __NR_sched_getscheduler
    public abstract SystemCallNumber SchedSetScheduler { get; } // __NR_sched_setscheduler
    public abstract SystemCallNumber SchedGetParam     { get; } // __NR_sched_getparam

    public abstract SystemCallNumber OpenAt { get; } // __NR_openat
    public abstract SystemCallNumber Close  { get; } // __NR_close
    public abstract SystemCallNumber Dup    { get; } // __NR_dup
    public abstract SystemCallNumber Read   { get; } // __NR_read
    public abstract SystemCallNumber Write  { get; } // __NR_write
    public abstract SystemCallNumber Ioctl  { get; } // __NR_ioctl
    public abstract SystemCallNumber Fcntl  { get; } // __NR_fcntl or __NR_fcntl64
    public abstract SystemCallNumber Statx  { get; } // __NR_statx
    public abstract SystemCallNumber Lseek  { get; } // __NR_lseek
    public virtual SystemCallNumber Llseek  => throw new NotImplementedException(); // __NR__llseek

    private abstract class Legacy : SystemCallTable
    {
        public override SystemCallNumber SchedGetScheduler => new(157);
        public override SystemCallNumber SchedSetScheduler => new(156);
        public override SystemCallNumber SchedGetParam     => new(155);

        public override SystemCallNumber Close => new(6);
        public override SystemCallNumber Dup   => new(41);
        public override SystemCallNumber Read  => new(3);
        public override SystemCallNumber Write => new(4);
        public override SystemCallNumber Ioctl => new(54);
        public override SystemCallNumber Lseek => new(19);
    }

    private abstract class Legacy32 : Legacy
    {
        public override SystemCallNumber Fcntl  => new(221);
        public override SystemCallNumber Llseek => new(140);
    }

    private sealed class X86 : Legacy32
    {
        public override SystemCallNumber OpenAt => new(295);
        public override SystemCallNumber Statx  => new(383);
    }

    private sealed class Arm : Legacy32
    {
        public override SystemCallNumber OpenAt => new(322);
        public override SystemCallNumber Statx  => new(397);
    }

    private sealed class X64 : SystemCallTable
    {
        public override SystemCallNumber SchedGetScheduler => new(145);
        public override SystemCallNumber SchedSetScheduler => new(144);
        public override SystemCallNumber SchedGetParam     => new(143);

        public override SystemCallNumber OpenAt => new(257);
        public override SystemCallNumber Close  => new(3);
        public override SystemCallNumber Dup    => new(32);
        public override SystemCallNumber Read   => new(0);
        public override SystemCallNumber Write  => new(1);
        public override SystemCallNumber Ioctl  => new(16);
        public override SystemCallNumber Fcntl  => new(72);
        public override SystemCallNumber Statx  => new(332);
        public override SystemCallNumber Lseek  => new(8);
    }

    private abstract class Legacy64 : Legacy
    {
        public override SystemCallNumber Fcntl => new(55);
    }

    private sealed class S390x : Legacy64
    {
        public override SystemCallNumber OpenAt => new(288);
        public override SystemCallNumber Statx  => new(379);
    }

    private sealed class Ppc64le : Legacy64
    {
        public override SystemCallNumber OpenAt => new(286);
        public override SystemCallNumber Statx  => new(383);
    }

    private sealed class Generic : SystemCallTable
    {
        public override SystemCallNumber SchedGetScheduler => new(120);
        public override SystemCallNumber SchedSetScheduler => new(119);
        public override SystemCallNumber SchedGetParam     => new(121);

        public override SystemCallNumber OpenAt => new(56);
        public override SystemCallNumber Close  => new(57);
        public override SystemCallNumber Dup    => new(23);
        public override SystemCallNumber Read   => new(63);
        public override SystemCallNumber Write  => new(64);
        public override SystemCallNumber Ioctl  => new(29);
        public override SystemCallNumber Fcntl  => new(25);
        public override SystemCallNumber Statx  => new(291);
        public override SystemCallNumber Lseek  => new(62);
    }
}