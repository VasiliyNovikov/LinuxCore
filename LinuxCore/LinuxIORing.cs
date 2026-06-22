using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.IOUring;

namespace LinuxCore;

public unsafe class LinuxIORing : NativeObject, IFileObject
{
    private readonly LinuxMemoryMap _submissionQueueMap;
    private readonly LinuxMemoryMap _submissionQueueEntryMap;
    private readonly LinuxMemoryMap _completionQueueMap;

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
    private readonly uint* _completionQueueFlags;
    private readonly io_uring_cqe* _completionQueueEntries;

    public FileDescriptor Descriptor { get; }
    public LinuxIORingFlags Flags { get; }
    public LinuxIORingFeatures Features { get; }
    public int SubmissionQueueSize { get; }
    public int CompletionQueueSize { get; }

    public LinuxIORing(int size, LinuxIORingFlags flags = LinuxIORingFlags.None)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        var @params =  new io_uring_params { flags = (uint)flags };
        Descriptor = io_uring_setup((uint)size, ref @params).ThrowIfError();
        Flags = (LinuxIORingFlags)@params.flags;
        Features = (LinuxIORingFeatures)@params.features;
        SubmissionQueueSize = (int)@params.sq_entries;
        CompletionQueueSize = (int)@params.cq_entries;

        var submissionRingSize = (int)(@params.sq_off.array + SubmissionQueueSize * sizeof(uint));
        var completionRingSize = (int)(@params.cq_off.cqes + CompletionQueueSize * sizeof(io_uring_cqe));

        _submissionQueueMap = new LinuxMemoryMap(Descriptor, submissionRingSize, LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Populate, (long)IORING_OFF_SQ_RING);
        _completionQueueMap = Features.HasFlag(LinuxIORingFeatures.SingleMemoryMap)
            ? _submissionQueueMap
            : new LinuxMemoryMap(Descriptor, completionRingSize, LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Populate, (long)IORING_OFF_CQ_RING);
        _submissionQueueEntryMap = new LinuxMemoryMap(Descriptor, SubmissionQueueSize * sizeof(io_uring_sqe), LinuxMemoryMapFlags.Shared | LinuxMemoryMapFlags.Populate, (long)IORING_OFF_SQES);

        var submissionQueuePtr = (byte*)Unsafe.AsPointer(ref _submissionQueueMap.Span[0]);
        _submissionQueueHead = (uint*)(submissionQueuePtr + @params.sq_off.head);
        _submissionQueueTail = (uint*)(submissionQueuePtr + @params.sq_off.tail);
        _submissionQueueRingMask = (uint*)(submissionQueuePtr + @params.sq_off.ring_mask);
        _submissionQueueRingEntries = (uint*)(submissionQueuePtr + @params.sq_off.ring_entries);
        _submissionQueueFlags = (uint*)(submissionQueuePtr + @params.sq_off.flags);
        _submissionQueueArray = (uint*)(submissionQueuePtr + @params.sq_off.array);
        _submissionQueueEntries = (io_uring_sqe*)(Unsafe.AsPointer(ref _submissionQueueEntryMap.Span[0]));
        
        _completionQueueHead = (uint*)(submissionQueuePtr + @params.cq_off.head);
        _completionQueueTail = (uint*)(submissionQueuePtr + @params.cq_off.tail);
        _completionQueueRingMask = (uint*)(submissionQueuePtr + @params.cq_off.ring_mask);
        _completionQueueRingEntries = (uint*)(submissionQueuePtr + @params.cq_off.ring_entries);
        _completionQueueFlags = (uint*)(submissionQueuePtr + @params.cq_off.flags);
        _completionQueueEntries = (io_uring_cqe*)(submissionQueuePtr + @params.cq_off.cqes);
    }

    protected override void ReleaseUnmanagedResources()
    {
        _submissionQueueMap.Dispose();
        _completionQueueMap.Dispose();
        _submissionQueueEntryMap.Dispose();
        Descriptor.Close();
    }
}