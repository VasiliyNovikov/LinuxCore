using System.Diagnostics.CodeAnalysis;

namespace LinuxCore;

[SuppressMessage("Style", "IDE0055:Fix formatting")]
public enum LinuxSocketOptionLevel
{
    Socket     =   1, // SOL_SOCKET
    TCP        =   6, // SOL_TCP
    UDP        =  17, // SOL_UDP
    IP         =   0, // SOL_IP
    IPv6       =  41, // SOL_IPV6
    Raw        = 255, // SOL_RAW
    Netlink    = 270  // SOL_NETLINK
}