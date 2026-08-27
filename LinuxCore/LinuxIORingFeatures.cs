using System;
using System.Diagnostics.CodeAnalysis;

using static LinuxCore.Interop.IOUring;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxIORingFeatures : uint
{
    SingleMemoryMap          = IORING_FEAT_SINGLE_MMAP,
    NoDrop                   = IORING_FEAT_NODROP,
    SubmitStable             = IORING_FEAT_SUBMIT_STABLE,
    ReadWriteCurrentPosition = IORING_FEAT_RW_CUR_POS,
    CurrentPersonality       = IORING_FEAT_CUR_PERSONALITY,
    FastPoll                 = IORING_FEAT_FAST_POLL,
    Poll32Bits               = IORING_FEAT_POLL_32BITS,
    SQPollNonFixed           = IORING_FEAT_SQPOLL_NONFIXED,
    ExtendedArgs             = IORING_FEAT_EXT_ARG,
    NativeWorkers            = IORING_FEAT_NATIVE_WORKERS,
    RSRCTags                 = IORING_FEAT_RSRC_TAGS,
    CQESkip                  = IORING_FEAT_CQE_SKIP,
    LinkedFile               = IORING_FEAT_LINKED_FILE,
    RegisterRegisteredRing   = IORING_FEAT_REG_REG_RING,
    ReceiveSendBundle        = IORING_FEAT_RECVSEND_BUNDLE,
    MinTimeout               = IORING_FEAT_MIN_TIMEOUT,
    ReadWriteAttributes      = IORING_FEAT_RW_ATTR,
    NoIOWait                 = IORING_FEAT_NO_IOWAIT,
}