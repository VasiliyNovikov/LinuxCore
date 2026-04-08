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
    public void LinuxClock_BootTime_Is_Positive()
    {
        var bootTime = LinuxClock.BootTime;
        Assert.IsGreaterThan(TimeSpan.Zero, bootTime, $"BootTime {bootTime} should be positive");
    }

    [TestMethod]
    public void LinuxClock_BootTimeNanoseconds_Is_Positive()
    {
        var ns = LinuxClock.BootTimeNanoseconds;
        Assert.IsGreaterThan(0L, ns, $"BootTimeNanoseconds {ns} should be positive");
    }

    [TestMethod]
    public void LinuxClock_BootTime_And_BootTimeNanoseconds_Are_Consistent()
    {
        var ns = LinuxClock.BootTimeNanoseconds;
        var ts = LinuxClock.BootTime;

        var tsNs = (long)(ts.TotalNanoseconds);
        Assert.AreEqual(ns, tsNs, 10_000_000.0); // 10ms tolerance
    }

    [TestMethod]
    public void LinuxClock_ProcessCpuTime_Is_Non_Negative()
    {
        var cpuTime = LinuxClock.ProcessCpuTime;
        Assert.IsGreaterThanOrEqualTo(TimeSpan.Zero, cpuTime, $"ProcessCpuTime {cpuTime} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ProcessCpuTimeNanoseconds_Is_Non_Negative()
    {
        var ns = LinuxClock.ProcessCpuTimeNanoseconds;
        Assert.IsGreaterThanOrEqualTo(0L, ns, $"ProcessCpuTimeNanoseconds {ns} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ThreadCpuTime_Is_Non_Negative()
    {
        var cpuTime = LinuxClock.ThreadCpuTime;
        Assert.IsGreaterThanOrEqualTo(TimeSpan.Zero, cpuTime, $"ThreadCpuTime {cpuTime} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_ThreadCpuTimeNanoseconds_Is_Non_Negative()
    {
        var ns = LinuxClock.ThreadCpuTimeNanoseconds;
        Assert.IsGreaterThanOrEqualTo(0L, ns, $"ThreadCpuTimeNanoseconds {ns} should be non-negative");
    }

    [TestMethod]
    public void LinuxClock_Sleep_Sleeps_For_Approximately_Expected_Duration()
    {
        var expected = TimeSpan.FromMilliseconds(50);
        var sw = Stopwatch.StartNew();

        LinuxClock.Sleep(expected);

        sw.Stop();
        // Allow generous tolerance: at least 45ms, at most 250ms
        Assert.IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(45), sw.Elapsed, $"Sleep was too short: {sw.Elapsed}");
        Assert.IsLessThan(TimeSpan.FromMilliseconds(250), sw.Elapsed, $"Sleep was too long: {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxClock_Sleep_Returns_Immediately_For_Zero_Duration()
    {
        var sw = Stopwatch.StartNew();
        LinuxClock.Sleep(TimeSpan.Zero);
        sw.Stop();

        Assert.IsLessThan(TimeSpan.FromMilliseconds(10), sw.Elapsed, $"Sleep(Zero) took too long: {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxClock_Sleep_Returns_Immediately_For_Negative_Duration()
    {
        var sw = Stopwatch.StartNew();
        LinuxClock.Sleep(TimeSpan.FromMilliseconds(-10));
        sw.Stop();

        Assert.IsLessThan(TimeSpan.FromMilliseconds(10), sw.Elapsed, $"Sleep(negative) took too long: {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxClock_SleepUntil_Sleeps_Until_Future_Timestamp()
    {
        var target = LinuxClock.Realtime.AddMilliseconds(50);
        var sw = Stopwatch.StartNew();

        LinuxClock.SleepUntil(target);

        sw.Stop();
        Assert.IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(40), sw.Elapsed, $"SleepUntil returned too early: {sw.Elapsed}");
        Assert.IsLessThan(TimeSpan.FromMilliseconds(250), sw.Elapsed, $"SleepUntil took too long: {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxClock_SleepUntil_Returns_Immediately_For_Past_Timestamp()
    {
        var past = LinuxClock.Realtime.AddSeconds(-1);
        var sw = Stopwatch.StartNew();

        LinuxClock.SleepUntil(past);

        sw.Stop();
        Assert.IsLessThan(TimeSpan.FromMilliseconds(50), sw.Elapsed, $"SleepUntil(past) took too long: {sw.Elapsed}");
    }
}