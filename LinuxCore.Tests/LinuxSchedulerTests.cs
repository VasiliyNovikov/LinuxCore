using System;
using System.Globalization;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sched = LinuxCore.Interop.Sched;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxSchedulerTests
{
    [TestMethod]
    public void LinuxScheduler_Constants_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.EnumValuesMatch<LinuxScheduler.Policy>(
        [
            (nameof(LinuxScheduler.Policy.Other), "SCHED_NORMAL"),
            (nameof(LinuxScheduler.Policy.FIFO), "SCHED_FIFO"),
            (nameof(LinuxScheduler.Policy.RoundRobin), "SCHED_RR"),
            (nameof(LinuxScheduler.Policy.Batch), "SCHED_BATCH"),
            (nameof(LinuxScheduler.Policy.Idle), "SCHED_IDLE"),
            (nameof(LinuxScheduler.Policy.Deadline), "SCHED_DEADLINE")
        ],
        [
            (nameof(LinuxScheduler.Policy.ISO), "SCHED_ISO"),
            (nameof(LinuxScheduler.Policy.Extensible), "SCHED_EXT")
        ], "linux/sched.h");
        Assert.AreEqual(Sched.SCHED_RESET_ON_FORK, CScript.EvaluateInt32("SCHED_RESET_ON_FORK", "linux/sched.h"));
    }

    private static (LinuxScheduler.Policy Policy, int Priority) ReadProcSched()
    {
        var lines = File.ReadAllLines("/proc/self/sched");
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
            Assert.AreEqual((LinuxScheduler.Policy.Batch, 0), ReadProcSched());
        }
        finally
        {
            LinuxScheduler.Set(LinuxScheduler.Policy.Other, 0);
        }
    }
}