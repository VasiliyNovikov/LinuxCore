using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.IOUring;

namespace LinuxCore;

/// <summary>
/// Owns an <c>io_uring</c> instance and its kernel-shared queue mappings.
/// </summary>
/// <remarks>
/// The constructor currently supports only <see cref="LinuxIORingFlags.None"/>,
/// <see cref="LinuxIORingFlags.Clamp"/>, and <see cref="LinuxIORingFlags.SubmitAll"/>.
/// The descriptor returned by <see cref="Descriptor"/> is non-owning. Keep this instance strongly
/// reachable and prevent concurrent disposal while the descriptor or queue mappings are in use.
/// </remarks>
public sealed unsafe class LinuxIORing : NativeObject, IFileObject
{
    private const LinuxIORingFlags SupportedFlags = LinuxIORingFlags.Clamp | LinuxIORingFlags.SubmitAll;
    private const LinuxIORingFlags KnownFlags = LinuxIORingFlags.IOPoll
                                              | LinuxIORingFlags.SQPoll
                                              | LinuxIORingFlags.SQAffinity
                                              | LinuxIORingFlags.CQSize
                                              | LinuxIORingFlags.Clamp
                                              | LinuxIORingFlags.AttachWQ
                                              | LinuxIORingFlags.Disabled
                                              | LinuxIORingFlags.SubmitAll
                                              | LinuxIORingFlags.CoopTaskRun
                                              | LinuxIORingFlags.TaskRunFlag
                                              | LinuxIORingFlags.SQE128
                                              | LinuxIORingFlags.CQE32
                                              | LinuxIORingFlags.SingleIssuer
                                              | LinuxIORingFlags.DeferTaskRun
                                              | LinuxIORingFlags.NoMmap
                                              | LinuxIORingFlags.RegisteredFdOnly
                                              | LinuxIORingFlags.NoSQArray
                                              | LinuxIORingFlags.HybridIOPoll
                                              | LinuxIORingFlags.CQEMixed
                                              | LinuxIORingFlags.SQEMixed
                                              | LinuxIORingFlags.SQRewind;

    /// <summary>
    /// Indicates whether the running kernel implements <c>io_uring_setup(2)</c>.
    /// </summary>
    /// <remarks>
    /// A value of <see langword="true"/> does not guarantee that ring creation is permitted by the
    /// current environment. Policy and permission errors are reported by the constructor.
    /// </remarks>
    public static readonly bool IsSupported = GetIsSupported();

    private readonly LinuxMemoryMap? _submissionQueueMap;
    private readonly LinuxMemoryMap? _submissionQueueEntryMap;
    private readonly LinuxMemoryMap? _completionQueueMap;

    private readonly uint* _submissionQueueHead;
    private readonly uint* _submissionQueueTail;
    private readonly uint* _submissionQueueRingMask;
    private readonly uint* _submissionQueueRingEntries;
    private readonly uint* _submissionQueueFlags;
    private readonly uint* _submissionQueueArray;
    private readonly io_uring_sqe* _submissionQueueEntries;

    private readonly uint* _completionQueueHead;
    private readonly uint* _completionQueueTail;
    private readonly uint* _completionQueueRingMask;
    private readonly uint* _completionQueueRingEntries;
    private readonly io_uring_cqe* _completionQueueEntries;

    public FileDescriptor Descriptor { get; } = new(-1);
    public LinuxIORingFlags Flags { get; }
    public LinuxIORingFeatures Features { get; }
    public int SubmissionQueueSize { get; }
    public int CompletionQueueSize { get; }

    public LinuxIORing(int size, LinuxIORingFlags flags = LinuxIORingFlags.None)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("io_uring is not supported on this platform");

        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        if ((flags & ~KnownFlags) != 0)
            throw new ArgumentOutOfRangeException(nameof(flags), flags, "Unknown io_uring setup flags were specified");
        var unsupportedFlags = flags & ~SupportedFlags;
        if (unsupportedFlags != 0)
            throw new NotSupportedException($"The requested io_uring setup flags are not supported yet: {unsupportedFlags}");
        try
        {
            var @params = new io_uring_params { flags = (uint)flags };
            Descriptor = io_uring_setup((uint)size, ref @params).ThrowIfError();
            Flags = (LinuxIORingFlags)@params.flags;
            Features = (LinuxIORingFeatures)@params.features;
            SubmissionQueueSize = checked((int)@params.sq_entries);
            CompletionQueueSize = checked((int)@params.cq_entries);

            var singleMemoryMap = Features.HasFlag(LinuxIORingFeatures.SingleMemoryMap);
            var submissionRingSize = checked((int)((ulong)@params.sq_off.array + (ulong)@params.sq_entries * sizeof(uint)));
            var completionRingSize = checked((int)((ulong)@params.cq_off.cqes + (ulong)@params.cq_entries * (ulong)sizeof(io_uring_cqe)));
            if (singleMemoryMap)
                submissionRingSize = completionRingSize = Math.Max(submissionRingSize, completionRingSize);
            var submissionQueueEntrySize = checked((int)((ulong)@params.sq_entries * (ulong)sizeof(io_uring_sqe)));

            _submissionQueueMap = new LinuxMemoryMap(Descriptor, submissionRingSize, LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Populate, (long)IORING_OFF_SQ_RING);
            _completionQueueMap = singleMemoryMap
                ? _submissionQueueMap
                : new LinuxMemoryMap(Descriptor, completionRingSize, LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Populate, (long)IORING_OFF_CQ_RING);
            _submissionQueueEntryMap = new LinuxMemoryMap(Descriptor, submissionQueueEntrySize, LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Populate, (long)IORING_OFF_SQES);

            var submissionQueuePtr = (byte*)Unsafe.AsPointer(ref _submissionQueueMap.Span[0]);
            _submissionQueueHead = (uint*)(submissionQueuePtr + @params.sq_off.head);
            _submissionQueueTail = (uint*)(submissionQueuePtr + @params.sq_off.tail);
            _submissionQueueRingMask = (uint*)(submissionQueuePtr + @params.sq_off.ring_mask);
            _submissionQueueRingEntries = (uint*)(submissionQueuePtr + @params.sq_off.ring_entries);
            _submissionQueueFlags = (uint*)(submissionQueuePtr + @params.sq_off.flags);
            _submissionQueueArray = (uint*)(submissionQueuePtr + @params.sq_off.array);
            _submissionQueueEntries = (io_uring_sqe*)(Unsafe.AsPointer(ref _submissionQueueEntryMap.Span[0]));

            var completionQueuePtr = (byte*)Unsafe.AsPointer(ref _completionQueueMap.Span[0]);
            _completionQueueHead = (uint*)(completionQueuePtr + @params.cq_off.head);
            _completionQueueTail = (uint*)(completionQueuePtr + @params.cq_off.tail);
            _completionQueueRingMask = (uint*)(completionQueuePtr + @params.cq_off.ring_mask);
            _completionQueueRingEntries = (uint*)(completionQueuePtr + @params.cq_off.ring_entries);
            _completionQueueEntries = (io_uring_cqe*)(completionQueuePtr + @params.cq_off.cqes);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    protected override void ReleaseUnmanagedResources()
    {
        _submissionQueueEntryMap?.Dispose();
        if (!ReferenceEquals(_completionQueueMap, _submissionQueueMap))
            _completionQueueMap?.Dispose();
        _submissionQueueMap?.Dispose();
        Descriptor.Close();
    }

    private static bool GetIsSupported()
    {
        var @params = new io_uring_params();
        var result = io_uring_setup(uint.MaxValue, ref @params);
        if (result.IsError)
            return LinuxErrorNumber.Last != LinuxErrorNumber.InvalidSystemCall;

        result.ThrowIfError().Close();
        return true;
    }
}