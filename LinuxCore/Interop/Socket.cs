using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Socket
{
    public const int MSG_OOB            = 0x0001;     // Process out-of-band data
    public const int MSG_PEEK           = 0x0002;     // Peek at incoming message
    public const int MSG_DONTROUTE      = 0x0004;     // Don't route
    public const int MSG_CTRUNC         = 0x0008;     // Control data lost
    public const int MSG_PROXY          = 0x0010;     // Supply or originate a proxy
    public const int MSG_TRUNC          = 0x0020;     // Packet was truncated
    public const int MSG_DONTWAIT       = 0x0040;     // Nonblocking IO
    public const int MSG_EOR            = 0x0080;     // End of record
    public const int MSG_WAITALL        = 0x0100;     // Wait for full request
    public const int MSG_FIN            = 0x0200;     // Sender will send no more
    public const int MSG_SYN            = 0x0400;     // Sender has more to send
    public const int MSG_CONFIRM        = 0x0800;     // Confirm path validity
    public const int MSG_RST            = 0x1000;     // Reset the connection
    public const int MSG_ERRQUEUE       = 0x2000;     // Fetch message from error queue
    public const int MSG_NOSIGNAL       = 0x4000;     // Do not generate SIGPIPE
    public const int MSG_MORE           = 0x8000;     // Sender will send more
    public const int MSG_CMSG_CLOEXEC   = 0x40000000; // Set FD_CLOEXEC on received fds

    public const int SCM_RIGHTS      = 0x01;
    public const int SCM_CREDENTIALS = 0x02;

    public const byte SOCKADDR_UN_PATH_OFFSET = 2;
    public const byte SOCKADDR_UN_PATH_LENGTH = 108;

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct sockaddr
    {
        public readonly ushort sa_family;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct sockaddr_un
    {
        public ushort sun_family;
        public fixed byte sun_path[SOCKADDR_UN_PATH_LENGTH];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct iovec
    {
        public void* iov_base;
        public nuint iov_len;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct msghdr
    {
        public void*  msg_name;
        public uint   msg_namelen;
        public iovec* msg_iov;
        public nuint  msg_iovlen;
        public void*  msg_control;
        public nuint  msg_controllen;
        public int    msg_flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct cmsghdr
    {
        public nuint cmsg_len;
        public int   cmsg_level;
        public int   cmsg_type;
    }

    // int socket(int domain, int type, int protocol);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "socket")]
    public static partial LinuxResult<FileDescriptor> socket(LinuxAddressFamily domain, LinuxSocketType type, ProtocolType protocol);

    // int getsockopt(socklen *restrict optlen; int sockfd, int level, int optname, void optval[_Nullable restrict *optlen], socklen_t *restrict optlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getsockopt")]
    public static partial LinuxResult getsockopt(FileDescriptor sockfd, LinuxSocketOptionLevel level, int optname, void* optval, ref uint optlen);

    // int setsockopt(socklen_t optlen; int sockfd, int level, int optname, const void optval[optlen], socklen_t optlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "setsockopt")]
    public static partial LinuxResult setsockopt(FileDescriptor sockfd, LinuxSocketOptionLevel level, int optname, void* optval, uint optlen);

    // int bind(int sockfd, const struct sockaddr *addr, socklen_t addrlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "bind")]
    public static partial LinuxResult bind(FileDescriptor sockfd, sockaddr* addr, uint addrlen);

    // int listen(int sockfd, int backlog);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "listen")]
    public static partial LinuxResult listen(FileDescriptor sockfd, int backlog);

    // int accept4(int sockfd, struct sockaddr *_Nullable restrict addr, socklen_t *_Nullable restrict addrlen, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "accept4")]
    public static partial LinuxResult<FileDescriptor> accept4(FileDescriptor sockfd, sockaddr* addr, uint* addrlen, LinuxSocketType flags);

    // int getsockname(int sockfd, struct sockaddr *restrict addr, socklen_t *restrict addrlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getsockname")]
    public static partial LinuxResult getsockname(FileDescriptor sockfd, sockaddr* addr, ref uint addrlen);

    // int connect(int sockfd, const struct sockaddr *addr, socklen_t addrlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "connect")]
    public static partial LinuxResult connect(FileDescriptor sockfd, sockaddr* addr, uint addrlen);

    // ssize_t send(size_t size; int sockfd, const void buf[size], size_t size, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "send")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> send(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // ssize_t send(size_t size; int sockfd, const void buf[size], size_t size, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "send")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> send_noblock(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // ssize_t sendto(int socket, const void *message, size_t length, int flags, const struct sockaddr *dest_addr, socklen_t dest_len);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sendto")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> sendto(FileDescriptor socket, void* message, nuint length, int flags, sockaddr* dest_addr, uint dest_len);

    // ssize_t sendto(int socket, const void *message, size_t length, int flags, const struct sockaddr *dest_addr, socklen_t dest_len);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sendto")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> sendto_noblock(FileDescriptor socket, void* message, nuint length, int flags, sockaddr* dest_addr, uint dest_len);

    // ssize_t recv(size_t size; int sockfd, void buf[size], size_t size, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recv")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> recv(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // ssize_t recv(size_t size; int sockfd, void buf[size], size_t size, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recv")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> recv_noblock(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // ssize_t recvfrom(int socket, void *restrict buffer, size_t length, int flags, struct sockaddr *restrict address, socklen_t *restrict address_len);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recvfrom")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> recvfrom(FileDescriptor socket, void* buffer, nuint length, int flags, sockaddr* address, ref uint address_len);

    // ssize_t recvfrom(int socket, void *restrict buffer, size_t length, int flags, struct sockaddr *restrict address, socklen_t *restrict address_len);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recvfrom")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> recvfrom_noblock(FileDescriptor socket, void* buffer, nuint length, int flags, sockaddr* address, ref uint address_len);

    // ssize_t sendmsg(int sockfd, const struct msghdr *msg, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sendmsg")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> sendmsg(FileDescriptor sockfd, msghdr* msg, int flags);

    // ssize_t sendmsg(int sockfd, const struct msghdr *msg, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "sendmsg")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> sendmsg_noblock(FileDescriptor sockfd, msghdr* msg, int flags);

    // ssize_t recvmsg(int sockfd, struct msghdr *msg, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recvmsg")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> recvmsg(FileDescriptor sockfd, msghdr* msg, int flags);

    // ssize_t recvmsg(int sockfd, struct msghdr *msg, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recvmsg")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> recvmsg_noblock(FileDescriptor sockfd, msghdr* msg, int flags);
}