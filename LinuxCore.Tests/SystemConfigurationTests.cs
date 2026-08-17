using System;
using System.Linq;
using System.Text.Json;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class SystemConfigurationTests
{
    [TestMethod]
    public void SystemConfigurationName_Values_Match_Current_Platform_Headers()
    {
        var names = Enum.GetNames<SystemConfigurationName>();
        var nativeNames = names.Select(static name => name switch
        {
            nameof(SystemConfigurationName.NGroupsMax) => "_SC_NGROUPS_MAX",
            nameof(SystemConfigurationName.TzNameMax) => "_SC_TZNAME_MAX",
            nameof(SystemConfigurationName.RealTimeSignals) => "_SC_REALTIME_SIGNALS",
            nameof(SystemConfigurationName.FSync) => "_SC_FSYNC",
            nameof(SystemConfigurationName.MemLock) => "_SC_MEMLOCK",
            nameof(SystemConfigurationName.MemLockRange) => "_SC_MEMLOCK_RANGE",
            nameof(SystemConfigurationName.PageSize) => "_SC_PAGESIZE",
            nameof(SystemConfigurationName.RtSigMax) => "_SC_RTSIG_MAX",
            nameof(SystemConfigurationName.SigQueueMax) => "_SC_SIGQUEUE_MAX",
            nameof(SystemConfigurationName.CharClassNameMax) => "_SC_CHARCLASS_NAME_MAX",
            nameof(SystemConfigurationName.Posix2Version) => "_SC_2_VERSION",
            nameof(SystemConfigurationName.Posix2CBind) => "_SC_2_C_BIND",
            nameof(SystemConfigurationName.Posix2CDev) => "_SC_2_C_DEV",
            nameof(SystemConfigurationName.Posix2FortDev) => "_SC_2_FORT_DEV",
            nameof(SystemConfigurationName.Posix2FortRun) => "_SC_2_FORT_RUN",
            nameof(SystemConfigurationName.Posix2SwDev) => "_SC_2_SW_DEV",
            nameof(SystemConfigurationName.Posix2Localedef) => "_SC_2_LOCALEDEF",
            nameof(SystemConfigurationName.UioMaxiov) => "_SC_UIO_MAXIOV",
            nameof(SystemConfigurationName.IovMax) => "_SC_IOV_MAX",
            nameof(SystemConfigurationName.TIovMax) => "_SC_T_IOV_MAX",
            nameof(SystemConfigurationName.GetGrRSizeMax) => "_SC_GETGR_R_SIZE_MAX",
            nameof(SystemConfigurationName.GetPwRSizeMax) => "_SC_GETPW_R_SIZE_MAX",
            nameof(SystemConfigurationName.NprocessorsConf) => "_SC_NPROCESSORS_CONF",
            nameof(SystemConfigurationName.NprocessorsOnln) => "_SC_NPROCESSORS_ONLN",
            nameof(SystemConfigurationName.AvphysPages) => "_SC_AVPHYS_PAGES",
            nameof(SystemConfigurationName.XOpenVersion) => "_SC_XOPEN_VERSION",
            nameof(SystemConfigurationName.XOpenXcuVersion) => "_SC_XOPEN_XCU_VERSION",
            nameof(SystemConfigurationName.XOpenUnix) => "_SC_XOPEN_UNIX",
            nameof(SystemConfigurationName.XOpenCrypt) => "_SC_XOPEN_CRYPT",
            nameof(SystemConfigurationName.XOpenEnhI18n) => "_SC_XOPEN_ENH_I18N",
            nameof(SystemConfigurationName.XOpenShm) => "_SC_XOPEN_SHM",
            nameof(SystemConfigurationName.Posix2CharTerm) => "_SC_2_CHAR_TERM",
            nameof(SystemConfigurationName.Posix2CVersion) => "_SC_2_C_VERSION",
            nameof(SystemConfigurationName.Posix2Upe) => "_SC_2_UPE",
            nameof(SystemConfigurationName.XopenXpg2) => "_SC_XOPEN_XPG2",
            nameof(SystemConfigurationName.XopenXpg3) => "_SC_XOPEN_XPG3",
            nameof(SystemConfigurationName.XopenXpg4) => "_SC_XOPEN_XPG4",
            nameof(SystemConfigurationName.CLangSupport) => "_SC_C_LANG_SUPPORT",
            nameof(SystemConfigurationName.CLangSupportR) => "_SC_C_LANG_SUPPORT_R",
            nameof(SystemConfigurationName.Cputime) => "_SC_CPUTIME",
            nameof(SystemConfigurationName.Posix2Pbs) => "_SC_2_PBS",
            nameof(SystemConfigurationName.Posix2PbsAccounting) => "_SC_2_PBS_ACCOUNTING",
            nameof(SystemConfigurationName.Posix2PbsLocate) => "_SC_2_PBS_LOCATE",
            nameof(SystemConfigurationName.Posix2PbsMessage) => "_SC_2_PBS_MESSAGE",
            nameof(SystemConfigurationName.Posix2PbsTrack) => "_SC_2_PBS_TRACK",
            nameof(SystemConfigurationName.Posix2PbsCheckpoint) => "_SC_2_PBS_CHECKPOINT",
            nameof(SystemConfigurationName.SSizeMax) => "_SC_SSIZE_MAX",
            nameof(SystemConfigurationName.SCharMax) => "_SC_SCHAR_MAX",
            nameof(SystemConfigurationName.SCharMin) => "_SC_SCHAR_MIN",
            nameof(SystemConfigurationName.UCharMax) => "_SC_UCHAR_MAX",
            nameof(SystemConfigurationName.UIntMax) => "_SC_UINT_MAX",
            nameof(SystemConfigurationName.ULongMax) => "_SC_ULONG_MAX",
            nameof(SystemConfigurationName.UShrtMax) => "_SC_USHRT_MAX",
            nameof(SystemConfigurationName.Level1ICacheSize) => "_SC_LEVEL1_ICACHE_SIZE",
            nameof(SystemConfigurationName.Level1ICacheAssoc) => "_SC_LEVEL1_ICACHE_ASSOC",
            nameof(SystemConfigurationName.Level1ICacheLinesize) => "_SC_LEVEL1_ICACHE_LINESIZE",
            nameof(SystemConfigurationName.Level1DCacheSize) => "_SC_LEVEL1_DCACHE_SIZE",
            nameof(SystemConfigurationName.Level1DCacheAssoc) => "_SC_LEVEL1_DCACHE_ASSOC",
            nameof(SystemConfigurationName.Level1DCacheLinesize) => "_SC_LEVEL1_DCACHE_LINESIZE",
            nameof(SystemConfigurationName.Level2CacheSize) => "_SC_LEVEL2_CACHE_SIZE",
            nameof(SystemConfigurationName.Level2CacheAssoc) => "_SC_LEVEL2_CACHE_ASSOC",
            nameof(SystemConfigurationName.Level2CacheLinesize) => "_SC_LEVEL2_CACHE_LINESIZE",
            nameof(SystemConfigurationName.Level3CacheSize) => "_SC_LEVEL3_CACHE_SIZE",
            nameof(SystemConfigurationName.Level3CacheAssoc) => "_SC_LEVEL3_CACHE_ASSOC",
            nameof(SystemConfigurationName.Level3CacheLinesize) => "_SC_LEVEL3_CACHE_LINESIZE",
            nameof(SystemConfigurationName.Level4CacheSize) => "_SC_LEVEL4_CACHE_SIZE",
            nameof(SystemConfigurationName.Level4CacheAssoc) => "_SC_LEVEL4_CACHE_ASSOC",
            nameof(SystemConfigurationName.Level4CacheLinesize) => "_SC_LEVEL4_CACHE_LINESIZE",
            nameof(SystemConfigurationName.XOpenStreams) => "_SC_XOPEN_STREAMS",
            _ => $"_SC_{JsonNamingPolicy.SnakeCaseUpper.ConvertName(name)}"
        }).ToArray();
        var nativeValues = CScript.EvaluateDefinedInt32s(nativeNames, "unistd.h");

        for (var i = 0; i < names.Length; ++i)
        {
            if (nativeValues[i] is { } nativeValue)
                Assert.AreEqual((int)Enum.Parse<SystemConfigurationName>(names[i]), nativeValue, names[i]);
            else
                Assert.IsTrue(NativeAbi.LibCImplementation == LibCImplementation.Musl || names[i] is nameof(SystemConfigurationName.Minsigstksz) or nameof(SystemConfigurationName.Sigstksz), $"Missing glibc constant {nativeNames[i]}");
        }
    }

    [TestMethod]
    public void SystemConfiguration_Get_PageSize_Returns_PowerOfTwo()
    {
        var pageSize = SystemConfiguration.Get(SystemConfigurationName.PageSize);
        Assert.IsGreaterThan(0L, pageSize);
        Assert.AreEqual(0L, pageSize & (pageSize - 1)); // power of two
        Assert.AreEqual(CScript.EvaluateInt64("sysconf(_SC_PAGESIZE)", "unistd.h"), pageSize);
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