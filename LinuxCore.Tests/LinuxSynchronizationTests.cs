using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxSynchronizationTests
{
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
    public void LinuxPoll_Wait_TimeSpan_Returns_True_On_Ready_Descriptor()
    {
        using var @event = new LinuxEvent();
        @event.Set();

        var result = LinuxPoll.Wait(@event.Descriptor, LinuxPoll.Event.Readable, TimeSpan.Zero);
        Assert.AreEqual(LinuxPoll.Event.Readable, result);
    }

    [TestMethod]
    public void LinuxPoll_Wait_TimeSpan_Span_Returns_True_On_Ready_Descriptor()
    {
        using var @event = new LinuxEvent();
        @event.Set();

        Span<LinuxPoll.Query> queries = [new(@event.Descriptor, LinuxPoll.Event.Readable)];
        Assert.IsTrue(LinuxPoll.Wait(queries, TimeSpan.Zero));
        Assert.AreEqual(LinuxPoll.Event.Readable, queries[0].ReturnedEvents);
    }

    [TestMethod]
    public void LinuxEvent_Created_With_IsSet_True_Is_Immediately_Readable()
    {
        using var @event = new LinuxEvent(isSet: true);
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(@event.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxSemaphore_WithInitialValue_Is_Immediately_Available()
    {
        using var semaphore = new LinuxSemaphore(initialValue: 3);

        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Decrement();
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Decrement();
        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));

        semaphore.Decrement();
        Assert.IsNull(LinuxPoll.Wait(semaphore.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxCancellationToken_None_Wait_Returns_True_When_Object_Ready()
    {
        using var @event = new LinuxEvent();
        @event.Set();

        Assert.IsTrue(LinuxCancellationToken.None.Wait(@event, LinuxPoll.Event.Readable));
        @event.Wait();
    }

    [TestMethod]
    public void LinuxCancellationToken_Wait_Multiple_Objects_Returns_When_First_Ready()
    {
        using var cts = new CancellationTokenSource();
        using var token = new LinuxCancellationToken(cts.Token);
        using var event1 = new LinuxEvent();
        using var event2 = new LinuxEvent();

        event1.Set();

        IFileObject[] objects = [event1, event2];
        LinuxPoll.Event[] events = [LinuxPoll.Event.Readable, LinuxPoll.Event.Readable];
        Assert.IsTrue(token.Wait(objects, events));

        event1.Wait();
    }
}