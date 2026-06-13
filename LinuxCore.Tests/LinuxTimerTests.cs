using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxTimerTests
{
    [TestMethod]
    public void LinuxTimer_Monotonic_Creates_Timer()
    {
        using var timer = LinuxTimer.Monotonic();
        Assert.IsNotNull(timer);
    }

    [TestMethod]
    public void LinuxTimer_Realtime_Creates_Timer()
    {
        using var timer = LinuxTimer.Realtime();
        Assert.IsNotNull(timer);
    }

    [TestMethod]
    public void LinuxTimer_BootTime_Creates_Timer()
    {
        using var timer = LinuxTimer.BootTime();
        Assert.IsNotNull(timer);
    }

    [TestMethod]
    public void LinuxTimer_SetOneShot_ZeroDelay_Throws()
    {
        using var timer = LinuxTimer.Monotonic();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timer.SetOneShot(TimeSpan.Zero));
    }

    [TestMethod]
    public void LinuxTimer_SetOneShot_NegativeDelay_Throws()
    {
        using var timer = LinuxTimer.Monotonic();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timer.SetOneShot(TimeSpan.FromMilliseconds(-1)));
    }

    [TestMethod]
    public void LinuxTimer_SetPeriodic_ZeroPeriod_Throws()
    {
        using var timer = LinuxTimer.Monotonic();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timer.SetPeriodic(TimeSpan.Zero));
    }

    [TestMethod]
    public void LinuxTimer_TryWait_Returns_False_Before_Expiry()
    {
        using var timer = LinuxTimer.Monotonic();
        timer.SetOneShot(TimeSpan.FromSeconds(60));
        Assert.IsFalse(timer.TryWait(out _));
    }

    [TestMethod]
    public void LinuxTimer_Wait_Fires_After_Delay()
    {
        using var timer = LinuxTimer.Monotonic();
        var delay = TimeSpan.FromMilliseconds(100);
        var sw = Stopwatch.StartNew();
        timer.SetOneShot(delay);
        var expirations = timer.Wait();
        sw.Stop();

        Assert.AreEqual(1UL, expirations);
        Assert.IsTrue(sw.Elapsed >= delay, $"Timer fired too early: {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxTimer_TryWait_Returns_True_After_Expiry()
    {
        using var timer = LinuxTimer.Monotonic();
        timer.SetOneShot(TimeSpan.FromMilliseconds(50));

        // Wait until it fires
        timer.Wait();

        // Now arm again and let it expire, then poll
        timer.SetOneShot(TimeSpan.FromMilliseconds(50));
        timer.Wait();

        // After Wait consumed the expiration, TryWait should be false again
        Assert.IsFalse(timer.TryWait(out _));
    }

    [TestMethod]
    public void LinuxTimer_Disarm_Prevents_Firing()
    {
        using var timer = LinuxTimer.Monotonic();
        timer.SetOneShot(TimeSpan.FromMilliseconds(50));
        timer.Disarm();

        // Give the timer some extra time; it should not fire
        System.Threading.Thread.Sleep(100);
        Assert.IsFalse(timer.TryWait(out _));
    }

    [TestMethod]
    public void LinuxTimer_Periodic_Fires_Multiple_Times()
    {
        using var timer = LinuxTimer.Monotonic();
        var period = TimeSpan.FromMilliseconds(50);
        timer.SetPeriodic(period);

        // Collect 3 expirations
        for (var i = 0; i < 3; i++)
        {
            var exp = timer.Wait();
            Assert.IsTrue(exp >= 1UL, $"Expected at least 1 expiration on iteration {i}, got {exp}");
        }
    }

    [TestMethod]
    public void LinuxTimer_SetPeriodic_With_InitialDelay_Fires_After_Delay()
    {
        using var timer = LinuxTimer.Monotonic();
        var delay = TimeSpan.FromMilliseconds(100);
        var period = TimeSpan.FromMilliseconds(200);
        var sw = Stopwatch.StartNew();
        timer.SetPeriodic(delay, period);
        var expirations = timer.Wait();
        sw.Stop();

        Assert.AreEqual(1UL, expirations);
        Assert.IsTrue(sw.Elapsed >= delay, $"First expiration occurred too early: {sw.Elapsed}");
    }

    [TestMethod]
    public void LinuxTimer_Descriptor_Has_CloseOnExec()
    {
        using var timer = LinuxTimer.Monotonic();
        Assert.IsTrue(timer.CloseOnExec);
    }
}
