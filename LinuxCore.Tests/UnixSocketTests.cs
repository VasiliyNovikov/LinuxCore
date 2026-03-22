using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public partial class UnixSocketTests
{
    [TestMethod]
    public void UnixSocket_Create_Sets_CloseOnExec()
    {
        using var socket = new UnixSocket();
        Assert.IsTrue(socket.CloseOnExec);
    }

    [TestMethod]
    public void UnixSocket_WrappingConstructor_DoesNotChangeCloseOnExec()
    {
        using var original = new UnixSocket();
        using var duplicate = new UnixSocket(original.Descriptor.Clone());

        Assert.IsTrue(original.CloseOnExec);
        Assert.IsFalse(duplicate.CloseOnExec);
    }

    [TestMethod]
    public void UnixSocket_Accept_Sets_CloseOnExec()
    {
        var socketPath = CreateSocketPath();
        try
        {
            using var listener = new UnixSocket();
            listener.Bind(UnixSocketAddress.FromPath(socketPath));
            listener.Listen(1);

            using var client = new UnixSocket();
            client.Connect(UnixSocketAddress.FromPath(socketPath));

            using var accepted = listener.Accept();
            Assert.IsTrue(accepted.CloseOnExec);
            Assert.IsTrue((accepted.Flags & LinuxFileFlags.NonBlock) == 0);
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocket_Accept_Inherits_NonBlocking_FromListener()
    {
        var socketPath = CreateSocketPath();
        try
        {
            using var listener = new UnixSocket(LinuxSocketType.Stream | LinuxSocketType.NonBlocking);
            listener.Bind(UnixSocketAddress.FromPath(socketPath));
            listener.Listen(1);

            using var client = new UnixSocket();
            client.Connect(UnixSocketAddress.FromPath(socketPath));

            using var accepted = listener.Accept();
            Assert.IsTrue((accepted.Flags & LinuxFileFlags.NonBlock) != 0);
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocket_TryAccept_WithoutPendingConnection_ReturnsFalse()
    {
        var socketPath = CreateSocketPath();
        try
        {
            using var listener = new UnixSocket(LinuxSocketType.Stream | LinuxSocketType.NonBlocking);
            listener.Bind(UnixSocketAddress.FromPath(socketPath));
            listener.Listen(1);

            Assert.IsFalse(listener.TryAccept(out var accepted));
            Assert.IsNull(accepted);
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocket_TryAccept_OnBlockingListener_ThrowsInvalidOperationException()
    {
        var socketPath = CreateSocketPath();
        try
        {
            using var listener = new UnixSocket();
            listener.Bind(UnixSocketAddress.FromPath(socketPath));
            listener.Listen(1);

            _ = Assert.ThrowsExactly<InvalidOperationException>(() => listener.TryAccept(out _));
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocket_Pathname_Stream_RoundTrips_And_GetLocalAddress_IsExact()
    {
        var socketPath = CreateSocketPath();
        try
        {
            var address = UnixSocketAddress.FromPath(socketPath);

            using var listener = new UnixSocket();
            listener.Bind(address);
            listener.Listen(1);

            Assert.AreEqual(address, listener.GetLocalAddress());

            using var client = new UnixSocket();
            client.Connect(address);

            using var accepted = listener.Accept();

            var payload = Encoding.ASCII.GetBytes("Hello, Unix pathname socket!");
            Assert.AreEqual(payload.Length, client.Send(payload));

            Span<byte> buffer = stackalloc byte[payload.Length];
            Assert.AreEqual(payload.Length, accepted.Receive(buffer));
            CollectionAssert.AreEqual(payload, buffer.ToArray());
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocket_Abstract_Stream_RoundTrips_And_GetLocalAddress_IsExact()
    {
        var address = UnixSocketAddress.FromAbstractName(CreateAbstractPayload());

        using var listener = new UnixSocket();
        listener.Bind(address);
        listener.Listen(1);

        CollectionAssert.AreEqual(address.ToArray(), listener.GetLocalAddress().ToArray());
        Assert.IsTrue(listener.GetLocalAddress().IsAbstract);

        using var client = new UnixSocket();
        client.Connect(address);

        using var accepted = listener.Accept();

        var payload = Encoding.ASCII.GetBytes("Hello, abstract socket!");
        Assert.AreEqual(payload.Length, client.Send(payload));

        Span<byte> buffer = stackalloc byte[payload.Length];
        Assert.AreEqual(payload.Length, accepted.Receive(buffer));
        CollectionAssert.AreEqual(payload, buffer.ToArray());
    }

    [TestMethod]
    public void UnixSocket_GetLocalAddress_OnUnboundSocket_IsUnnamed()
    {
        using var socket = new UnixSocket(LinuxSocketType.Datagram);
        Assert.IsTrue(socket.GetLocalAddress().IsUnnamed);
    }

    [TestMethod]
    public void UnixSocket_ReceiveFrom_Decodes_NamedSender_Address()
    {
        var receiverAddress = UnixSocketAddress.FromAbstractName(CreateAbstractPayload());
        var senderAddress = UnixSocketAddress.FromAbstractName(CreateAbstractPayload());

        using var receiver = new UnixSocket(LinuxSocketType.Datagram);
        receiver.Bind(receiverAddress);

        using var sender = new UnixSocket(LinuxSocketType.Datagram);
        sender.Bind(senderAddress);

        var payload = Encoding.ASCII.GetBytes("ping");
        Assert.AreEqual(payload.Length, sender.SendTo(receiverAddress, payload));

        Span<byte> buffer = stackalloc byte[payload.Length];
        Assert.AreEqual(payload.Length, receiver.ReceiveFrom(out var receivedAddress, buffer));
        Assert.AreEqual(senderAddress, receivedAddress);
        CollectionAssert.AreEqual(payload, buffer.ToArray());

        var secondPayload = Encoding.ASCII.GetBytes("pong");
        Assert.AreEqual(secondPayload.Length, sender.SendTo(receiverAddress, secondPayload));

        Span<byte> secondBuffer = stackalloc byte[secondPayload.Length];
        Assert.IsTrue(receiver.TryReceiveFrom(out var tryReceivedAddress, secondBuffer, out var receivedCount));
        Assert.AreEqual((nuint)secondPayload.Length, receivedCount);
        Assert.AreEqual(senderAddress, tryReceivedAddress);
        CollectionAssert.AreEqual(secondPayload, secondBuffer.ToArray());
    }

    [TestMethod]
    public void UnixSocket_ReceiveFrom_Decodes_UnnamedSender_Address()
    {
        var receiverAddress = UnixSocketAddress.FromAbstractName(CreateAbstractPayload());

        using var receiver = new UnixSocket(LinuxSocketType.Datagram);
        receiver.Bind(receiverAddress);

        using var sender = new UnixSocket(LinuxSocketType.Datagram);

        var payload = Encoding.ASCII.GetBytes("left");
        Assert.AreEqual(payload.Length, sender.SendTo(receiverAddress, payload));

        Span<byte> buffer = stackalloc byte[payload.Length];
        Assert.AreEqual(payload.Length, receiver.ReceiveFrom(out var receivedAddress, buffer));
        Assert.IsTrue(receivedAddress.IsUnnamed);
        CollectionAssert.AreEqual(payload, buffer.ToArray());

        var secondPayload = Encoding.ASCII.GetBytes("right");
        Assert.AreEqual(secondPayload.Length, sender.SendTo(receiverAddress, secondPayload));

        Span<byte> secondBuffer = stackalloc byte[secondPayload.Length];
        Assert.IsTrue(receiver.TryReceiveFrom(out var tryReceivedAddress, secondBuffer, out var receivedCount));
        Assert.AreEqual((nuint)secondPayload.Length, receivedCount);
        Assert.IsTrue(tryReceivedAddress.IsUnnamed);
        CollectionAssert.AreEqual(secondPayload, secondBuffer.ToArray());
    }

    [TestMethod]
    public unsafe void UnixSocket_GetLocalAddress_Decodes_108Byte_RawPathname()
    {
        var socketPath = CreateMaxNativeSocketPath();
        try
        {
            var pathBytes = Encoding.ASCII.GetBytes(socketPath);
            Assert.AreEqual(RawUnixSocketInterop.PathLength, pathBytes.Length);

            using var socket = new UnixSocket(RawUnixSocketInterop.socket(LinuxAddressFamily.Unix, LinuxSocketType.Stream, 0));

            RawUnixSocketInterop.sockaddr_un nativeAddress = default;
            nativeAddress.sun_family = (ushort)LinuxAddressFamily.Unix;
            pathBytes.CopyTo(MemoryMarshal.CreateSpan(ref nativeAddress.sun_path[0], RawUnixSocketInterop.PathLength));
            RawUnixSocketInterop.bind(socket.Descriptor, &nativeAddress, RawUnixSocketInterop.AddressLength);

            var address = socket.GetLocalAddress();
            Assert.IsTrue(address.IsPathname);
            CollectionAssert.AreEqual(pathBytes, address.ToArray());
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocketAddress_FromPath_Accepts_MaximumLength()
    {
        var path = new string('a', UnixSocketAddress.MaxPayloadLength);
        var address = UnixSocketAddress.FromPath(path);

        Assert.IsTrue(address.IsPathname);
        Assert.AreEqual(path, address.ToUtf8String());
    }

    [TestMethod]
    public void UnixSocketAddress_FromPath_Oversized_ThrowsArgumentOutOfRangeException()
    {
        var path = new string('a', UnixSocketAddress.MaxPayloadLength + 1);
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => UnixSocketAddress.FromPath(path));
    }

    [TestMethod]
    public void UnixSocketAddress_FromPath_EmbeddedNul_ThrowsArgumentException()
    {
        _ = Assert.ThrowsExactly<ArgumentException>(() => UnixSocketAddress.FromPath("abc\0def"));
    }

    [TestMethod]
    public void UnixSocketAddress_FromAbstractName_Accepts_MaximumLength()
    {
        var payload = EnumerableRepeat((byte)0xA5, UnixSocketAddress.MaxPayloadLength);
        var address = UnixSocketAddress.FromAbstractName(payload);

        Assert.IsTrue(address.IsAbstract);
        CollectionAssert.AreEqual(payload, address.ToArray());
    }

    [TestMethod]
    public void UnixSocketAddress_FromAbstractName_Oversized_ThrowsArgumentOutOfRangeException()
    {
        var payload = EnumerableRepeat((byte)0x5A, UnixSocketAddress.MaxPayloadLength + 1);
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => UnixSocketAddress.FromAbstractName(payload));
    }

    private static byte[] CreateAbstractPayload()
    {
        var guidBytes = Guid.NewGuid().ToByteArray();
        return [0x80, 0xFF, 0x00, .. guidBytes];
    }

    private static string CreateSocketPath() => $"/tmp/linuxcore-{Guid.NewGuid():N}.sock";

    private static string CreateMaxNativeSocketPath()
    {
        const string prefix = "/tmp/";
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return prefix + new string('p', RawUnixSocketInterop.PathLength - prefix.Length - suffix.Length) + suffix;
    }

    private static void DeleteSocketPath(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static byte[] EnumerableRepeat(byte value, int count)
    {
        var buffer = new byte[count];
        Array.Fill(buffer, value);
        return buffer;
    }

    private static unsafe partial class RawUnixSocketInterop
    {
        public const int PathLength = 108;
        public const uint AddressLength = 110;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct sockaddr_un
        {
            public ushort sun_family;
            public fixed byte sun_path[PathLength];
        }

        [LibraryImport("libc", EntryPoint = "socket")]
        private static partial int socket_raw(int domain, int type, int protocol);

        [LibraryImport("libc", EntryPoint = "bind")]
        private static partial int bind_raw(int sockfd, sockaddr_un* addr, uint addrlen);

        public static FileDescriptor socket(LinuxAddressFamily domain, LinuxSocketType type, int protocol)
        {
            var descriptor = socket_raw((int)domain, (int)type, protocol);
            return descriptor == -1
                ? throw LinuxException.FromLastError()
                : Unsafe.BitCast<int, FileDescriptor>(descriptor);
        }

        public static void bind(FileDescriptor sockfd, sockaddr_un* addr, uint addrlen)
        {
            if (bind_raw(Unsafe.BitCast<FileDescriptor, int>(sockfd), addr, addrlen) == -1)
                throw LinuxException.FromLastError();
        }
    }
}
