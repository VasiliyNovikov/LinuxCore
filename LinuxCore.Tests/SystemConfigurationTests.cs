using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class SystemConfigurationTests
{
    [TestMethod]
    public void SystemConfiguration_Get_PageSize_Returns_PowerOfTwo()
    {
        var pageSize = SystemConfiguration.Get(SystemConfigurationName.PageSize);
        Assert.IsGreaterThan(0L, pageSize);
        Assert.AreEqual(0L, pageSize & (pageSize - 1)); // power of two
    }

    [TestMethod]
    public void SystemConfiguration_Get_OpenMax_Returns_Positive()
    {
        var openMax = SystemConfiguration.Get(SystemConfigurationName.OpenMax);
        Assert.IsGreaterThan(0L, openMax);
    }

    [TestMethod]
    public void SystemConfiguration_Get_NprocessorsOnln_Returns_Positive()
    {
        var nproc = SystemConfiguration.Get(SystemConfigurationName.NprocessorsOnln);
        Assert.IsGreaterThan(0L, nproc);
    }

    [TestMethod]
    public void SystemConfiguration_Get_NprocessorsConf_Returns_GreaterOrEqual_NprocessorsOnln()
    {
        var conf = SystemConfiguration.Get(SystemConfigurationName.NprocessorsConf);
        var onln = SystemConfiguration.Get(SystemConfigurationName.NprocessorsOnln);
        Assert.IsGreaterThanOrEqualTo(onln, conf);
    }

    [TestMethod]
    public void SystemConfiguration_Get_ClkTck_Returns_Positive()
    {
        var clkTck = SystemConfiguration.Get(SystemConfigurationName.ClkTck);
        Assert.IsGreaterThan(0L, clkTck);
    }

    [TestMethod]
    public void SystemConfiguration_Get_Invalid_Name_Throws_LinuxException()
    {
        Assert.ThrowsExactly<LinuxException>(() => SystemConfiguration.Get((SystemConfigurationName)(-1)));
    }
}