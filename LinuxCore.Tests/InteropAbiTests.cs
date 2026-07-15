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
        Assert.AreEqual(IntPtr.Size, CScript.EvaluateInt32("sizeof(long)", "stddef.h"));
        Assert.AreEqual(UIntPtr.Size, CScript.EvaluateInt32("sizeof(unsigned long)", "stddef.h"));
    }

    [TestMethod]
    public void Poll_Query_Layout_Matches_Native_Pollfd()
    {
        Assert.AreEqual(CScript.EvaluateInt32("sizeof(struct pollfd)", "poll.h"), Unsafe.SizeOf<LinuxPoll.Query>());
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.fd)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.Descriptor)));
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.events)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.Events)));
        Assert.AreEqual(Marshal.OffsetOf<Poll.pollfd>(nameof(Poll.pollfd.revents)), Marshal.OffsetOf<LinuxPoll.Query>(nameof(LinuxPoll.Query.ReturnedEvents)));
    }

    [TestMethod]
    public void Statx_Layout_Matches_Current_Platform_Headers()
    {
        Assert.AreEqual(CScript.EvaluateInt32("sizeof(struct statx)", "stddef.h", "linux/stat.h"), Unsafe.SizeOf<File.statx>());
        Assert.AreEqual(CScript.EvaluateInt32("sizeof(struct statx_timestamp)", "stddef.h", "linux/stat.h"), Unsafe.SizeOf<File.statx_timestamp>());
        AssertOffset(nameof(File.statx.stx_mask), 0x00);
        AssertOffset(nameof(File.statx.stx_blksize), 0x04);
        AssertOffset(nameof(File.statx.stx_attributes), 0x08);
        AssertOffset(nameof(File.statx.stx_nlink), 0x10);
        AssertOffset(nameof(File.statx.stx_uid), 0x14);
        AssertOffset(nameof(File.statx.stx_gid), 0x18);
        AssertOffset(nameof(File.statx.stx_mode), 0x1c);
        AssertOffset(nameof(File.statx.stx_ino), 0x20);
        AssertOffset(nameof(File.statx.stx_size), 0x28);
        AssertOffset(nameof(File.statx.stx_blocks), 0x30);
        AssertOffset(nameof(File.statx.stx_attributes_mask), 0x38);
        AssertOffset(nameof(File.statx.stx_atime), 0x40);
        AssertOffset(nameof(File.statx.stx_btime), 0x50);
        AssertOffset(nameof(File.statx.stx_ctime), 0x60);
        AssertOffset(nameof(File.statx.stx_mtime), 0x70);
        AssertOffset(nameof(File.statx.stx_rdev_major), 0x80);
        AssertOffset(nameof(File.statx.stx_rdev_minor), 0x84);
        AssertOffset(nameof(File.statx.stx_dev_major), 0x88);
        AssertOffset(nameof(File.statx.stx_dev_minor), 0x8c);
        AssertOffset(nameof(File.statx.stx_mnt_id), 0x90);
        AssertOffset(nameof(File.statx.stx_dio_mem_align), 0x98);
        AssertOffset(nameof(File.statx.stx_dio_offset_align), 0x9c);
        AssertOffset(nameof(File.statx.stx_subvol), 0xa0);
        AssertOffset(nameof(File.statx.stx_atomic_write_unit_min), 0xa8);
        AssertOffset(nameof(File.statx.stx_atomic_write_unit_max), 0xac);
        AssertOffset(nameof(File.statx.stx_atomic_write_segments_max), 0xb0);
        AssertOffset(nameof(File.statx.stx_dio_read_offset_align), 0xb4);
        AssertOffset(nameof(File.statx.stx_atomic_write_unit_max_opt), 0xb8);

        static void AssertOffset(string fieldName, int expected)
        {
            Assert.AreEqual(expected, Marshal.OffsetOf<File.statx>(fieldName).ToInt32(), fieldName);
        }
    }

    [TestMethod]
    public void ResourceLimit_Layout_Matches_Current_Platform_Headers()
    {
        Assert.AreEqual(sizeof(ulong), CScript.EvaluateInt32("sizeof(rlim_t)", "sys/resource.h"));
        Assert.AreEqual(CScript.EvaluateInt32("sizeof(struct rlimit)", "sys/resource.h"), Unsafe.SizeOf<Resource.rlimit>());
    }

    [TestMethod]
    public void Arm32_Time64_Layout_Matches_Current_Platform_Headers()
    {
        if (!NativeAbi.IsArm32)
            return;

        Assert.AreEqual(CScript.EvaluateInt32("sizeof(struct timespec)", "stddef.h", "time.h"), Unsafe.SizeOf<Time.arm_timespec64>());
        Assert.AreEqual(CScript.EvaluateInt32("offsetof(struct timespec, tv_sec)", "stddef.h", "time.h"), Marshal.OffsetOf<Time.arm_timespec64>(nameof(Time.arm_timespec64.tv_sec)).ToInt32());
        Assert.AreEqual(CScript.EvaluateInt32("offsetof(struct timespec, tv_nsec)", "stddef.h", "time.h"), Marshal.OffsetOf<Time.arm_timespec64>(nameof(Time.arm_timespec64.tv_nsec)).ToInt32());
        Assert.AreEqual(sizeof(int), CScript.EvaluateInt32("sizeof(((struct timespec*)0)->tv_nsec)", "time.h"));
    }
}
