using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
    public void LinuxMemoryMap_FileBacked_Large_Offset_RoundTrips()
    {
        const long offset = 1L << 32;
        var pageSize = checked((int)SystemConfiguration.Get(SystemConfigurationName.PageSize));
        var filePath = Path.GetTempFileName();
        try
        {
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadWrite | LinuxFileFlags.Truncate);
            file.Position = offset + pageSize - 1;
            Assert.AreEqual(1, file.Write("\0"u8));

            using (var map = new LinuxMemoryMap(file.Descriptor, pageSize, offset: offset))
            {
                "large-map"u8.CopyTo(map.Span);
                map.Sync();
            }

            file.Position = offset;
            Span<byte> actual = stackalloc byte[9];
            Assert.AreEqual(actual.Length, file.Read(actual));
            Assert.IsTrue(actual.SequenceEqual("large-map"u8));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public unsafe void MemoryMap_Munmap_DirectSyscall_Succeeds()
    {
        const nuint length = 4096;
        var address = Interop.MemoryMap.mmap(
            null,
            length,
            LinuxMemoryProtection.Read | LinuxMemoryProtection.Write,
            NativeLinuxMemoryMapFlags.ToNative(LinuxMemoryMapFlags.Private | LinuxMemoryMapFlags.Anonymous),
            new FileDescriptor(-1),
            0).ThrowIfError();

        var unmapped = false;
        try
        {
            *(byte*)address = 1;
            Interop.MemoryMap.munmap((void*)address, length).ThrowIfError();
            unmapped = true;
        }
        finally
        {
            if (!unmapped)
                Interop.MemoryMap.munmap((void*)address, length);
        }
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
    public void LinuxMemoryMap_FileBacked_UnalignedOffset_ThrowsLinuxException()
    {
        using var file = new LinuxMemoryFile("unaligned-offset");

        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(file.Descriptor, 4, offset: 1));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
    }

    [TestMethod]
    public void LinuxMemoryMap_FileBacked_32BitArch_OutOfRangeOffset_ThrowsLinuxException()
    {
        if (Interop.NativeAbi.Is64Bit)
            return;

        using var file = new LinuxMemoryFile("32bitarch-out-of-range-offset");

        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(file.Descriptor, 4, offset: 1L << 44));
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
    public void LinuxMemoryMap_Locked_Respects_Memlock_Limit()
    {
        using var unlocked = new LinuxMemoryMap(4096);
        using var locked = new LinuxMemoryMap(4096, LinuxMemoryMapFlags.Private | LinuxMemoryMapFlags.Locked);
        var (originalSoft, originalHard) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.MemoryLock);
        try
        {
            LinuxResourceLimit.Set(LinuxResourceLimit.Resource.MemoryLock, 0, originalHard);
            var exception = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryMap(4096, LinuxMemoryMapFlags.Private | LinuxMemoryMapFlags.Locked));
            Assert.IsTrue(exception.ErrorNumber is LinuxErrorNumber.TryAgain or LinuxErrorNumber.OperationNotPermitted or LinuxErrorNumber.OutOfMemory, $"Unexpected MAP_LOCKED error: {exception.ErrorNumber}");
        }
        finally
        {
            LinuxResourceLimit.Set(LinuxResourceLimit.Resource.MemoryLock, originalSoft, originalHard);
        }
    }

    [TestMethod]
    public void LinuxMemoryMap_Constants_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.EnumValuesMatch<LinuxMemoryProtection>(
        [
            (nameof(LinuxMemoryProtection.None), "PROT_NONE"),
            (nameof(LinuxMemoryProtection.Read), "PROT_READ"),
            (nameof(LinuxMemoryProtection.Write), "PROT_WRITE"),
            (nameof(LinuxMemoryProtection.Execute), "PROT_EXEC")
        ], "sys/mman.h");

        NativeConstantAssert.EnumValuesMatch<LinuxMemoryMapSync>(
        [
            (nameof(LinuxMemoryMapSync.Sync), "MS_SYNC"),
            (nameof(LinuxMemoryMapSync.Async), "MS_ASYNC"),
            (nameof(LinuxMemoryMapSync.Invalidate), "MS_INVALIDATE")
        ], "sys/mman.h");
    }

    [TestMethod]
    public void LinuxMemoryMap_Flag_Translation_Matches_Current_Platform_Headers()
    {
        (LinuxMemoryMapFlags Managed, string Native)[] constants =
        [
            (LinuxMemoryMapFlags.None, "0"),
            (LinuxMemoryMapFlags.Shared, "MAP_SHARED"),
            (LinuxMemoryMapFlags.Private, "MAP_PRIVATE"),
            (LinuxMemoryMapFlags.SharedValidate, "MAP_SHARED_VALIDATE"),
            (LinuxMemoryMapFlags.Droppable, "MAP_DROPPABLE"),
            (LinuxMemoryMapFlags.Fixed, "MAP_FIXED"),
            (LinuxMemoryMapFlags.Anonymous, "MAP_ANONYMOUS"),
            (LinuxMemoryMapFlags.GrowsDown, "MAP_GROWSDOWN"),
            (LinuxMemoryMapFlags.Locked, "MAP_LOCKED"),
            (LinuxMemoryMapFlags.NoReserve, "MAP_NORESERVE"),
            (LinuxMemoryMapFlags.Populate, "MAP_POPULATE"),
            (LinuxMemoryMapFlags.NonBlocking, "MAP_NONBLOCK"),
            (LinuxMemoryMapFlags.FixedNoReplace, "MAP_FIXED_NOREPLACE"),
            (LinuxMemoryMapFlags.Uninitialized, "MAP_UNINITIALIZED"),
            (LinuxMemoryMapFlags.HugeTLB, "MAP_HUGETLB"),
            (LinuxMemoryMapFlags.Huge2M, "MAP_HUGETLB | MAP_HUGE_2MB"),
            (LinuxMemoryMapFlags.Huge1G, "MAP_HUGETLB | MAP_HUGE_1GB")
        ];
        Assert.AreSequenceEqual(Enum.GetValues<LinuxMemoryMapFlags>(), constants.Select(static constant => constant.Managed), SequenceOrder.InAnyOrder);

        (string symbol, string Native)[] nativeConstants =
        [
            .. constants.Select(static constant =>
            {
                var symbol = constant.Managed switch
                {
                    LinuxMemoryMapFlags.None => "MAP_SHARED",
                    LinuxMemoryMapFlags.Huge2M => "MAP_HUGE_2MB",
                    LinuxMemoryMapFlags.Huge1G => "MAP_HUGE_1GB",
                    _ => constant.Native
                };
                return (symbol, constant.Native);
            })
        ];
        var nativeValues = CScript.EvaluateDefinedInt32s(nativeConstants, "linux/mman.h");
        for (var i = 0; i < constants.Length; ++i)
        {
            if (nativeValues[i] is { } nativeValue)
                Assert.AreEqual(nativeValue, NativeLinuxMemoryMapFlags.ToNative(constants[i].Managed), constants[i].Managed.ToString());
            else
                Assert.AreEqual(LinuxMemoryMapFlags.Droppable, constants[i].Managed, $"Unexpected undefined native constant: {nativeConstants[i].symbol}");
        }
    }
}