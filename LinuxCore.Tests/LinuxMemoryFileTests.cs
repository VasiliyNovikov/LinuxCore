using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxMemoryFileTests
{
    [TestMethod]
    public void LinuxMemoryFile_Create_Write_And_Read_RoundTrips()
    {
        using var file = new LinuxMemoryFile("roundtrip");

        const string expectedContent = "Hello, Linux memory file!";
        var buffer = Encoding.ASCII.GetBytes(expectedContent);

        Assert.AreEqual(expectedContent.Length, file.Write(buffer));
        Assert.AreEqual(expectedContent.Length, file.Size);
        Assert.AreEqual(expectedContent, File.ReadAllText($"/proc/self/fd/{file.Descriptor}"));
    }

    [TestMethod]
    public void LinuxMemoryFile_Create_Sets_CloseOnExec()
    {
        using var file = new LinuxMemoryFile("cloexec");
        Assert.IsTrue(file.CloseOnExec);
    }

    [TestMethod]
    public void LinuxMemoryFile_WrappingConstructor_DoesNotChangeCloseOnExec()
    {
        using var original = new LinuxMemoryFile("wrapped");
        using var duplicate = new LinuxMemoryFile(original.Descriptor.Clone());

        Assert.IsTrue(original.CloseOnExec);
        Assert.IsFalse(duplicate.CloseOnExec);
    }

    [TestMethod]
    public void LinuxMemoryFile_AddSeals_RoundTrips_WhenAllowSealing()
    {
        using var file = new LinuxMemoryFile("sealable", LinuxMemoryFileFlags.AllowSealing);

        const LinuxMemoryFileSeals expectedSeals = LinuxMemoryFileSeals.Shrink | LinuxMemoryFileSeals.Grow;

        Assert.AreEqual(LinuxMemoryFileSeals.None, file.Seals);

        file.AddSeals(expectedSeals);

        Assert.AreEqual(expectedSeals, file.Seals);
    }

    [TestMethod]
    public void LinuxMemoryFile_AddSeals_WithoutAllowSealing_ThrowsLinuxException()
    {
        using var file = new LinuxMemoryFile("sealed-by-default");

        Assert.AreEqual(LinuxMemoryFileSeals.Seal, file.Seals);

        var error = Assert.ThrowsExactly<LinuxException>(() => file.AddSeals(LinuxMemoryFileSeals.Write));
        Assert.AreEqual(LinuxErrorNumber.OperationNotPermitted, error.ErrorNumber);
    }

    [TestMethod]
    public void LinuxMemoryFile_Create_InvalidName_ThrowsLinuxException()
    {
        var invalidName = new string('a', 250);
        var error = Assert.ThrowsExactly<LinuxException>(() => new LinuxMemoryFile(invalidName));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, error.ErrorNumber);
    }
}