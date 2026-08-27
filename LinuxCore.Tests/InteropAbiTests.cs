using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class InteropAbiTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessActionsAlignmentProbe
    {
        public byte Prefix;
        public Process.posix_spawn_file_actions_t Actions;
    }

    [TestMethod]
    public void Native_Word_Widths_Match_Current_Platform_Headers()
    {
        Assert.AreEqual(IntPtr.Size, CScript.EvaluateInt32("sizeof(long)"));
        Assert.AreEqual(UIntPtr.Size, CScript.EvaluateInt32("sizeof(unsigned long)"));
    }

    [TestMethod]
    public void Poll_Query_Layout_Matches_Native_Pollfd()
    {
        const string header = "poll.h";
        NativeConstantAssert.SizeMatches<Poll.pollfd>(header);
        NativeConstantAssert.OffsetMatches<Poll.pollfd>(nameof(Poll.pollfd.fd), header);
        NativeConstantAssert.OffsetMatches<Poll.pollfd>(nameof(Poll.pollfd.events), header);
        NativeConstantAssert.OffsetMatches<Poll.pollfd>(nameof(Poll.pollfd.revents), header);
        Assert.AreEqual(Unsafe.SizeOf<Poll.pollfd>(), Unsafe.SizeOf<LinuxPoll.Query>());
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.fd)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.Descriptor)));
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.events)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.Events)));
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.revents)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.ReturnedEvents)));
    }

    [TestMethod]
    public void Process_Interop_Matches_Current_Platform_Headers()
    {
        var expectedActionsSize = 72 + IntPtr.Size;
        Assert.AreEqual(expectedActionsSize, Unsafe.SizeOf<Process.posix_spawn_file_actions_t>());
        Assert.AreEqual(IntPtr.Size, Marshal.OffsetOf<ProcessActionsAlignmentProbe>(nameof(ProcessActionsAlignmentProbe.Actions)));
        Assert.AreEqual(expectedActionsSize, CScript.EvaluateInt32("sizeof(posix_spawn_file_actions_t)", "spawn.h"));
        Assert.AreEqual(IntPtr.Size, CScript.EvaluateInt32("_Alignof(posix_spawn_file_actions_t)", "spawn.h"));
        Assert.AreEqual(File.F_DUPFD_CLOEXEC, CScript.EvaluateInt32("F_DUPFD_CLOEXEC", "fcntl.h"));
    }

    [TestMethod]
    public void Statx_Layout_Matches_Current_Platform_Headers()
    {
        const string header = "linux/stat.h";
        NativeConstantAssert.SizeMatches<File.statx>(header);
        NativeConstantAssert.SizeMatches<File.statx_timestamp>(header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_mask), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_blksize), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_attributes), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_nlink), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_uid), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_gid), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_mode), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_ino), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_size), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_blocks), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_attributes_mask), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_atime), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_btime), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_ctime), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_mtime), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_rdev_major), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_rdev_minor), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_dev_major), header);
        NativeConstantAssert.OffsetMatches<File.statx>(nameof(File.statx.stx_dev_minor), header);
    }

    [TestMethod]
    public void ResourceLimit_Layout_Matches_Current_Platform_Headers()
    {
        const string header = "sys/resource.h";
        Assert.AreEqual(sizeof(ulong), CScript.EvaluateInt32("sizeof(rlim_t)", header));
        NativeConstantAssert.SizeMatches<Resource.rlimit>(header);
    }

    [TestMethod]
    public void Time64_Layout_Matches_Current_Platform_Headers()
    {
        if (NativeAbi.Is64Bit)
            return;

        const string header = "time.h";
        Assert.AreEqual(CScript.EvaluateInt32("sizeof(struct timespec)", header), Unsafe.SizeOf<Time.timespec64>());
        Assert.AreEqual(CScript.EvaluateNInt("offsetof(struct timespec, tv_sec)", header), Marshal.OffsetOf<Time.timespec64>(nameof(Time.timespec64.tv_sec)));
        Assert.AreEqual(CScript.EvaluateNInt("offsetof(struct timespec, tv_nsec)", header), Marshal.OffsetOf<Time.timespec64>(nameof(Time.timespec64.tv_nsec)));
        Assert.AreEqual(sizeof(int), CScript.EvaluateInt32("sizeof(((struct timespec*)0)->tv_nsec)", header));
    }
}