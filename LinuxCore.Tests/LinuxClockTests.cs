using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxClockTests
{
    [TestMethod]
    public void LinuxClock_Interval_Is_Valid()
    {
        var sw = Stopwatch.StartNew();
        var start = LinuxClock.Monotonic;

        Thread.Sleep(100);

        sw.Stop();
        var end = LinuxClock.Monotonic;

        var linuxElapsed = end - start;
        var stopwatchElapsed = sw.Elapsed;

        Assert.AreEqual(stopwatchElapsed.TotalMilliseconds, linuxElapsed.TotalMilliseconds, 1);
    }

    [TestMethod]
    public void LinuxClock_Realtime_Is_Close_To_UtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var realtime = LinuxClock.Realtime;
        var after = DateTimeOffset.UtcNow;

        Assert.IsGreaterThanOrEqualTo(before, realtime, $"Realtime {realtime} is before UtcNow snapshot {before}");
        Assert.IsLessThanOrEqualTo(after, realtime, $"Realtime {realtime} is after UtcNow snapshot {after}");
    }

    [TestMethod]
    public void LinuxClock_RealtimeNanoseconds_Is_Positive_And_Recent()
    {
        var ns = LinuxClock.RealtimeNanoseconds;

        // Must be after 2020-01-01T00:00:00Z: 1577836800 seconds * 1e9
        const long minExpected = 1_577_836_800L * 1_000_000_000L;
        Assert.IsGreaterThan(minExpected, ns, $"RealtimeNanoseconds {ns} looks too small");
    }

    [TestMethod]
    public void LinuxClock_Realtime_And_RealtimeNanoseconds_Are_Consistent()
    {
        var ns = LinuxClock.RealtimeNanoseconds;
        var dt = LinuxClock.Realtime;

        // Both readings are close in time; allow 10ms tolerance
        var dtNs = (dt - DateTimeOffset.UnixEpoch).Ticks * TimeSpan.NanosecondsPerTick;
        Assert.AreEqual(ns, dtNs, 10_000_000.0); // 10ms in nanoseconds
    }

    [TestMethod]
    public void LinuxClock_BootTimeNanoseconds_Is_Positive()
    {
        var ns = LinuxClock.BootTimeNanoseconds;

        Assert.IsTrue(ns > 0L, $"BootTimeNanoseconds {ns} should be positive");
    }

    [TestMethod]
    public void LinuxClock_BootTime_Is_Positive()
    {
        var time = LinuxClock.BootTime;

        Assert.IsTrue(time > TimeSpan.Zero, $"BootTime {time} should be positive");
    }

    [TestMethod]
    public void LinuxClock_BootTime_And_BootTimeNanoseconds_Are_Consistent()
    {
        var ns = LinuxClock.BootTimeNanoseconds;
        var time = LinuxClock.BootTime;

        var timeNs = time.Ticks * TimeSpan.NanosecondsPerTick;
        Assert.AreEqual(ns, timeNs, 10_000_000.0); // 10ms in nanoseconds
    }

    [TestMethod]
    public void LinuxClock_ProcessCpuTimeNanoseconds_Is_NonNegative()
    {
        var ns = LinuxClock.ProcessCpuTimeNanoseconds;

        Assert.IsTrue(ns >= 0L, $"ProcessCpuTimeNanoseconds {ns} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ProcessCpuTime_Is_NonNegative()
    {
        var time = LinuxClock.ProcessCpuTime;

        Assert.IsTrue(time >= TimeSpan.Zero, $"ProcessCpuTime {time} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ThreadCpuTimeNanoseconds_Is_NonNegative()
    {
        var ns = LinuxClock.ThreadCpuTimeNanoseconds;

        Assert.IsTrue(ns >= 0L, $"ThreadCpuTimeNanoseconds {ns} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ThreadCpuTime_Is_NonNegative()
    {
        var time = LinuxClock.ThreadCpuTime;

        Assert.IsTrue(time >= TimeSpan.Zero, $"ThreadCpuTime {time} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ProcessCpuTime_Advances_During_Cpu_Work()
    {
        var start = LinuxClock.ProcessCpuTimeNanoseconds;

        BurnCpu();

        var end = LinuxClock.ProcessCpuTimeNanoseconds;
        Assert.IsTrue(end > start, $"Process CPU time did not advance from {start} to {end}");
    }

    [TestMethod]
    public void LinuxClock_ThreadCpuTime_Advances_During_Cpu_Work()
    {
        var start = LinuxClock.ThreadCpuTimeNanoseconds;

        BurnCpu();

        var end = LinuxClock.ThreadCpuTimeNanoseconds;
        Assert.IsTrue(end > start, $"Thread CPU time did not advance from {start} to {end}");
    }

    [TestMethod]
    public void LinuxClock_Sleep_Zero_Returns_Quickly()
    {
        var sw = Stopwatch.StartNew();

        LinuxClock.Sleep(TimeSpan.Zero);

        sw.Stop();
        Assert.IsTrue(sw.Elapsed < TimeSpan.FromMilliseconds(100), $"Zero sleep took {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxClock_Sleep_Negative_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => LinuxClock.Sleep(TimeSpan.FromTicks(-1)));
    }

    [TestMethod]
    public void LinuxClock_Sleep_Waits_For_Duration()
    {
        var duration = TimeSpan.FromMilliseconds(100);
        var precision = duration / 5;
        var sw = Stopwatch.StartNew();

        LinuxClock.Sleep(duration);

        sw.Stop();
        Assert.IsTrue(sw.Elapsed >= duration, $"Sleep elapsed {sw.Elapsed} was shorter than {duration}");
        Assert.AreEqual(duration.TotalMilliseconds, sw.Elapsed.TotalMilliseconds, precision.TotalMilliseconds);
    }

    private static void BurnCpu()
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < TimeSpan.FromMilliseconds(20))
            Thread.SpinWait(1000);
    }

    [TestMethod]
    public void LinuxClock_MonotonicRawNanoseconds_Is_Positive()
    {
        var ns = LinuxClock.MonotonicRawNanoseconds;

        Assert.IsTrue(ns > 0L, $"MonotonicRawNanoseconds {ns} should be positive");
    }

    [TestMethod]
    public void LinuxClock_MonotonicRaw_Is_Positive()
    {
        var time = LinuxClock.MonotonicRaw;

        Assert.IsTrue(time > TimeSpan.Zero, $"MonotonicRaw {time} should be positive");
    }

    [TestMethod]
    public void LinuxClock_MonotonicRaw_And_MonotonicRawNanoseconds_Are_Consistent()
    {
        var ns = LinuxClock.MonotonicRawNanoseconds;
        var time = LinuxClock.MonotonicRaw;

        var timeNs = time.Ticks * TimeSpan.NanosecondsPerTick;
        Assert.AreEqual(ns, timeNs, 10_000_000.0); // 10ms in nanoseconds
    }

    [TestMethod]
    public void LinuxClock_MonotonicRaw_Advances_Monotonically()
    {
        var ns1 = LinuxClock.MonotonicRawNanoseconds;
        var ns2 = LinuxClock.MonotonicRawNanoseconds;

        Assert.IsTrue(ns2 >= ns1, $"MonotonicRaw went backwards: {ns1} -> {ns2}");
    }
}