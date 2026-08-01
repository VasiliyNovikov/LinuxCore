using System;
using System.Threading;

using LinuxCore;

var payload = "aot-smoke"u8;

var filePath = System.IO.Path.GetTempFileName();
try
{
    using var pathFile = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);
    _ = pathFile.Flags;
    _ = pathFile.Size;
}
finally
{
    System.IO.File.Delete(filePath);
}

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

if (LinuxClock.MonotonicNanoseconds <= 0)
    throw new InvalidOperationException("Expected a positive monotonic clock value.");

var (softLimit, hardLimit) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.NumOpenFiles);
if (softLimit == 0 || hardLimit < softLimit)
    throw new InvalidOperationException("Expected valid open-file limits.");

const long largeOffset = 1L << 32;
file.Position = largeOffset + pageSize - 1;
if (file.Write("\0"u8) != 1)
    throw new InvalidOperationException("Failed to extend the memory file for a large-offset map.");

using var largeMap = new LinuxMemoryMap(file.Descriptor, checked((int)pageSize), offset: largeOffset);
"large-map"u8.CopyTo(largeMap.Span);
file.Position = largeOffset;
if (file.Read(buffer) != buffer.Length || !buffer.SequenceEqual("large-map"u8))
    throw new InvalidOperationException("Failed to round-trip a large-offset memory map.");

using var map = new LinuxMemoryMap(16);
"mmap"u8.CopyTo(map.Span);
if (!map.Span[..4].SequenceEqual("mmap"u8))
    throw new InvalidOperationException("Failed to round-trip an anonymous memory map.");

var socketAddress = UnixSocketAddress.FromAbstractName($"linuxcore-aot-{Guid.NewGuid():N}");
using var descriptorSender = new UnixSocket(LinuxSocketType.Datagram);
using var descriptorReceiver = new UnixSocket(LinuxSocketType.Datagram);
descriptorReceiver.Bind(socketAddress);
descriptorSender.Connect(socketAddress);
if (descriptorSender.SendFileDescriptors([1], [file.Descriptor]) != 1)
    throw new InvalidOperationException("Failed to send a file descriptor.");
Span<byte> descriptorPayload = stackalloc byte[1];
Span<FileDescriptor> receivedDescriptors = stackalloc FileDescriptor[1];
try
{
    if (descriptorReceiver.ReceiveFileDescriptors(descriptorPayload, receivedDescriptors, out var receivedDescriptorCount, out _) != 1 || receivedDescriptorCount != 1)
        throw new InvalidOperationException("Failed to receive a file descriptor.");
}
finally
{
    foreach (var descriptor in receivedDescriptors)
        descriptor.Close();
}

var currentUser = LinuxUser.Current ?? throw new InvalidOperationException("Expected the current user to be resolvable.");
Console.WriteLine($"LinuxCore AOT smoke passed for {currentUser.Name} with page size {pageSize}.");