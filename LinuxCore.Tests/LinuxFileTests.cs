using System;
using System.Diagnostics;
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
            var expectedINode = ulong.Parse(Process.Start(new ProcessStartInfo("stat", ["-c", "%i", filePath]) { RedirectStandardOutput = true })!.StandardOutput.ReadToEnd().Trim(), CultureInfo.InvariantCulture);
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);
            Assert.AreEqual(expectedINode, file.INode);
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
    public void Linux_File_DeviceId()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);
            Assert.IsTrue(file.DeviceId > 0, $"DeviceId {file.DeviceId} should be positive");
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public void Linux_File_CloseOnExec_Setter()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            using var file = new LinuxFile(filePath, LinuxFileFlags.ReadOnly);

            Assert.IsTrue(file.CloseOnExec);

            file.CloseOnExec = false;
            Assert.IsFalse(file.CloseOnExec);

            file.CloseOnExec = true;
            Assert.IsTrue(file.CloseOnExec);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}