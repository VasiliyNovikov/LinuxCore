using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class InteropAbiTests
{
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
        AssertSize<Poll.pollfd>(header);
        AssertOffset<Poll.pollfd>(nameof(Poll.pollfd.fd), header);
        AssertOffset<Poll.pollfd>(nameof(Poll.pollfd.events), header);
        AssertOffset<Poll.pollfd>(nameof(Poll.pollfd.revents), header);
        Assert.AreEqual(Unsafe.SizeOf<Poll.pollfd>(), Unsafe.SizeOf<LinuxPoll.Query>());
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.fd)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.Descriptor)));
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.events)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.Events)));
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.revents)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.ReturnedEvents)));
    }

    [TestMethod]
    public void Statx_Layout_Matches_Current_Platform_Headers()
    {
        const string header = "linux/stat.h";
        AssertSize<File.statx>(header);
        AssertSize<File.statx_timestamp>(header);
        AssertOffset<File.statx>(nameof(File.statx.stx_mask), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_blksize), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_attributes), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_nlink), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_uid), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_gid), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_mode), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_ino), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_size), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_blocks), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_attributes_mask), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_atime), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_btime), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_ctime), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_mtime), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_rdev_major), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_rdev_minor), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_dev_major), header);
        AssertOffset<File.statx>(nameof(File.statx.stx_dev_minor), header);
    }

    [TestMethod]
    public void ResourceLimit_Layout_Matches_Current_Platform_Headers()
    {
        const string header = "sys/resource.h";
        Assert.AreEqual(sizeof(ulong), CScript.EvaluateInt32("sizeof(rlim_t)", header));
        AssertSize<Resource.rlimit>(header);
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

    private static void AssertSize<T>(string header) where T : unmanaged
    {
        Assert.AreEqual(CScript.EvaluateInt32($"sizeof(struct {typeof(T).Name})", header), Unsafe.SizeOf<T>());
    }

    private static void AssertOffset<T>(string fieldName, string header) where T : unmanaged
    {
        Assert.AreEqual(CScript.EvaluateNInt($"offsetof(struct {typeof(T).Name}, {fieldName})", header), Marshal.OffsetOf<T>(fieldName));
    }
}
