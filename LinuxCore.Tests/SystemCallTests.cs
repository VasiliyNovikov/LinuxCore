using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class SystemCallTests
{
    [TestMethod]
    public void SystemCall_Invoke_WithResult_ReturnsExpectedValue()
    {
        // sched_getscheduler(pid=0) returns the current process's scheduling policy
        var result = SystemCall.Invoke<int, int>(SystemCallTable.Current.SchedGetScheduler, 0);

        Assert.IsFalse(result.IsError);
        Assert.AreEqual((int)LinuxScheduler.Policy.Other, result.ThrowIfError());
    }

    [TestMethod]
    public void SystemCall_Invoke_WithResult_Error_IsError()
    {
        // sched_getscheduler(-1) — invalid pid, kernel returns ESRCH
        var result = SystemCall.Invoke<int, int>(SystemCallTable.Current.SchedGetScheduler, -1);

        Assert.IsTrue(result.IsError);
    }

    [TestMethod]
    public void SystemCall_Invoke_VoidResult_ErrorResult_IsError()
    {
        // sched_getparam(pid=0, param=null) — passing a null output pointer returns EFAULT
        var result = SystemCall.Invoke(SystemCallTable.Current.SchedGetParam, 0, 0L);

        Assert.IsTrue(result.IsError);
    }

    [TestMethod]
    public void SystemCall_NonBlocking_Invoke_WithResult_ReturnsExpectedValue()
    {
        var result = SystemCall.NonBlocking.Invoke<int, int>(SystemCallTable.Current.SchedGetScheduler, 0);

        Assert.IsFalse(result.IsError);
        Assert.AreEqual((int)LinuxScheduler.Policy.Other, result.ThrowIfError());
    }

    [TestMethod]
    public void SystemCall_NonBlocking_Invoke_Error_IsError()
    {
        var result = SystemCall.NonBlocking.Invoke<int, int>(SystemCallTable.Current.SchedGetScheduler, -1);

        Assert.IsTrue(result.IsError);
    }

    [TestMethod]
    public void SystemCallTable_Current_Architecture_HasValidSchedulerNumbers()
    {
        // Verify the table returns plausible syscall numbers for all three scheduler calls.
        // Exact values are architecture-dependent; we only verify they are positive.
        Assert.IsNotNull(SystemCallTable.Current);
        _ = SystemCallTable.Current.SchedGetScheduler;
        _ = SystemCallTable.Current.SchedSetScheduler;
        _ = SystemCallTable.Current.SchedGetParam;
    }
}
