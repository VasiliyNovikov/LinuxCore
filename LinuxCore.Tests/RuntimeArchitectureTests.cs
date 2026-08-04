using System;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class RuntimeArchitectureTests
{
    [TestMethod]
    public void Runtime_Architecture_Matches_PointerSize()
    {
        var expectedPointerSize = RuntimeInformation.ProcessArchitecture is Architecture.X86 or Architecture.Arm or Architecture.Armv6 ? 4 : 8;
        Assert.AreEqual(expectedPointerSize, IntPtr.Size);
    }

    [TestMethod]
    public void Runtime_Matches_CI_Architecture_Marker()
    {
        if (Environment.GetEnvironmentVariable("LINUXCORE_EXPECTED_ARCHITECTURE") is { } expectedArchitecture)
            Assert.AreEqual(expectedArchitecture.Equals("loong64", StringComparison.OrdinalIgnoreCase) ? Architecture.LoongArch64 : Enum.Parse<Architecture>(expectedArchitecture, true), RuntimeInformation.ProcessArchitecture);
        else
            Assert.Inconclusive("CI variable LINUXCORE_EXPECTED_ARCHITECTURE is missing");
    }

    [TestMethod]
    public void Runtime_Matches_CI_LibCImplementation_Marker()
    {
        if (Environment.GetEnvironmentVariable("LINUXCORE_EXPECTED_LIBC_IMPLEMENTATION") is { } expectedLibCImplementation)
            Assert.AreEqual(Enum.Parse<LibCImplementation>(expectedLibCImplementation, true), NativeAbi.LibCImplementation);
        else
            Assert.Inconclusive("CI variable LINUXCORE_EXPECTED_LIBC_IMPLEMENTATION is missing");
    }

    [TestMethod]
    public void Runtime_Matches_CI_Qemu_Linux_User_Marker()
    {
        if (Environment.GetEnvironmentVariable("LINUXCORE_EXPECTED_QEMU_LINUX_USER") is { } expectedQemuLinuxUser)
            Assert.AreEqual(bool.Parse(expectedQemuLinuxUser), NativeAbi.IsLikelyQemuLinuxUser);
        else
            Assert.Inconclusive("CI variable LINUXCORE_EXPECTED_QEMU_LINUX_USER is missing");
    }
}