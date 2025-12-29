using System;
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
}