using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

[SuppressMessage("Style", "IDE0055:Fix formatting")]
internal static unsafe partial class LibC
{
    private const string Lib = LinuxLibraries.LibC;

    public const int EFD_SEMAPHORE = 0x00001; // Semaphore semantics for eventfd
    public const int EFD_NONBLOCK  = 0x00800; // Set non-blocking mode
    public const int EFD_CLOEXEC   = 0x80000; // Set close-on-exec flag

    public const short POLLIN   = 0b000001; // There is data to read
    public const short POLLPRI  = 0b000010; // There is urgent data to read
    public const short POLLOUT  = 0b000100; // Writing now not block
    public const short POLLERR  = 0b001000; // Error condition
    public const short POLLHUP  = 0b010000; // Hung up
    public const short POLLNVAL = 0b100000; // Invalid request: fd not open
    
    public const int MSG_OOB       = 0x0001; // Process out-of-band data
    public const int MSG_PEEK      = 0x0002; // Peek at incoming message
    public const int MSG_DONTROUTE = 0x0004; // Don't route
    public const int MSG_CTRUNC    = 0x0008; // Control data lost
    public const int MSG_PROXY     = 0x0010; // Supply originate a proxy
    public const int MSG_TRUNC     = 0x0020; // Packet was truncated
    public const int MSG_DONTWAIT  = 0x0040; // Nonblocking IO
    public const int MSG_EOR       = 0x0080; // End of record
    public const int MSG_WAITALL   = 0x0100; // Wait for full request
    public const int MSG_FIN       = 0x0200; // Sender will send no more
    public const int MSG_SYN       = 0x0400; // Sender has more to send
    public const int MSG_CONFIRM   = 0x0800; // Confirm path validity
    public const int MSG_RST       = 0x1000; // Reset the connection
    public const int MSG_ERRQUEUE  = 0x2000; // Fetch message from error queue
    public const int MSG_NOSIGNAL  = 0x4000; // Do not generate SIGPIPE
    public const int MSG_MORE      = 0x8000; // Sender will send more

    public const int SCHED_OTHER    = 0;
    public const int SCHED_FIFO     = 1;
    public const int SCHED_RR       = 2;
    public const int SCHED_BATCH    = 3;
    public const int SCHED_IDLE     = 5;
    public const int SCHED_DEADLINE = 6;

    public const int CLOCK_MONOTONIC = 1;

    // int * __errno_location(void);
    [LibraryImport(Lib, EntryPoint = "__errno_location")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxErrorNumber* __errno_location();

    // char *strerror(int errnum);
    [LibraryImport(Lib, EntryPoint = "strerror")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial byte* strerror(LinuxErrorNumber errnum);

    [LibraryImport(Lib, EntryPoint = "vsnprintf")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial int vsnprintf(byte* str, nuint size, byte* format, void* ap);

    // int close(int fd);
    [LibraryImport(Lib, EntryPoint = "close")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult close(FileDescriptor fd);

    // int open(const char *pathname, int flags, mode_t mode);
    [LibraryImport(Lib, EntryPoint = "open", StringMarshalling = StringMarshalling.Utf8)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<FileDescriptor> open(string path, LinuxFileFlags flags, UnixFileMode mode);

    // ssize_t read(int fd, void* buf, size_t count);
    [LibraryImport(Lib, EntryPoint = "read")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> read(FileDescriptor fd, void* buf, nuint count);

    // ssize_t write(int fd, const void* buf, size_t count);
    [LibraryImport(Lib, EntryPoint = "write")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> write(FileDescriptor fd, void* buf, nuint count);

    // int ioctl(int fd, unsigned long operation, ...);
    [LibraryImport(Lib, EntryPoint = "ioctl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult ioctl(FileDescriptor fd, ulong operation, void* argp);

    // int socket(int domain, int type, int protocol);
    [LibraryImport(Lib, EntryPoint = "socket")]
    public static partial LinuxResult<FileDescriptor> socket(LinuxAddressFamily domain, LinuxSocketType type, ProtocolType protocol);

    // int getsockopt(socklen *restrict optlen; int sockfd, int level, int optname, void optval[_Nullable restrict *optlen], socklen_t *restrict optlen);
    [LibraryImport(Lib, EntryPoint = "getsockopt")]
    public static partial LinuxResult getsockopt(FileDescriptor sockfd, LinuxSocketOptionLevel level, int optname, void* optval, ref uint optlen);

    // int setsockopt(socklen_t optlen; int sockfd, int level, int optname, const void optval[optlen], socklen_t optlen);
    [LibraryImport(Lib, EntryPoint = "setsockopt")]
    public static partial LinuxResult setsockopt(FileDescriptor sockfd, LinuxSocketOptionLevel level, int optname, void* optval, uint optlen);

    // int bind(int sockfd, const struct sockaddr *addr, socklen_t addrlen);
    [LibraryImport(Lib, EntryPoint = "bind")]
    public static partial LinuxResult bind(FileDescriptor sockfd, sockaddr* addr, uint addrlen);

    // int getsockname(int sockfd, struct sockaddr *restrict addr, socklen_t *restrict addrlen);
    [LibraryImport(Lib, EntryPoint = "getsockname")]
    public static partial LinuxResult getsockname(FileDescriptor sockfd, sockaddr* addr, ref uint addrlen);

    // int connect(int sockfd, const struct sockaddr *addr, socklen_t addrlen);
    [LibraryImport(Lib, EntryPoint = "connect")]
    public static partial LinuxResult connect(FileDescriptor sockfd, sockaddr* addr, uint addrlen);

    // ssize_t send(size_t size; int sockfd, const void buf[size], size_t size, int flags);
    [LibraryImport(Lib, EntryPoint = "send")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> send(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // ssize_t recv(size_t size; int sockfd, void buf[size], size_t size, int flags);
    [LibraryImport(Lib, EntryPoint = "recv")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> recv(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // int eventfd(unsigned int initval, int flags);
    [LibraryImport(Lib, EntryPoint = "eventfd")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<FileDescriptor> eventfd(uint initval, int flags);

    // int poll(struct pollfd *fds, nfds_t nfds, int timeout);
    [LibraryImport(Lib, EntryPoint = "poll")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<int> poll(pollfd* fds, uint nfds, int timeout);

    // int sched_setscheduler(pid_t pid, int policy, const struct sched_param *param);
    [LibraryImport(Lib, EntryPoint = "sched_setscheduler")]
    public static partial LinuxResult sched_setscheduler(int pid, int policy, in sched_param param);

    // int fstat(int fd, struct stat *statbuf);
    [LibraryImport(Lib, EntryPoint = "fstat")]
    public static partial LinuxResult fstat(FileDescriptor fd, out stat statbuf);

    // int clock_gettime(clockid_t clockid, struct timespec *tp);
    [LibraryImport(Lib, EntryPoint = "clock_gettime")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult clock_gettime(int clockid, out timespec tp);

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct pollfd
    {
        public readonly FileDescriptor fd; // File descriptor to poll
        public readonly short events; // Types of events poller cares about
        public readonly short revents; // Types of events that actually occurred
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct sched_param
    {
        public int sched_priority;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct timespec
    {
        public readonly long tv_sec;  // seconds
        public readonly long tv_nsec; // nanoseconds
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct stat
    {
        public readonly ulong st_dev;
        public readonly ulong st_ino;
        public readonly ulong st_nlink;
        public readonly uint st_mode;
        public readonly uint st_uid;
        public readonly uint st_gid;
        public readonly uint __pad0;
        public readonly ulong st_rdev;
        public readonly long st_size;
        public readonly long st_blksize;
        public readonly long st_blocks;
        public readonly timespec st_atim;
        public readonly timespec st_mtim;
        public readonly timespec st_ctim;
        public readonly long __glibc_reserved0;
        public readonly long __glibc_reserved1;
        public readonly long __glibc_reserved2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct sockaddr 
    {
        public readonly ushort sa_family;
    }
}