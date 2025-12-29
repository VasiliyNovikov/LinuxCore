using System.Diagnostics.CodeAnalysis;

namespace LinuxCore;

[SuppressMessage("Style", "IDE0055:Fix formatting")]
public enum LinuxAddressFamily
{
    Unspecified =  0, // AF_UNSPEC
    Unix        =  1, // AF_UNIX
    Inet        =  2, // AF_INET
    Bridge      =  7, // AF_BRIDGE
    Inet6       = 10, // AF_INET6
    Netlink     = 16, // AF_NETLINK
    Packet      = 17, // AF_PACKET
    LLC         = 26  // AF_LLC
}