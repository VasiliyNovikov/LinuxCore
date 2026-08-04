using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class SocketTests
{
    [TestMethod]
    public void Socket_Flags_Match_Current_Platform_Headers()
    {
        Assert.AreEqual((int)LinuxSocketType.NonBlocking, CScript.EvaluateInt32("SOCK_NONBLOCK", "sys/socket.h"));
        Assert.AreEqual((int)LinuxSocketType.CloseOnExec, CScript.EvaluateInt32("SOCK_CLOEXEC", "sys/socket.h"));
    }
}