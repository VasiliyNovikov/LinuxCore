using System;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxFileTests
{
    [TestMethod]
    public void Linux_File_Read()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            const string expectedContent = "Hello, Linux File System!";
            File.WriteAllText(filePath, expectedContent);

            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);
            Span<byte> buffer = stackalloc byte[expectedContent.Length];
            Assert.AreEqual(expectedContent.Length, file.Read(buffer));
            var actualContent = Encoding.ASCII.GetString(buffer);

            Assert.AreEqual(expectedContent, actualContent);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_Size()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            const string content = "Hello, Linux File System!";
            File.WriteAllText(filePath, content);

            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);
            Assert.AreEqual(content.Length, file.Size);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_INode()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            var expectedINode = ulong.Parse(Script.Run("stat", "-c", "%i", filePath), CultureInfo.InvariantCulture);
            var expectedDeviceId = ulong.Parse(Script.Run("stat", "-c", "%d", filePath), CultureInfo.InvariantCulture);
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);
            Assert.AreEqual(expectedINode, file.INode);
            Assert.AreEqual(expectedDeviceId, file.DeviceId);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_Write()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            const string contentToWrite = "Writing to Linux File System!";
            using (var file = new LinuxFile(filePath, LinuxFileFlags.WriteOnly | LinuxFileFlags.Create))
            {
                var buffer = Encoding.ASCII.GetBytes(contentToWrite);
                Assert.AreEqual(contentToWrite.Length, file.Write(buffer));
            }

            var actualContent = File.ReadAllText(filePath);
            Assert.AreEqual(contentToWrite, actualContent);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_ReadExactly_And_WriteExactly()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadWrite | LinuxFileFlags.Truncate);
            var expected = "exact-content"u8;
            file.WriteExactly(expected);
            file.Position = 0;

            Span<byte> actual = stackalloc byte[expected.Length];
            file.ReadExactly(actual);
            Assert.IsTrue(actual.SequenceEqual(expected));
            _ = Assert.ThrowsExactly<EndOfStreamException>(() => file.ReadExactly(new byte[1]));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_Seek()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            const string content = "Hello, Linux File System!";
            File.WriteAllText(filePath, content);

            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);

            // Seek from beginning
            Assert.AreEqual(7L, file.Seek(7, LinuxSeekOrigin.Begin));

            // Read from seeked position
            Span<byte> buffer = stackalloc byte[5];
            Assert.AreEqual(5, file.Read(buffer));
            Assert.AreEqual("Linux", Encoding.ASCII.GetString(buffer));

            // Seek from current position
            Assert.AreEqual(18L, file.Seek(6, LinuxSeekOrigin.Current));

            // Seek from end
            Assert.AreEqual(content.Length - 7, file.Seek(-7, LinuxSeekOrigin.End));
            buffer = stackalloc byte[7];
            Assert.AreEqual(7, file.Read(buffer));
            Assert.AreEqual("System!", Encoding.ASCII.GetString(buffer));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_Position()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            const string content = "Hello, Linux File System!";
            File.WriteAllText(filePath, content);

            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);

            // Initial position is 0
            Assert.AreEqual(0L, file.Position);

            // Read advances position
            Span<byte> buffer = stackalloc byte[5];
            file.Read(buffer);
            Assert.AreEqual(5L, file.Position);

            // Set position
            file.Position = 7;
            Assert.AreEqual(7L, file.Position);

            // Read from new position
            file.Read(buffer);
            Assert.AreEqual("Linux", Encoding.ASCII.GetString(buffer));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_Large_Sparse_Offset_RoundTrips()
    {
        const long offset = (1L << 32) + 4096;
        var filePath = Path.GetTempFileName();
        try
        {
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadWrite | LinuxFileFlags.Truncate);
            Assert.AreEqual(offset, file.Seek(offset, LinuxSeekOrigin.Begin));
            Assert.AreEqual(1, file.Write("x"u8));
            Assert.AreEqual(offset + 1, file.Size);
            Assert.AreEqual(offset, file.Seek(-1, LinuxSeekOrigin.End));

            Span<byte> actual = stackalloc byte[1];
            Assert.AreEqual(1, file.Read(actual));
            Assert.IsTrue(actual.SequenceEqual("x"u8));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_NoFollow_Rejects_Symbolic_Link()
    {
        var directory = Directory.CreateTempSubdirectory("linuxcore-nofollow-");
        try
        {
            var target = Path.Combine(directory.FullName, "target");
            var link = Path.Combine(directory.FullName, "link");
            File.WriteAllText(target, "target");
            File.CreateSymbolicLink(link, target);

            var exception = Assert.ThrowsExactly<LinuxException>(() => new LinuxFile(link, LinuxFileFlags.ReadOnly | LinuxFileFlags.NoFollow));
            Assert.AreEqual(LinuxErrorNumber.TooManySymbolicLinks, exception.ErrorNumber);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [TestMethod]
    public void Linux_File_Directory_Requires_A_Directory()
    {
        var directory = Directory.CreateTempSubdirectory("linuxcore-directory-");
        try
        {
            using var directoryFile = new LinuxFile(directory.FullName, LinuxFileFlags.ReadOnly | LinuxFileFlags.Directory);
            Assert.AreEqual(LinuxFileFlags.Directory, directoryFile.Flags & LinuxFileFlags.Directory);

            var regularFile = Path.Combine(directory.FullName, "file");
            File.WriteAllText(regularFile, "file");
            var exception = Assert.ThrowsExactly<LinuxException>(() => new LinuxFile(regularFile, LinuxFileFlags.ReadOnly | LinuxFileFlags.Directory));
            Assert.AreEqual(LinuxErrorNumber.NotADirectory, exception.ErrorNumber);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [TestMethod]
    public void Linux_File_Direct_Uses_Native_Flag()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var file = new LinuxFile(path, LinuxFileFlags.ReadWrite | LinuxFileFlags.Direct);
            Assert.AreEqual(LinuxFileFlags.Direct, file.Flags & LinuxFileFlags.Direct);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Linux_File_TmpFile_Creates_And_Writes()
    {
        using var file = new LinuxFile(Path.GetTempPath(), LinuxFileFlags.ReadWrite | LinuxFileFlags.TmpFile, LinuxFileMode.UserRead | LinuxFileMode.UserWrite);
        Assert.AreEqual(LinuxFileFlags.Directory, file.Flags & LinuxFileFlags.Directory);
        Assert.AreEqual(4, file.Write("test"u8));
    }

    [TestMethod]
    public void Linux_File_Flag_Translation_Matches_Current_Platform_Headers()
    {
        AssertFileFlag(LinuxFileFlags.ReadOnly, "O_RDONLY");
        AssertFileFlag(LinuxFileFlags.WriteOnly, "O_WRONLY");
        AssertFileFlag(LinuxFileFlags.ReadWrite, "O_RDWR");
        AssertFileFlag(LinuxFileFlags.Append, "O_APPEND");
        AssertFileFlag(LinuxFileFlags.NonBlock, "O_NONBLOCK");
        AssertFileFlag(LinuxFileFlags.Direct, "O_DIRECT");
        AssertFileFlag(LinuxFileFlags.LargeFile, "O_LARGEFILE");
        AssertFileFlag(LinuxFileFlags.Directory, "O_DIRECTORY");
        AssertFileFlag(LinuxFileFlags.NoFollow, "O_NOFOLLOW");
        AssertFileFlag(LinuxFileFlags.CloseOnExec, "O_CLOEXEC");
        AssertFileFlag(LinuxFileFlags.TmpFile, "O_TMPFILE");

        return;

        static void AssertFileFlag(LinuxFileFlags managed, string nativeName)
        {
            var native = CScript.EvaluateInt32(nativeName, "asm/fcntl.h");
            Assert.AreEqual(native, NativeLinuxFileFlags.ToNative(managed));
            Assert.AreEqual(managed, NativeLinuxFileFlags.FromNative(native));
        }
    }
}