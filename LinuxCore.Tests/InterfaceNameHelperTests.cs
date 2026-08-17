using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class InterfaceNameHelperTests
{
    [TestMethod]
    public void InterfaceNameHelper_Constants_Match_Current_Platform_Headers()
    {
        Assert.AreEqual(NetIf.IF_NAMESIZE, CScript.EvaluateInt32("IF_NAMESIZE", "net/if.h"));
    }

    [TestMethod]
    public void InterfaceNameHelper_GetIndex()
    {
        var index = InterfaceNameHelper.GetIndex("lo");
        Assert.AreEqual(1, index);
    }

    [TestMethod]
    public void InterfaceNameHelper_GetIndex_NonExisting()
    {
        var error = Assert.ThrowsExactly<LinuxException>(() => InterfaceNameHelper.GetIndex("nonexisting"));
        Assert.AreEqual(LinuxErrorNumber.NoSuchDevice, error.ErrorNumber);
    }

    [TestMethod]
    public void InterfaceNameHelper_GetName()
    {
        var name = InterfaceNameHelper.GetName(1);
        Assert.AreEqual("lo", name);
    }

    [TestMethod]
    public void InterfaceNameHelper_GetName_NonExisting()
    {
        var error = Assert.ThrowsExactly<LinuxException>(() => InterfaceNameHelper.GetName(9999));
        Assert.AreEqual(LinuxErrorNumber.NoSuchDeviceOrAddress, error.ErrorNumber);
    }
}