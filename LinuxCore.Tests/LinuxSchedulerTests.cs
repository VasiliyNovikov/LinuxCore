using System;
using System.Globalization;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxSchedulerTests
{
    private const string ProcSelfSchedPath = "/proc/self/sched";

    private static (LinuxScheduler.Policy Policy, int Priority) ReadProcSched()
    {
        var lines = File.ReadAllLines(ProcSelfSchedPath);
        var policy = (LinuxScheduler.Policy)(-1);
        var prio = -1;
        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length != 2)
                continue;
            var key = parts[0].Trim();
            switch (key)
            {
                case "policy":
                    policy = (LinuxScheduler.Policy)int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                    break;
                case "prio":
                    prio = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                    break;
            }
        }
        // For RT policies (FIFO=1, RR=2), sched_priority = 100 - prio
        // For non-RT policies (OTHER=0, BATCH=3, IDLE=5), sched_priority = 0
        var schedPriority = policy is LinuxScheduler.Policy.FIFO or LinuxScheduler.Policy.RoundRobin ? 100 - prio : 0;
        return (policy, schedPriority);
    }

    [TestMethod]
    public void LinuxScheduler_Get_Returns_Other_For_Normal_Process()
    {
        Assert.AreEqual((LinuxScheduler.Policy.Other, 0), LinuxScheduler.Get());
        if (File.Exists(ProcSelfSchedPath))
            Assert.AreEqual((LinuxScheduler.Policy.Other, 0), ReadProcSched());
    }

    [TestMethod]
    public void LinuxScheduler_Get_By_Pid_Returns_Same_As_Current_Process() => Assert.AreEqual(LinuxScheduler.Get(), LinuxScheduler.Get(Environment.ProcessId));

    [TestMethod]
    public void LinuxScheduler_Get_Invalid_Pid_Throws_LinuxException()
    {
        var error = Assert.ThrowsExactly<LinuxException>(() => LinuxScheduler.Get(-1));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, error.ErrorNumber);
    }

    [TestMethod]
    public void LinuxScheduler_Set_Invalid_Policy_Throws_LinuxException()
    {
        var error = Assert.ThrowsExactly<LinuxException>(() => LinuxScheduler.Set((LinuxScheduler.Policy)10, 0));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, error.ErrorNumber);
    }

    [TestMethod]
    public void LinuxScheduler_Set_Invalid_Priority_Throws_LinuxException()
    {
        var error = Assert.ThrowsExactly<LinuxException>(() => LinuxScheduler.Set(LinuxScheduler.Policy.Other, 1));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, error.ErrorNumber);
    }

    [TestMethod]
    public void LinuxScheduler_Get_Set_RoundTrip()
    {
        Assert.AreEqual((LinuxScheduler.Policy.Other, 0), LinuxScheduler.Get());
        try
        {
            LinuxScheduler.Set(LinuxScheduler.Policy.Batch, 0);
            Assert.AreEqual((LinuxScheduler.Policy.Batch, 0), LinuxScheduler.Get());
            if (File.Exists(ProcSelfSchedPath))
                Assert.AreEqual((LinuxScheduler.Policy.Batch, 0), ReadProcSched());
        }
        finally
        {
            LinuxScheduler.Set(LinuxScheduler.Policy.Other, 0);
        }
    }
}