using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxMemoryMapTests
{
    [TestMethod]
    public void LinuxMemoryMap_Anonymous_RoundTrips_Through_Memory()
    {
        using var map = new LinuxMemoryMap(16);

        var span = map.Span;
        "mapped"u8.CopyTo(span);

        Assert.IsTrue(span[..6].SequenceEqual("mapped"u8));
    }

    [TestMethod]
    public void LinuxMemoryMap_FileBacked_Shared_Writes_To_File()
    {
        using var file = new LinuxMemoryFile("mapped-file");

        const int length = 16;
        Span<byte> zeroes = stackalloc byte[length];
        Assert.AreEqual(length, file.Write(zeroes));

        using var map = new LinuxMemoryMap(file.Descriptor, length);
        var expected = "shared-map"u8;
        expected.CopyTo(map.Span);

        file.Position = 0;
        Span<byte> actual = stackalloc byte[expected.Length];
        Assert.AreEqual(expected.Length, file.Read(actual));
        Assert.IsTrue(actual.SequenceEqual(expected));
    }

    [TestMethod]
    public void LinuxReadOnlyMemoryMap_FileBacked_RoundTrips_From_File()
    {
        using var file = new LinuxMemoryFile("readonly-mapped-file");

        var expected = "readonly-map"u8;
        Assert.AreEqual(expected.Length, file.Write(expected));

        using var map = new LinuxReadOnlyMemoryMap(file.Descriptor, expected.Length);

        Assert.IsTrue(map.Span.SequenceEqual(expected));
    }

    [TestMethod]
    public void LinuxMemoryMap_Sync_SyncAndAsync_ThrowsLinuxException()
    {
        using var map = new LinuxMemoryMap(4);

        map.Sync(LinuxMemoryMapSync.Sync);
        map.Sync(LinuxMemoryMapSync.Async);
        map.Sync(LinuxMemoryMapSync.Sync | LinuxMemoryMapSync.Invalidate);
        map.Sync(LinuxMemoryMapSync.Async | LinuxMemoryMapSync.Invalidate);

        var e = Assert.ThrowsExactly<LinuxException>(() => map.Sync(LinuxMemoryMapSync.Sync | LinuxMemoryMapSync.Async));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
    }

    [TestMethod]
    public void LinuxMemoryMap_Memory_Span_AfterDispose_ThrowsObjectDisposedException()
    {
        Memory<byte> memory;
        using (var map = new LinuxMemoryMap(4))
        {
            memory = map.Memory;
        }

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = memory.Span);
    }

    [TestMethod]
    public void LinuxMemoryMap_Memory_Pin_AfterDispose_ThrowsObjectDisposedException()
    {
        Memory<byte> memory;
        using (var map = new LinuxMemoryMap(4))
            memory = map.Memory;

        Assert.ThrowsExactly<ObjectDisposedException>(() => memory.Pin());
    }

    [TestMethod]
    public void LinuxMemoryMap_Memory_Pin_AtLength_Succeeds()
    {
        using var map = new LinuxMemoryMap(4);

        Assert.IsTrue(MemoryMarshal.TryGetMemoryManager<byte, MemoryManager<byte>>(map.Memory, out var manager));
        using var handle = manager!.Pin(4);
    }

    [TestMethod]
    public void LinuxMemoryMap_Memory_Pin_BeyondLength_ThrowsArgumentOutOfRangeException()
    {
        using var map = new LinuxMemoryMap(4);

        Assert.IsTrue(MemoryMarshal.TryGetMemoryManager<byte, MemoryManager<byte>>(map.Memory, out var manager));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => manager!.Pin(5));
    }

    [TestMethod]
    public void LinuxMemoryMap_Anonymous_InvalidLength_ThrowsLinuxException()
    {
        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(0));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LinuxMemoryMap(-1));
    }

    [TestMethod]
    public void LinuxMemoryMap_FileBacked_InvalidLength_ThrowsLinuxException()
    {
        using var file = new LinuxMemoryFile("invalid-length");

        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(file.Descriptor, 0));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LinuxMemoryMap(file.Descriptor, -1));
    }

    [TestMethod]
    public void LinuxMemoryMap_FileBacked_NegativeOffset_ThrowsLinuxException()
    {
        using var file = new LinuxMemoryFile("negative-offset");

        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(file.Descriptor, 4, offset: -1));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
    }

    [TestMethod]
    public void LinuxMemoryMap_InvalidMappingType_ThrowsLinuxException()
    {
        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(4, flags: LinuxMemoryMapFlags.None));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
        e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(4, flags: LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Private));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
    }

    [TestMethod]
    public void LinuxMemoryMap_FileBacked_AnonymousFlag_ThrowsLinuxException()
    {
        using var file = new LinuxMemoryFile("anonymous-rejected");

        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(file.Descriptor, 4, flags: LinuxMemoryMapFlags.SharedValidate | LinuxMemoryMapFlags.Anonymous));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
    }

    [TestMethod]
    public void LinuxMemoryMap_Sync_AfterDispose_ThrowsObjectDisposedException()
    {
        var map = new LinuxMemoryMap(4);
        map.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => map.Sync());
    }

    [TestMethod]
    public void LinuxReadOnlyMemoryMap_Anonymous_Span_ReadsZeroBytes()
    {
        using var map = new LinuxReadOnlyMemoryMap(8);

        var span = map.Span;
        Assert.AreEqual(8, span.Length);
        foreach (var b in span)
            Assert.AreEqual(0, b);
    }

    [TestMethod]
    public void LinuxReadOnlyMemoryMap_Memory_AfterDispose_ThrowsObjectDisposedException()
    {
        ReadOnlyMemory<byte> memory;
        using (var map = new LinuxReadOnlyMemoryMap(4))
            memory = map.Memory;

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = memory.Span);
    }

    [TestMethod]
    public void LinuxMemoryMap_FileBacked_WithOffset_RoundTrips()
    {
        using var file = new LinuxMemoryFile("offset-map");

        var pageSize = (int)SystemConfiguration.Get(SystemConfigurationName.PageSize);
        var content = Encoding.ASCII.GetBytes("page-two-data");
        var zeroes = new byte[pageSize];
        Assert.AreEqual(pageSize, file.Write(zeroes));
        Assert.AreEqual(content.Length, file.Write(content));

        using var map = new LinuxMemoryMap(file.Descriptor, content.Length, offset: pageSize);
        Assert.IsTrue(map.Span[..content.Length].SequenceEqual(content));
    }

    [TestMethod]
    public void LinuxReadOnlyMemoryMap_FileBacked_ReflectsFileChanges()
    {
        using var file = new LinuxMemoryFile("shared-changes");

        var initial = "initial-data"u8;
        Assert.AreEqual(initial.Length, file.Write(initial));

        using var map = new LinuxReadOnlyMemoryMap(file.Descriptor, initial.Length);
        Assert.IsTrue(map.Span.SequenceEqual(initial));

        var updated = "updated-data"u8;
        file.Position = 0;
        Assert.AreEqual(updated.Length, file.Write(updated));

        Assert.IsTrue(map.Span.SequenceEqual(updated));
    }
}
