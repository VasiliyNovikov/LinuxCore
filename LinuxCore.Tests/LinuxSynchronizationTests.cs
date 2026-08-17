using System;
using System.Threading;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxSynchronizationTests
{
    [TestMethod]
    public void LinuxPoll_And_EventFd_Constants_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.EnumValuesMatch<LinuxPoll.Event>(
        [
            (nameof(LinuxPoll.Event.None), "0"),
            (nameof(LinuxPoll.Event.Readable), "POLLIN"),
            (nameof(LinuxPoll.Event.Urgent), "POLLPRI"),
            (nameof(LinuxPoll.Event.Writable), "POLLOUT"),
            (nameof(LinuxPoll.Event.Error), "POLLERR"),
            (nameof(LinuxPoll.Event.HangUp), "POLLHUP"),
            (nameof(LinuxPoll.Event.Invalid), "POLLNVAL")
        ], "poll.h");

        NativeConstantAssert.ValuesMatch(
        [
            (nameof(EventFd.EFD_SEMAPHORE), EventFd.EFD_SEMAPHORE, "EFD_SEMAPHORE"),
            (nameof(EventFd.EFD_NONBLOCK), EventFd.EFD_NONBLOCK, "EFD_NONBLOCK"),
            (nameof(EventFd.EFD_CLOEXEC), EventFd.EFD_CLOEXEC, "EFD_CLOEXEC")
        ], "sys/eventfd.h");
    }

    [TestMethod]
    public void LinuxEvent_Set_Makes_Descriptor_Readable_Until_Wait()
    {
        using var @event = new LinuxEvent();

        Assert.IsNull(LinuxPoll.Wait(@event.Descriptor, LinuxPoll.Event.Readable, 0));

        @event.Set();
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(@event.Descriptor, LinuxPoll.Event.Readable, 0));

        @event.Wait();
        Assert.IsNull(LinuxPoll.Wait(@event.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxSemaphore_Increment_Allows_Decrement_And_Clears_Readiness()
    {
        using var semaphore = new LinuxSemaphore();

        semaphore.Increment();
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Decrement();
        Assert.IsNull(LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxSemaphore_InitialValue_And_CloseOnExec_Are_Applied()
    {
        using var semaphore = new LinuxSemaphore(2);

        Assert.IsTrue(semaphore.CloseOnExec);
        semaphore.Decrement();
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));
        semaphore.Decrement();
        Assert.IsNull(LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxSemaphoreSlim_Add_Remove_Tracks_Count_Transitions()
    {
        using var semaphore = new LinuxSemaphoreSlim();

        Assert.IsNull(LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Add(3);
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Remove(2);
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Decrement();
        Assert.IsNull(LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxSemaphoreSlim_Decrement_Blocks_Until_Increment()
    {
        using var semaphore = new LinuxSemaphoreSlim();
        using var started = new ManualResetEventSlim();

        var thread = new Thread(() =>
        {
            started.Set();
            Thread.Sleep(50);
            semaphore.Increment();
        });

        thread.Start();
        Assert.IsTrue(started.Wait(TimeSpan.FromSeconds(1)));

        semaphore.Decrement();
        thread.Join();

        Assert.IsNull(LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxPoll_Wait_With_Unset_Event_Times_Out()
    {
        using var @event = new LinuxEvent();
        Span<LinuxPoll.Query> queries = [new(@event.Descriptor, LinuxPoll.Event.Readable)];

        Assert.IsFalse(LinuxPoll.Wait(queries, 0));
        Assert.AreEqual(LinuxPoll.Event.None, queries[0].ReturnedEvents);
    }

    [TestMethod]
    public void LinuxCancellationToken_Wait_Returns_When_Object_Is_Ready()
    {
        using var cts = new CancellationTokenSource();
        using var token = new LinuxCancellationToken(cts.Token);
        using var @event = new LinuxEvent();

        @event.Set();

        Assert.IsTrue(token.Wait(@event, LinuxPoll.Event.Readable));
        @event.Wait();
    }

    [TestMethod]
    public void LinuxCancellationToken_Wait_Throws_When_Already_Cancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using var token = new LinuxCancellationToken(cts.Token);
        using var @event = new LinuxEvent();

        Assert.ThrowsExactly<OperationCanceledException>(() => token.Wait(@event, LinuxPoll.Event.Readable));
    }

    [TestMethod]
    public void LinuxCancellationToken_Wait_Throws_When_Cancelled_During_Wait()
    {
        using var cts = new CancellationTokenSource();
        using var token = new LinuxCancellationToken(cts.Token);
        using var @event = new LinuxEvent();
        using var started = new ManualResetEventSlim();

        var thread = new Thread(() =>
        {
            started.Set();
            Thread.Sleep(50);
            cts.Cancel();
        });

        thread.Start();
        Assert.IsTrue(started.Wait(TimeSpan.FromSeconds(1)));

        Assert.ThrowsExactly<OperationCanceledException>(() => token.Wait(@event, LinuxPoll.Event.Readable));
        thread.Join();
    }

    [TestMethod]
    public void LinuxCancellationToken_None_Wait_Returns_When_Object_Is_Ready()
    {
        using var @event = new LinuxEvent();
        @event.Set();

        Assert.IsTrue(LinuxCancellationToken.None.Wait(@event, LinuxPoll.Event.Readable));
        @event.Wait();
    }
}