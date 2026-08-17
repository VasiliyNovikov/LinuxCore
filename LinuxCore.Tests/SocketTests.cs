using Microsoft.VisualStudio.TestTools.UnitTesting;

using NativeSocket = LinuxCore.Interop.Socket;

namespace LinuxCore.Tests;

[TestClass]
public class SocketTests
{
    [TestMethod]
    public void Socket_Constants_Match_Current_Platform_Headers()
    {
        NativeConstantAssert.EnumValuesMatch<LinuxSocketType>(
        [
            (nameof(LinuxSocketType.Stream), "SOCK_STREAM"),
            (nameof(LinuxSocketType.Datagram), "SOCK_DGRAM"),
            (nameof(LinuxSocketType.Raw), "SOCK_RAW"),
            (nameof(LinuxSocketType.RDM), "SOCK_RDM"),
            (nameof(LinuxSocketType.SeqPacket), "SOCK_SEQPACKET"),
            (nameof(LinuxSocketType.NonBlocking), "SOCK_NONBLOCK"),
            (nameof(LinuxSocketType.CloseOnExec), "SOCK_CLOEXEC")
        ], "sys/socket.h");

        NativeConstantAssert.EnumValuesMatch<LinuxAddressFamily>(
        [
            (nameof(LinuxAddressFamily.Unspecified), "AF_UNSPEC"),
            (nameof(LinuxAddressFamily.Unix), "AF_UNIX"),
            (nameof(LinuxAddressFamily.Inet), "AF_INET"),
            (nameof(LinuxAddressFamily.Bridge), "AF_BRIDGE"),
            (nameof(LinuxAddressFamily.Inet6), "AF_INET6"),
            (nameof(LinuxAddressFamily.Netlink), "AF_NETLINK"),
            (nameof(LinuxAddressFamily.Packet), "AF_PACKET"),
            (nameof(LinuxAddressFamily.LLC), "AF_LLC")
        ], "sys/socket.h");

        NativeConstantAssert.EnumValuesMatch<LinuxSocketOptionLevel>(
        [
            (nameof(LinuxSocketOptionLevel.Socket), "SOL_SOCKET"),
            (nameof(LinuxSocketOptionLevel.TCP), "SOL_TCP"),
            (nameof(LinuxSocketOptionLevel.UDP), "SOL_UDP"),
            (nameof(LinuxSocketOptionLevel.IP), "SOL_IP"),
            (nameof(LinuxSocketOptionLevel.IPv6), "SOL_IPV6"),
            (nameof(LinuxSocketOptionLevel.Raw), "SOL_RAW"),
            (nameof(LinuxSocketOptionLevel.Netlink), "SOL_NETLINK")
        ], "sys/socket.h", "netinet/in.h", "netinet/tcp.h", "netinet/udp.h");

        NativeConstantAssert.EnumValuesMatch<LinuxSocketMessageFlags>(
        [
            (nameof(LinuxSocketMessageFlags.None), "0"),
            (nameof(LinuxSocketMessageFlags.OutOfBand), "MSG_OOB"),
            (nameof(LinuxSocketMessageFlags.Peek), "MSG_PEEK"),
            (nameof(LinuxSocketMessageFlags.DontWait), "MSG_DONTWAIT"),
            (nameof(LinuxSocketMessageFlags.WaitAll), "MSG_WAITALL"),
            (nameof(LinuxSocketMessageFlags.ControlTruncated), "MSG_CTRUNC"),
            (nameof(LinuxSocketMessageFlags.CmsgCloseOnExec), "MSG_CMSG_CLOEXEC")
        ], "sys/socket.h");

        NativeConstantAssert.EnumValuesMatch<LinuxControlMessageType>(
        [
            (nameof(LinuxControlMessageType.ScmRights), "SCM_RIGHTS"),
            (nameof(LinuxControlMessageType.ScmCredentials), "SCM_CREDENTIALS")
        ], "sys/socket.h");

        NativeConstantAssert.ValuesMatch(
        [
            (nameof(NativeSocket.MSG_OOB), NativeSocket.MSG_OOB, "MSG_OOB"),
            (nameof(NativeSocket.MSG_PEEK), NativeSocket.MSG_PEEK, "MSG_PEEK"),
            (nameof(NativeSocket.MSG_DONTROUTE), NativeSocket.MSG_DONTROUTE, "MSG_DONTROUTE"),
            (nameof(NativeSocket.MSG_CTRUNC), NativeSocket.MSG_CTRUNC, "MSG_CTRUNC"),
            (nameof(NativeSocket.MSG_PROXY), NativeSocket.MSG_PROXY, "MSG_PROXY"),
            (nameof(NativeSocket.MSG_TRUNC), NativeSocket.MSG_TRUNC, "MSG_TRUNC"),
            (nameof(NativeSocket.MSG_DONTWAIT), NativeSocket.MSG_DONTWAIT, "MSG_DONTWAIT"),
            (nameof(NativeSocket.MSG_EOR), NativeSocket.MSG_EOR, "MSG_EOR"),
            (nameof(NativeSocket.MSG_WAITALL), NativeSocket.MSG_WAITALL, "MSG_WAITALL"),
            (nameof(NativeSocket.MSG_FIN), NativeSocket.MSG_FIN, "MSG_FIN"),
            (nameof(NativeSocket.MSG_SYN), NativeSocket.MSG_SYN, "MSG_SYN"),
            (nameof(NativeSocket.MSG_CONFIRM), NativeSocket.MSG_CONFIRM, "MSG_CONFIRM"),
            (nameof(NativeSocket.MSG_RST), NativeSocket.MSG_RST, "MSG_RST"),
            (nameof(NativeSocket.MSG_ERRQUEUE), NativeSocket.MSG_ERRQUEUE, "MSG_ERRQUEUE"),
            (nameof(NativeSocket.MSG_NOSIGNAL), NativeSocket.MSG_NOSIGNAL, "MSG_NOSIGNAL"),
            (nameof(NativeSocket.MSG_MORE), NativeSocket.MSG_MORE, "MSG_MORE"),
            (nameof(NativeSocket.MSG_CMSG_CLOEXEC), NativeSocket.MSG_CMSG_CLOEXEC, "MSG_CMSG_CLOEXEC"),
            (nameof(NativeSocket.SCM_RIGHTS), NativeSocket.SCM_RIGHTS, "SCM_RIGHTS"),
            (nameof(NativeSocket.SCM_CREDENTIALS), NativeSocket.SCM_CREDENTIALS, "SCM_CREDENTIALS")
        ], "sys/socket.h");
    }
}