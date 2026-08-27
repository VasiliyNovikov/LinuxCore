using System;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class SystemCallTableTests
{
    [TestMethod]
    public void Current_Values_Match_Current_Platform_Headers()
    {
        AssertSysCall("__NR_sched_getscheduler", SystemCallTable.Current.SchedGetScheduler);
        AssertSysCall("__NR_sched_setscheduler", SystemCallTable.Current.SchedSetScheduler);
        AssertSysCall("__NR_sched_getparam", SystemCallTable.Current.SchedGetParam);

        AssertSysCall("__NR_pidfd_open", SystemCallTable.PidFdOpen);

        AssertSysCall("__NR_io_uring_setup", SystemCallTable.IOUringSetup);
        AssertSysCall("__NR_io_uring_enter", SystemCallTable.IOUringEnter);
        AssertSysCall("__NR_io_uring_register", SystemCallTable.IOUringRegister);

        AssertSysCall("__NR_openat", SystemCallTable.Current.OpenAt);
        AssertSysCall("__NR_close", SystemCallTable.Current.Close);
        AssertSysCall("__NR_dup", SystemCallTable.Current.Dup);
        AssertSysCall("__NR_read", SystemCallTable.Current.Read);
        AssertSysCall("__NR_write", SystemCallTable.Current.Write);
        AssertSysCall("__NR_ioctl", SystemCallTable.Current.Ioctl);
        AssertSysCall(NativeAbi.Is64Bit ? "__NR_fcntl" : "__NR_fcntl64", SystemCallTable.Current.Fcntl);
        AssertSysCall("__NR_statx", SystemCallTable.Current.Statx);
        AssertSysCall("__NR_lseek", SystemCallTable.Current.Lseek);
        if (!NativeAbi.Is64Bit)
            AssertSysCall("__NR__llseek", SystemCallTable.Current.Llseek);

        if (NativeAbi.Is64Bit)
        {
            AssertSysCall("__NR_mmap", SystemCallTable.Current.Mmap);
            Assert.ThrowsExactly<NotImplementedException>(() => SystemCallTable.Current.Mmap2);
        }
        else
        {
            Assert.ThrowsExactly<NotImplementedException>(() => SystemCallTable.Current.Mmap);
            AssertSysCall("__NR_mmap2", SystemCallTable.Current.Mmap2);
        }
        AssertSysCall("__NR_munmap", SystemCallTable.Current.Munmap);
        AssertSysCall("__NR_msync", SystemCallTable.Current.Msync);

        AssertSysCall("__NR_memfd_create", SystemCallTable.Current.MemFdCreate);

        AssertSysCall("__NR_eventfd2", SystemCallTable.Current.EventFd2);
    }

    private static void AssertSysCall(string name, SystemCallNumber number) => Assert.AreEqual(CScript.EvaluateNInt(name, "sys/syscall.h"), number.Value);
}