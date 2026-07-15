using System;
using System.Globalization;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class RuntimeArchitectureTests
{
    [TestMethod]
    public void Runtime_Matches_CI_Architecture_Markers()
    {
        var expectedArchitecture = Environment.GetEnvironmentVariable("LINUXCORE_EXPECTED_ARCHITECTURE");
        var expectedPointerSize = Environment.GetEnvironmentVariable("LINUXCORE_EXPECTED_POINTER_SIZE");
        if (expectedArchitecture is null && expectedPointerSize is null)
            return;

        Assert.IsNotNull(expectedArchitecture, "LINUXCORE_EXPECTED_ARCHITECTURE must be set with LINUXCORE_EXPECTED_POINTER_SIZE.");
        Assert.IsNotNull(expectedPointerSize, "LINUXCORE_EXPECTED_POINTER_SIZE must be set with LINUXCORE_EXPECTED_ARCHITECTURE.");
        Assert.IsTrue(Enum.TryParse<Architecture>(expectedArchitecture, false, out var architecture), $"Invalid expected architecture: {expectedArchitecture}");
        Assert.IsTrue(int.TryParse(expectedPointerSize, NumberStyles.None, CultureInfo.InvariantCulture, out var pointerSize), $"Invalid expected pointer size: {expectedPointerSize}");
        Assert.AreEqual(architecture, RuntimeInformation.ProcessArchitecture);
        Assert.AreEqual(pointerSize, IntPtr.Size);
    }

    [TestMethod]
    public void Runtime_Matches_CI_Qemu_Linux_User_Marker()
    {
        if (Environment.GetEnvironmentVariable("LINUXCORE_EXPECTED_QEMU_LINUX_USER") is { } expectedQemuLinuxUser)
            Assert.AreEqual(bool.Parse(expectedQemuLinuxUser), NativeAbi.IsLikelyQemuLinuxUser);
    }
}