using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Socket
{
    public const int MSG_OOB       = 0x0001; // Process out-of-band data
    public const int MSG_PEEK      = 0x0002; // Peek at incoming message
    public const int MSG_DONTROUTE = 0x0004; // Don't route
    public const int MSG_CTRUNC    = 0x0008; // Control data lost
    public const int MSG_PROXY     = 0x0010; // Supply or originate a proxy
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

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct sockaddr
    {
        public readonly ushort sa_family;
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

    // int getsockname(int sockfd, struct sockaddr *restrict addr, socklen_t *restrict addrlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getsockname")]
    public static partial LinuxResult getsockname(FileDescriptor sockfd, sockaddr* addr, ref uint addrlen);

    // int connect(int sockfd, const struct sockaddr *addr, socklen_t addrlen);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "connect")]
    public static partial LinuxResult connect(FileDescriptor sockfd, sockaddr* addr, uint addrlen);

    // ssize_t send(size_t size; int sockfd, const void buf[size], size_t size, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "send")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> send(FileDescriptor sockfd, void* buf, nuint size, int flags);

    // ssize_t recv(size_t size; int sockfd, void buf[size], size_t size, int flags);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "recv")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> recv(FileDescriptor sockfd, void* buf, nuint size, int flags);
}