using System;
using System.Linq;
using System.Reflection;

using LinuxCore.Interop;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static LinuxCore.Interop.File;

namespace LinuxCore.Tests;

[TestClass]
public unsafe class LinuxIORingTests
{
    private const string Header = "linux/io_uring.h";

    [TestMethod]
    public void IOUring_Offsets_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.ValuesMatch(
        [
            (nameof(IOUring.IORING_OFF_SQ_RING), (long)IOUring.IORING_OFF_SQ_RING),
            (nameof(IOUring.IORING_OFF_CQ_RING), (long)IOUring.IORING_OFF_CQ_RING),
            (nameof(IOUring.IORING_OFF_SQES), (long)IOUring.IORING_OFF_SQES)
        ], Header);
        NativeConstantAssert.OptionalValuesMatch(
        [
            (nameof(IOUring.IORING_OFF_PBUF_RING), (long)IOUring.IORING_OFF_PBUF_RING),
            (nameof(IOUring.IORING_OFF_PBUF_SHIFT), (long)IOUring.IORING_OFF_PBUF_SHIFT),
            (nameof(IOUring.IORING_OFF_MMAP_MASK), (long)IOUring.IORING_OFF_MMAP_MASK)
        ], Header);
    }

    [TestMethod]
    public void IOUring_Layouts_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.SizeMatches<IOUring.io_sqring_offsets>(Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.head), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.tail), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.ring_mask), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.ring_entries), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.flags), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.dropped), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.array), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.resv1), Header);
        if (CScript.IsDefined("IORING_SETUP_NO_MMAP", Header))
            NativeConstantAssert.OffsetMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.user_addr), Header);
        else
            NativeConstantAssert.OffsetExpressionMatches<IOUring.io_sqring_offsets>(nameof(IOUring.io_sqring_offsets.user_addr), "offsetof(struct io_sqring_offsets, resv2)", Header);
        NativeConstantAssert.SizeMatches<IOUring.io_cqring_offsets>(Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.head), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.tail), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.ring_mask), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.ring_entries), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.overflow), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.cqes), Header);
        var hasCompletionFlags = CScript.IsDefined("IORING_CQ_EVENTFD_DISABLED", Header);
        if (hasCompletionFlags)
        {
            NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.flags), Header);
            NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.resv1), Header);
        }
        else
        {
            NativeConstantAssert.OffsetExpressionMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.flags), "offsetof(struct io_cqring_offsets, resv)", Header);
            NativeConstantAssert.OffsetExpressionMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.resv1), "offsetof(struct io_cqring_offsets, resv) + sizeof(__u32)", Header);
        }

        if (CScript.IsDefined("IORING_SETUP_NO_MMAP", Header))
            NativeConstantAssert.OffsetMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.user_addr), Header);
        else
            NativeConstantAssert.OffsetExpressionMatches<IOUring.io_cqring_offsets>(nameof(IOUring.io_cqring_offsets.user_addr),
                                                                                    hasCompletionFlags
                                                                                        ? "offsetof(struct io_cqring_offsets, resv2)"
                                                                                        : "offsetof(struct io_cqring_offsets, resv) + sizeof(__u64)",
                                                                                    Header);
        NativeConstantAssert.SizeMatches<IOUring.io_uring_params>(Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.sq_entries), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.cq_entries), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.flags), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.sq_thread_cpu), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.sq_thread_idle), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.features), Header);
        if (CScript.IsDefined("IORING_SETUP_ATTACH_WQ", Header))
        {
            NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.wq_fd), Header);
            NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.resv), Header);
        }
        else
        {
            NativeConstantAssert.OffsetExpressionMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.wq_fd), "offsetof(struct io_uring_params, resv)", Header);
            NativeConstantAssert.OffsetExpressionMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.resv), "offsetof(struct io_uring_params, resv) + sizeof(__u32)", Header);
        }
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.sq_off), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_params>(nameof(IOUring.io_uring_params.cq_off), Header);

        NativeConstantAssert.SizeMatches<IOUring.io_uring_sqe>(Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.opcode), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.flags), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.ioprio), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.fd), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.off), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.addr), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.len), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.rw_flags), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.user_data), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_sqe>(nameof(IOUring.io_uring_sqe.buf_index), Header);

        NativeConstantAssert.SizeMatches<IOUring.io_uring_cqe>(Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_cqe>(nameof(IOUring.io_uring_cqe.user_data), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_cqe>(nameof(IOUring.io_uring_cqe.res), Header);
        NativeConstantAssert.OffsetMatches<IOUring.io_uring_cqe>(nameof(IOUring.io_uring_cqe.flags), Header);
    }

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
        ], Header);
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
        ], Header);
    }

    [TestMethod]
    public void IOUring_EnterFlags_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.ValuesMatch(
        [
            (nameof(IOUring.IORING_ENTER_GETEVENTS), IOUring.IORING_ENTER_GETEVENTS),
            (nameof(IOUring.IORING_ENTER_SQ_WAKEUP), IOUring.IORING_ENTER_SQ_WAKEUP)
        ], Header);
        NativeConstantAssert.OptionalValuesMatch(
        [
            (nameof(IOUring.IORING_ENTER_SQ_WAIT), IOUring.IORING_ENTER_SQ_WAIT),
            (nameof(IOUring.IORING_ENTER_EXT_ARG), IOUring.IORING_ENTER_EXT_ARG),
            (nameof(IOUring.IORING_ENTER_REGISTERED_RING), IOUring.IORING_ENTER_REGISTERED_RING),
            (nameof(IOUring.IORING_ENTER_ABS_TIMER), IOUring.IORING_ENTER_ABS_TIMER),
            (nameof(IOUring.IORING_ENTER_EXT_ARG_REG), IOUring.IORING_ENTER_EXT_ARG_REG),
            (nameof(IOUring.IORING_ENTER_NO_IOWAIT), IOUring.IORING_ENTER_NO_IOWAIT)
        ], Header);
    }

    [TestMethod]
    public void IOUring_Operations_Match_Current_Platform_Headers()
    {
        var names = Enum.GetNames<IOUring.io_uring_op>();
        int[] nativeValues;
        if (CScript.TryEvaluateInt32("IORING_OP_LAST", out var nativeLast, Header))
        {
            Assert.IsTrue(nativeLast <= names.Length, $"Native headers define {nativeLast} operations but only {names.Length} are mapped.");
            nativeValues = CScript.EvaluateInt32s(names[..nativeLast], Header);
        }
        else
        {
            var definedValues = CScript.EvaluateDefinedInt32s(names, Header);
            var nativeOperationCount = definedValues.IndexOf((int?)null);
            if (nativeOperationCount < 0)
                nativeOperationCount = definedValues.Length;
            Assert.DoesNotContain(value => value is not null, definedValues[nativeOperationCount..], "Native opcode macros must form a contiguous prefix.");
            nativeValues = [.. definedValues[..nativeOperationCount].Select(value => value!.Value)];
        }

        for (var i = 0; i < nativeValues.Length; ++i)
            Assert.AreEqual((int)Enum.Parse<IOUring.io_uring_op>(names[i]), nativeValues[i], names[i]);
    }

    [TestMethod]
    public void LinuxIORing_IsSupported_MatchesCIAssumptions() => Assert.AreNotEqual(NativeAbi.IsLikelyQemuLinuxUser, LinuxIORing.IsSupported);

    [TestMethod]
    public void LinuxIORing_IsSupported_FalseMeansKernelDoesNotSupportSetup()
    {
        if (LinuxIORing.IsSupported)
            return;

        Assert.ThrowsExactly<PlatformNotSupportedException>(() => new LinuxIORing(1));
    }

    [TestMethod]
    public void LinuxIORing_IsSupported_IsKernelCapabilityField()
    {
        var field = typeof(LinuxIORing).GetField(nameof(LinuxIORing.IsSupported), BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(field);
        Assert.IsTrue(field.IsInitOnly);
        Assert.AreEqual(typeof(bool), field.FieldType);
    }

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
        if (NativeAbi.IsLikelyQemuLinuxUser)
            return;

        fcntl(FileDescriptor.StandardInput, F_GETFD).ThrowIfError();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LinuxIORing(-1));

        GC.Collect();
        GC.WaitForPendingFinalizers();

        fcntl(FileDescriptor.StandardInput, F_GETFD).ThrowIfError();
    }
}