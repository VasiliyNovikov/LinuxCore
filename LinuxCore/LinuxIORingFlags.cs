using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.IOUring;

namespace LinuxCore;

/// <summary>
/// Specifies Linux <c>io_uring_setup(2)</c> flags.
/// </summary>
/// <remarks>
/// These values mirror the Linux UAPI. <see cref="LinuxIORing"/> currently accepts only
/// <see cref="None"/>, <see cref="Clamp"/>, and <see cref="SubmitAll"/>.
/// </remarks>
[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxIORingFlags : uint
{
    None             = 0,
    IOPoll           = IORING_SETUP_IOPOLL,
    SQPoll           = IORING_SETUP_SQPOLL,
    SQAffinity       = IORING_SETUP_SQ_AFF,
    CQSize           = IORING_SETUP_CQSIZE,
    Clamp            = IORING_SETUP_CLAMP,
    AttachWQ         = IORING_SETUP_ATTACH_WQ,
    Disabled         = IORING_SETUP_R_DISABLED,
    SubmitAll        = IORING_SETUP_SUBMIT_ALL,
    CoopTaskRun      = IORING_SETUP_COOP_TASKRUN,
    TaskRunFlag      = IORING_SETUP_TASKRUN_FLAG,
    SQE128           = IORING_SETUP_SQE128,
    CQE32            = IORING_SETUP_CQE32,
    SingleIssuer     = IORING_SETUP_SINGLE_ISSUER,
    DeferTaskRun     = IORING_SETUP_DEFER_TASKRUN,
    NoMmap           = IORING_SETUP_NO_MMAP,
    RegisteredFdOnly = IORING_SETUP_REGISTERED_FD_ONLY,
    NoSQArray        = IORING_SETUP_NO_SQARRAY,
    HybridIOPoll     = IORING_SETUP_HYBRID_IOPOLL,
    CQEMixed         = IORING_SETUP_CQE_MIXED,
    SQEMixed         = IORING_SETUP_SQE_MIXED,
    SQRewind         = IORING_SETUP_SQ_REWIND
}