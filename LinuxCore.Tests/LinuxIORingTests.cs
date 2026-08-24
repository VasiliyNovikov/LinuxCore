using System;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static LinuxCore.Interop.File;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxIORingTests
{
    [TestMethod]
    public void LinuxIORingFlags_Constants_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.EnumValuesMatch<LinuxIORingFlags>(
        [
            (nameof(LinuxIORingFlags.None), "0"),
            (nameof(LinuxIORingFlags.IOPoll), "IORING_SETUP_IOPOLL"),
            (nameof(LinuxIORingFlags.SQPoll), "IORING_SETUP_SQPOLL"),
            (nameof(LinuxIORingFlags.SQAffinity), "IORING_SETUP_SQ_AFF")
        ],
        [
            (nameof(LinuxIORingFlags.CQSize), "IORING_SETUP_CQSIZE"),
            (nameof(LinuxIORingFlags.Clamp), "IORING_SETUP_CLAMP"),
            (nameof(LinuxIORingFlags.AttachWQ), "IORING_SETUP_ATTACH_WQ"),
            (nameof(LinuxIORingFlags.Disabled), "IORING_SETUP_R_DISABLED"),
            (nameof(LinuxIORingFlags.SubmitAll), "IORING_SETUP_SUBMIT_ALL"),
            (nameof(LinuxIORingFlags.CoopTaskRun), "IORING_SETUP_COOP_TASKRUN"),
            (nameof(LinuxIORingFlags.TaskRunFlag), "IORING_SETUP_TASKRUN_FLAG"),
            (nameof(LinuxIORingFlags.SQE128), "IORING_SETUP_SQE128"),
            (nameof(LinuxIORingFlags.CQE32), "IORING_SETUP_CQE32"),
            (nameof(LinuxIORingFlags.SingleIssuer), "IORING_SETUP_SINGLE_ISSUER"),
            (nameof(LinuxIORingFlags.DeferTaskRun), "IORING_SETUP_DEFER_TASKRUN"),
            (nameof(LinuxIORingFlags.NoMmap), "IORING_SETUP_NO_MMAP"),
            (nameof(LinuxIORingFlags.RegisteredFdOnly), "IORING_SETUP_REGISTERED_FD_ONLY"),
            (nameof(LinuxIORingFlags.NoSQArray), "IORING_SETUP_NO_SQARRAY"),
            (nameof(LinuxIORingFlags.HybridIOPoll), "IORING_SETUP_HYBRID_IOPOLL"),
            (nameof(LinuxIORingFlags.CQEMixed), "IORING_SETUP_CQE_MIXED"),
            (nameof(LinuxIORingFlags.SQEMixed), "IORING_SETUP_SQE_MIXED"),
            (nameof(LinuxIORingFlags.SQRewind), "IORING_SETUP_SQ_REWIND")
        ], "linux/io_uring.h");
    }

    [TestMethod]
    public void LinuxIORingFeatures_Constants_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.EnumValuesMatch<LinuxIORingFeatures>(
        [
            (nameof(LinuxIORingFeatures.SingleMemoryMap), "IORING_FEAT_SINGLE_MMAP")
        ],
        [
            (nameof(LinuxIORingFeatures.NoDrop), "IORING_FEAT_NODROP"),
            (nameof(LinuxIORingFeatures.SubmitStable), "IORING_FEAT_SUBMIT_STABLE"),
            (nameof(LinuxIORingFeatures.ReadWriteCurrentPosition), "IORING_FEAT_RW_CUR_POS"),
            (nameof(LinuxIORingFeatures.CurrentPersonality), "IORING_FEAT_CUR_PERSONALITY"),
            (nameof(LinuxIORingFeatures.FastPoll), "IORING_FEAT_FAST_POLL"),
            (nameof(LinuxIORingFeatures.Poll32Bits), "IORING_FEAT_POLL_32BITS"),
            (nameof(LinuxIORingFeatures.SQPollNonFixed), "IORING_FEAT_SQPOLL_NONFIXED"),
            (nameof(LinuxIORingFeatures.ExtendedArgs), "IORING_FEAT_EXT_ARG"),
            (nameof(LinuxIORingFeatures.NativeWorkers), "IORING_FEAT_NATIVE_WORKERS"),
            (nameof(LinuxIORingFeatures.RSRCTags), "IORING_FEAT_RSRC_TAGS"),
            (nameof(LinuxIORingFeatures.CQESkip), "IORING_FEAT_CQE_SKIP"),
            (nameof(LinuxIORingFeatures.LinkedFile), "IORING_FEAT_LINKED_FILE"),
            (nameof(LinuxIORingFeatures.RegisterRegisteredRing), "IORING_FEAT_REG_REG_RING"),
            (nameof(LinuxIORingFeatures.ReceiveSendBundle), "IORING_FEAT_RECVSEND_BUNDLE"),
            (nameof(LinuxIORingFeatures.MinTimeout), "IORING_FEAT_MIN_TIMEOUT"),
            (nameof(LinuxIORingFeatures.ReadWriteAttributes), "IORING_FEAT_RW_ATTR"),
            (nameof(LinuxIORingFeatures.NoIOWait), "IORING_FEAT_NO_IOWAIT")
        ], "linux/io_uring.h");
    }

    [TestMethod]
    public void LinuxIORing_IsSupported() => Assert.AreNotEqual(NativeAbi.IsLikelyQemuLinuxUser, LinuxIORing.IsSupported);

    [TestMethod]
    public void LinuxIORing_Create()
    {
        if (NativeAbi.IsLikelyQemuLinuxUser)
            return;

        using var ring = new LinuxIORing(32);
        Assert.AreEqual(LinuxIORingFlags.None, ring.Flags);
        Assert.IsTrue(ring.Features.HasFlag(LinuxIORingFeatures.SingleMemoryMap));
        Assert.IsTrue(ring.Features.HasFlag(LinuxIORingFeatures.NoDrop));
        Assert.IsGreaterThanOrEqualTo(32, ring.SubmissionQueueSize);
        Assert.IsGreaterThanOrEqualTo(32, ring.CompletionQueueSize);
    }

    [TestMethod]
    public void LinuxIORing_Create_FailsOnInvalidSize()
    {
        if (NativeAbi.IsLikelyQemuLinuxUser)
            return;

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LinuxIORing(-1));
        var e = Assert.ThrowsExactly<LinuxException>(() => new LinuxIORing(0));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
        e = Assert.ThrowsExactly<LinuxException>(() => new LinuxIORing(int.MaxValue));
        Assert.AreEqual(LinuxErrorNumber.InvalidArgument, e.ErrorNumber);
    }

    [TestMethod]
    public void LinuxIORing_FailedConstruction_DoesNotCloseStandardInput()
    {
        fcntl(FileDescriptor.StandardInput, F_GETFD).ThrowIfError();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LinuxIORing(-1));

        GC.Collect();
        GC.WaitForPendingFinalizers();

        fcntl(FileDescriptor.StandardInput, F_GETFD).ThrowIfError();
    }
}