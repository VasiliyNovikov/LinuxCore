using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe class IOUring
{
    private const int _NSIG = 64;

    public const ulong IORING_OFF_SQ_RING    = 0UL;
    public const ulong IORING_OFF_CQ_RING    = 0x8000000UL;
    public const ulong IORING_OFF_SQES       = 0x10000000UL;
    public const ulong IORING_OFF_PBUF_RING  = 0x80000000UL;
    public const ulong IORING_OFF_PBUF_SHIFT = 16;
    public const ulong IORING_OFF_MMAP_MASK  = 0xf8000000UL;

    public const uint IORING_SETUP_IOPOLL             = 1U <<  0;
    public const uint IORING_SETUP_SQPOLL             = 1U <<  1;
    public const uint IORING_SETUP_SQ_AFF             = 1U <<  2;
    public const uint IORING_SETUP_CQSIZE             = 1U <<  3;
    public const uint IORING_SETUP_CLAMP              = 1U <<  4;
    public const uint IORING_SETUP_ATTACH_WQ          = 1U <<  5;
    public const uint IORING_SETUP_R_DISABLED         = 1U <<  6;
    public const uint IORING_SETUP_SUBMIT_ALL         = 1U <<  7;
    public const uint IORING_SETUP_COOP_TASKRUN       = 1U <<  8;
    public const uint IORING_SETUP_TASKRUN_FLAG       = 1U <<  9;
    public const uint IORING_SETUP_SQE128             = 1U << 10;
    public const uint IORING_SETUP_CQE32              = 1U << 11;
    public const uint IORING_SETUP_SINGLE_ISSUER      = 1U << 12;
    public const uint IORING_SETUP_DEFER_TASKRUN      = 1U << 13;
    public const uint IORING_SETUP_NO_MMAP            = 1U << 14;
    public const uint IORING_SETUP_REGISTERED_FD_ONLY = 1U << 15;
    public const uint IORING_SETUP_NO_SQARRAY         = 1U << 16;
    public const uint IORING_SETUP_HYBRID_IOPOLL      = 1U << 17;
    public const uint IORING_SETUP_CQE_MIXED          = 1U << 18;
    public const uint IORING_SETUP_SQE_MIXED          = 1U << 19;
    public const uint IORING_SETUP_SQ_REWIND          = 1U << 20;

    public const uint IORING_FEAT_SINGLE_MMAP     = 1U <<  0;
    public const uint IORING_FEAT_NODROP          = 1U <<  1;
    public const uint IORING_FEAT_SUBMIT_STABLE   = 1U <<  2;
    public const uint IORING_FEAT_RW_CUR_POS      = 1U <<  3;
    public const uint IORING_FEAT_CUR_PERSONALITY = 1U <<  4;
    public const uint IORING_FEAT_FAST_POLL       = 1U <<  5;
    public const uint IORING_FEAT_POLL_32BITS     = 1U <<  6;
    public const uint IORING_FEAT_SQPOLL_NONFIXED = 1U <<  7;
    public const uint IORING_FEAT_EXT_ARG         = 1U <<  8;
    public const uint IORING_FEAT_NATIVE_WORKERS  = 1U <<  9;
    public const uint IORING_FEAT_RSRC_TAGS       = 1U << 10;
    public const uint IORING_FEAT_CQE_SKIP        = 1U << 11;
    public const uint IORING_FEAT_LINKED_FILE     = 1U << 12;
    public const uint IORING_FEAT_REG_REG_RING    = 1U << 13;
    public const uint IORING_FEAT_RECVSEND_BUNDLE = 1U << 14;
    public const uint IORING_FEAT_MIN_TIMEOUT     = 1U << 15;
    public const uint IORING_FEAT_RW_ATTR         = 1U << 16;
    public const uint IORING_FEAT_NO_IOWAIT       = 1U << 17;

    public const uint IORING_ENTER_GETEVENTS       = 1U << 0;
    public const uint IORING_ENTER_SQ_WAKEUP       = 1U << 1;
    public const uint IORING_ENTER_SQ_WAIT         = 1U << 2;
    public const uint IORING_ENTER_EXT_ARG         = 1U << 3;
    public const uint IORING_ENTER_REGISTERED_RING = 1U << 4;
    public const uint IORING_ENTER_ABS_TIMER       = 1U << 5;
    public const uint IORING_ENTER_EXT_ARG_REG     = 1U << 6;
    public const uint IORING_ENTER_NO_IOWAIT       = 1U << 7;

    // struct io_sqring_offsets {
    //     __u32 head;
    //     __u32 tail;
    //     __u32 ring_mask;
    //     __u32 ring_entries;
    //     __u32 flags;
    //     __u32 dropped;
    //     __u32 array;
    //     __u32 resv1;
    //     __u64 user_addr;
    // };
    [StructLayout(LayoutKind.Sequential)]
    public struct io_sqring_offsets
    {
        public uint head;
        public uint tail;
        public uint ring_mask;
        public uint ring_entries;
        public uint flags;
        public uint dropped;
        public uint array;
        public uint resv1;
        public ulong user_addr;
    }

    // struct io_cqring_offsets {
    //     __u32 head;
    //     __u32 tail;
    //     __u32 ring_mask;
    //     __u32 ring_entries;
    //     __u32 overflow;
    //     __u32 cqes;
    //     __u32 flags;
    //     __u32 resv1;
    //     __u64 user_addr;
    // };
    [StructLayout(LayoutKind.Sequential)]
    public struct io_cqring_offsets
    {
        public uint head;
        public uint tail;
        public uint ring_mask;
        public uint ring_entries;
        public uint overflow;
        public uint cqes;
        public uint flags;
        public uint resv1;
        public ulong user_addr;
    }

    // struct io_uring_params {
    //     __u32 sq_entries;
    //     __u32 cq_entries;
    //     __u32 flags;
    //     __u32 sq_thread_cpu;
    //     __u32 sq_thread_idle;
    //     __u32 features;
    //     __u32 wq_fd;
    //     __u32 resv[3];
    //     struct io_sqring_offsets sq_off;
    //     struct io_cqring_offsets cq_off;
    // };
    [StructLayout(LayoutKind.Sequential)]
    public struct io_uring_params
    {
        public uint sq_entries;
        public uint cq_entries;
        public uint flags;
        public uint sq_thread_cpu;
        public uint sq_thread_idle;
        public uint features;
        public uint wq_fd;
        public InlineArray3<uint> resv;
        public io_sqring_offsets sq_off;
        public io_cqring_offsets cq_off;
    }
    
    public enum io_uring_op : byte
    {
        IORING_OP_NOP,
        IORING_OP_READV,
        IORING_OP_WRITEV,
        IORING_OP_FSYNC,
        IORING_OP_READ_FIXED,
        IORING_OP_WRITE_FIXED,
        IORING_OP_POLL_ADD,
        IORING_OP_POLL_REMOVE,
        IORING_OP_SYNC_FILE_RANGE,
        IORING_OP_SENDMSG,
        IORING_OP_RECVMSG,
        IORING_OP_TIMEOUT,
        IORING_OP_TIMEOUT_REMOVE,
        IORING_OP_ACCEPT,
        IORING_OP_ASYNC_CANCEL,
        IORING_OP_LINK_TIMEOUT,
        IORING_OP_CONNECT,
        IORING_OP_FALLOCATE,
        IORING_OP_OPENAT,
        IORING_OP_CLOSE,
        IORING_OP_FILES_UPDATE,
        IORING_OP_STATX,
        IORING_OP_READ,
        IORING_OP_WRITE,
        IORING_OP_FADVISE,
        IORING_OP_MADVISE,
        IORING_OP_SEND,
        IORING_OP_RECV,
        IORING_OP_OPENAT2,
        IORING_OP_EPOLL_CTL,
        IORING_OP_SPLICE,
        IORING_OP_PROVIDE_BUFFERS,
        IORING_OP_REMOVE_BUFFERS,
        IORING_OP_TEE,
        IORING_OP_SHUTDOWN,
        IORING_OP_RENAMEAT,
        IORING_OP_UNLINKAT,
        IORING_OP_MKDIRAT,
        IORING_OP_SYMLINKAT,
        IORING_OP_LINKAT,
        IORING_OP_MSG_RING,
        IORING_OP_FSETXATTR,
        IORING_OP_SETXATTR,
        IORING_OP_FGETXATTR,
        IORING_OP_GETXATTR,
        IORING_OP_SOCKET,
        IORING_OP_URING_CMD,
        IORING_OP_SEND_ZC,
        IORING_OP_SENDMSG_ZC,
        IORING_OP_READ_MULTISHOT,
        IORING_OP_WAITID,
        IORING_OP_FUTEX_WAIT,
        IORING_OP_FUTEX_WAKE,
        IORING_OP_FUTEX_WAITV,
        IORING_OP_FIXED_FD_INSTALL,
        IORING_OP_FTRUNCATE,
        IORING_OP_BIND,
        IORING_OP_LISTEN,
        IORING_OP_RECV_ZC,
        IORING_OP_EPOLL_WAIT,
        IORING_OP_READV_FIXED,
        IORING_OP_WRITEV_FIXED,
        IORING_OP_PIPE,
        IORING_OP_NOP128,
        IORING_OP_URING_CMD128,

        /* this goes last, obviously */
        IORING_OP_LAST,
    }

    // struct io_uring_sqe {
    // 	__u8	opcode;     /* type of operation for this sqe */
    // 	__u8	flags;      /* IOSQE_ flags */
    // 	__u16	ioprio;     /* ioprio for the request */
    // 	__s32	fd;         /* file descriptor to do IO on */
    // 	union {
    // 		__u64	off;    /* offset into file */
    // 		__u64	addr2;
    // 		struct {
    // 			__u32	cmd_op;
    // 			__u32	__pad1;
    // 		};
    // 	};
    // 	union {
    // 		__u64	addr;   /* pointer to buffer or iovecs */
    // 		__u64	splice_off_in;
    // 		struct {
    // 			__u32	level;
    // 			__u32	optname;
    // 		};
    // 	};
    // 	__u32	len;        /* buffer size or number of iovecs */
    // 	union {
    // 		__u32		rw_flags;
    // 		__u32		fsync_flags;
    // 		__u16		poll_events;    /* compatibility */
    // 		__u32		poll32_events;  /* word-reversed for BE */
    // 		__u32		sync_range_flags;
    // 		__u32		msg_flags;
    // 		__u32		timeout_flags;
    // 		__u32		accept_flags;
    // 		__u32		cancel_flags;
    // 		__u32		open_flags;
    // 		__u32		statx_flags;
    // 		__u32		fadvise_advice;
    // 		__u32		splice_flags;
    // 		__u32		rename_flags;
    // 		__u32		unlink_flags;
    // 		__u32		hardlink_flags;
    // 		__u32		xattr_flags;
    // 		__u32		msg_ring_flags;
    // 		__u32		uring_cmd_flags;
    // 		__u32		waitid_flags;
    // 		__u32		futex_flags;
    // 		__u32		install_fd_flags;
    // 		__u32		nop_flags;
    // 		__u32		pipe_flags;
    // 	};
    // 	__u64	user_data;  /* data to be passed back at completion time */
    // 	/* pack this to avoid bogus arm OABI complaints */
    // 	union {
    // 		/* index into fixed buffers, if used */
    // 		__u16	buf_index;
    // 		/* for grouped buffer selection */
    // 		__u16	buf_group;
    // 	} __attribute__((packed));
    // 	/* personality to use, if used */
    // 	__u16	personality;
    // 	union {
    // 		__s32	splice_fd_in;
    // 		__u32	file_index;
    // 		__u32	zcrx_ifq_idx;
    // 		__u32	optlen;
    // 		struct {
    // 			__u16	addr_len;
    // 			__u16	__pad3[1];
    // 		};
    // 		struct {
    // 			__u8	write_stream;
    // 			__u8	__pad4[3];
    // 		};
    // 	};
    // 	union {
    // 		struct {
    // 			__u64	addr3;
    // 			__u64	__pad2[1];
    // 		};
    // 		struct {
    // 			__u64	attr_ptr; /* pointer to attribute information */
    // 			__u64	attr_type_mask; /* bit mask of attributes */
    // 		};
    // 		__u64	optval;
    // 		/*
    // 		 * If the ring is initialized with IORING_SETUP_SQE128, then
    // 		 * this field is used for 80 bytes of arbitrary command data
    // 		 */
    // 		__u8	cmd[0];
    // 	};
    // };
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct io_uring_sqe
    {
        [FieldOffset(0)]
        public io_uring_op opcode;

        [FieldOffset(1)]
        public byte flags;

        [FieldOffset(2)]
        public ushort ioprio;

        [FieldOffset(4)]
        public FileDescriptor fd;

        /*
         * union {
         *     __u64 off;
         *     __u64 addr2;
         *     struct {
         *         __u32 cmd_op;
         *         __u32 __pad1;
         *     };
         * };
         */
        [FieldOffset(8)]
        public ulong off;

        [FieldOffset(8)]
        public ulong addr2;

        [FieldOffset(8)]
        public uint cmd_op;

        /*
         * union {
         *     __u64 addr;
         *     __u64 splice_off_in;
         *     struct {
         *         __u32 level;
         *         __u32 optname;
         *     };
         * };
         */
        [FieldOffset(16)]
        public ulong addr;

        [FieldOffset(16)]
        public ulong splice_off_in;

        [FieldOffset(16)]
        public uint level;

        [FieldOffset(20)]
        public uint optname;

        [FieldOffset(24)]
        public uint len;

        /*
         * union {
         *     __u32 rw_flags;
         *     __u32 fsync_flags;
         *     __u16 poll_events;
         *     __u32 poll32_events;
         *     ...
         * };
         */
        [FieldOffset(28)]
        public uint rw_flags;

        [FieldOffset(28)]
        public uint fsync_flags;

        [FieldOffset(28)]
        public ushort poll_events;

        [FieldOffset(28)]
        public uint poll32_events;

        [FieldOffset(28)]
        public uint sync_range_flags;

        [FieldOffset(28)]
        public uint msg_flags;

        [FieldOffset(28)]
        public uint timeout_flags;

        [FieldOffset(28)]
        public uint accept_flags;

        [FieldOffset(28)]
        public uint cancel_flags;

        [FieldOffset(28)]
        public uint open_flags;

        [FieldOffset(28)]
        public uint statx_flags;

        [FieldOffset(28)]
        public uint fadvise_advice;

        [FieldOffset(28)]
        public uint splice_flags;

        [FieldOffset(28)]
        public uint rename_flags;

        [FieldOffset(28)]
        public uint unlink_flags;

        [FieldOffset(28)]
        public uint hardlink_flags;

        [FieldOffset(28)]
        public uint xattr_flags;

        [FieldOffset(28)]
        public uint msg_ring_flags;

        [FieldOffset(28)]
        public uint uring_cmd_flags;

        [FieldOffset(28)]
        public uint waitid_flags;

        [FieldOffset(28)]
        public uint futex_flags;

        [FieldOffset(28)]
        public uint install_fd_flags;

        [FieldOffset(28)]
        public uint nop_flags;

        [FieldOffset(28)]
        public uint pipe_flags;

        [FieldOffset(32)]
        public ulong user_data;

        /*
         * union {
         *     __u16 buf_index;
         *     __u16 buf_group;
         * } __attribute__((packed));
         */
        [FieldOffset(40)]
        public ushort buf_index;

        [FieldOffset(40)]
        public ushort buf_group;

        [FieldOffset(42)]
        public ushort personality;

        /*
         * union {
         *     __s32 splice_fd_in;
         *     __u32 file_index;
         *     __u32 zcrx_ifq_idx;
         *     __u32 optlen;
         *     struct {
         *         __u16 addr_len;
         *         __u16 __pad3[1];
         *     };
         *     struct {
         *         __u8 write_stream;
         *         __u8 __pad4[3];
         *     };
         * };
         */
        [FieldOffset(44)]
        public int splice_fd_in;

        [FieldOffset(44)]
        public uint file_index;

        [FieldOffset(44)]
        public uint zcrx_ifq_idx;

        [FieldOffset(44)]
        public uint optlen;

        [FieldOffset(44)]
        public ushort addr_len;

        [FieldOffset(44)]
        public byte write_stream;

        /*
         * union {
         *     struct {
         *         __u64 addr3;
         *         __u64 __pad2[1];
         *     };
         *     struct {
         *         __u64 attr_ptr;
         *         __u64 attr_type_mask;
         *     };
         *     __u64 optval;
         *     __u8 cmd[0];
         * };
         */
        [FieldOffset(48)]
        public ulong addr3;

        [FieldOffset(48)]
        public ulong attr_ptr;

        [FieldOffset(56)]
        public ulong attr_type_mask;

        [FieldOffset(48)]
        public ulong optval;

        [FieldOffset(48)]
        private byte _cmd;

        public byte* cmd
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte*)Unsafe.AsPointer(ref _cmd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static io_uring_sqe* at(void* sqes, uint index, uint sqe_size) => (io_uring_sqe*)((byte*)sqes + index * sqe_size);
    }

    // struct io_uring_cqe {
    // 	   __u64	user_data;	/* sqe->user_data value passed back */
    // 	   __s32	res;		/* result code for this event */
    // 	   __u32	flags;
    // 
     //    /*
    // 	    * If the ring is initialized with IORING_SETUP_CQE32, then this field
    // 	    * contains 16-bytes of padding, doubling the size of the CQE.
    // 	    */
    // 	   __u64 big_cqe[];
    // };
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct io_uring_cqe
    {
        public readonly ulong user_data;
        public readonly int res;
        public readonly uint flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static io_uring_cqe* at(void* cqes, uint index, uint cqe_size) => (io_uring_cqe*)((byte*)cqes + index * cqe_size);
    }

    // static inline int io_uring_setup(unsigned int entries, struct io_uring_params *p) {
    //     return syscall(__NR_io_uring_setup, entries, p);
    // }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> io_uring_setup(uint entries, ref io_uring_params p)
    {
        fixed (io_uring_params* ptr = &p)
            return SystemCall.Invoke<uint, nint, FileDescriptor>(SystemCallTable.IOUringSetup, entries, (nint)ptr);
    }

    // static inline int io_uring_enter(int fd, unsigned int to_submit, unsigned int min_complete, unsigned int flags, sigset_t *sig) {
    //     return syscall(__NR_io_uring_enter, fd, to_submit, min_complete, flags, sig, _NSIG / 8);
    // }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult io_uring_enter(FileDescriptor fd, uint to_submit, uint min_complete, uint flags, void* sig = null)
    {
        return SystemCall.Invoke(SystemCallTable.IOUringEnter, fd, to_submit, min_complete, flags, (nint)sig, _NSIG / 8);
    }
}