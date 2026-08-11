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
    }

    private static void AssertSysCall(string name, SystemCallNumber number) => Assert.AreEqual(CScript.EvaluateNInt(name, "sys/syscall.h"), number.Value);
}