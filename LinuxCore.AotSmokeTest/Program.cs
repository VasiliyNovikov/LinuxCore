using System;
using System.Threading;

using LinuxCore;

var payload = "aot-smoke"u8;

using var file = new LinuxMemoryFile("linuxcore-aot-smoke");
if (file.Write(payload) != payload.Length)
    throw new InvalidOperationException("Failed to write the expected payload.");

file.Position = 0;
Span<byte> buffer = stackalloc byte[payload.Length];
if (file.Read(buffer) != payload.Length || !buffer.SequenceEqual(payload))
    throw new InvalidOperationException("Failed to read the expected payload.");

using var @event = new LinuxEvent();
if (LinuxPoll.Wait(@event.Descriptor, LinuxPoll.Event.Readable, 0) is not null)
    throw new InvalidOperationException("New event should start unset.");

@event.Set();
@event.Wait();

using var semaphore = new LinuxSemaphoreSlim();
var incrementThread = new Thread(semaphore.Increment);
incrementThread.Start();
semaphore.Decrement();
incrementThread.Join();

var pageSize = SystemConfiguration.Get(SystemConfigurationName.PageSize);
if (pageSize <= 0)
    throw new InvalidOperationException("Expected a positive page size.");

using var map = new LinuxMemoryMap(16);
"mmap"u8.CopyTo(map.Span);
if (!map.Span[..4].SequenceEqual("mmap"u8))
    throw new InvalidOperationException("Failed to round-trip an anonymous memory map.");

var currentUser = LinuxUser.Current ?? throw new InvalidOperationException("Expected the current user to be resolvable.");
Console.WriteLine($"LinuxCore AOT smoke passed for {currentUser.Name} with page size {pageSize}.");
