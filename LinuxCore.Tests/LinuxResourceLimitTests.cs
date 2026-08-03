using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxResourceLimitTests
{
    [TestMethod]
    public void LinuxResourceLimitTests_Get_NumOpenFiles_Returns_Valid_Limits()
    {
        var (soft, hard) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.NumOpenFiles);
        Assert.IsGreaterThan(0ul, soft);
        Assert.IsGreaterThanOrEqualTo(soft, hard);
    }

    [TestMethod]
    public void LinuxResourceLimitTests_Set_And_Get_CoreSize_RoundTrips()
    {
        var (originalSoft, originalHard) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.Core);
        try
        {
            LinuxResourceLimit.Set(LinuxResourceLimit.Resource.Core, 0, originalHard);
            var (newSoft, _) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.Core);
            Assert.AreEqual(0UL, newSoft);
        }
        finally
        {
            LinuxResourceLimit.Set(LinuxResourceLimit.Resource.Core, originalSoft, originalHard);
        }
    }

    [TestMethod]
    public void LinuxResourceLimitTests_Get_Invalid_Resource_Throws_LinuxException()
    {
        Assert.ThrowsExactly<LinuxException>(() => LinuxResourceLimit.Get((LinuxResourceLimit.Resource)(-1)));
    }

    [TestMethod]
    public void LinuxResourceLimitTests_Set_Soft_Above_Hard_Throws_LinuxException()
    {
        var (_, hard) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.Core);
        if (hard == LinuxResourceLimit.Infinity)
            return;

        Assert.ThrowsExactly<LinuxException>(() => LinuxResourceLimit.Set(LinuxResourceLimit.Resource.Core, hard + 1, hard));
    }

    [TestMethod]
    public void LinuxResourceLimitTests_Set_MemoryLock_Limit_Test()
    {
        var (originalSoft, originalHard) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.MemoryLock);
        try
        {
            LinuxResourceLimit.Set(LinuxResourceLimit.Resource.MemoryLock, 0, originalHard);
            var (newSoft, _) = LinuxResourceLimit.Get(LinuxResourceLimit.Resource.MemoryLock);
            Assert.AreEqual(0UL, newSoft);
        }
        finally
        {
            LinuxResourceLimit.Set(LinuxResourceLimit.Resource.MemoryLock, originalSoft, originalHard);
        }
    }
}