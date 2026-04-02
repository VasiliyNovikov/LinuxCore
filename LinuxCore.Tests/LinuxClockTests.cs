using System;
using System.Diagnostics;

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

        System.Threading.Thread.Sleep(100);

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
}