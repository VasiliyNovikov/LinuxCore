using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class UnixSocketTests
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

            Assert.AreEqual(address, listener.LocalAddress);

            using var client = new UnixSocket();
            client.Connect(address);

            using var accepted = listener.Accept();

            var payload = "Hello, Unix pathname socket!"u8;
            Assert.AreEqual(payload.Length, client.Send(payload));

            Span<byte> buffer = stackalloc byte[payload.Length];
            Assert.AreEqual(payload.Length, accepted.Receive(buffer));
            CollectionAssert.AreEqual(payload.ToArray(), buffer.ToArray());
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocket_Abstract_Stream_RoundTrips_And_GetLocalAddress_IsExact()
    {
        var address = UnixSocketAddress.FromAbstractName(CreateAbstractPath());

        using var listener = new UnixSocket();
        listener.Bind(address);
        listener.Listen(1);

        Assert.AreEqual(address, listener.LocalAddress);

        using var client = new UnixSocket();
        client.Connect(address);

        using var accepted = listener.Accept();

        var payload = "Hello, abstract socket!"u8;
        Assert.AreEqual(payload.Length, client.Send(payload));

        Span<byte> buffer = stackalloc byte[payload.Length];
        Assert.AreEqual(payload.Length, accepted.Receive(buffer));
        CollectionAssert.AreEqual(payload.ToArray(), buffer.ToArray());
    }

    [TestMethod]
    public void UnixSocket_GetLocalAddress_OnUnboundSocket_IsUnnamed()
    {
        using var socket = new UnixSocket(LinuxSocketType.Datagram);
        Assert.AreEqual(UnixSocketAddress.Unnamed, socket.LocalAddress);
    }

    [TestMethod]
    public void UnixSocket_ReceiveFrom_Decodes_NamedSender_Address()
    {
        var receiverAddress = UnixSocketAddress.FromAbstractName(CreateAbstractPath());
        var senderAddress = UnixSocketAddress.FromAbstractName(CreateAbstractPath());

        using var receiver = new UnixSocket(LinuxSocketType.Datagram);
        receiver.Bind(receiverAddress);

        using var sender = new UnixSocket(LinuxSocketType.Datagram);
        sender.Bind(senderAddress);

        var payload = "ping"u8;
        Assert.AreEqual(payload.Length, sender.SendTo(receiverAddress, payload));

        Span<byte> buffer = stackalloc byte[payload.Length];
        Assert.AreEqual(payload.Length, receiver.ReceiveFrom(out var receivedAddress, buffer));
        Assert.AreEqual(senderAddress, receivedAddress);
        CollectionAssert.AreEqual(payload.ToArray(), buffer.ToArray());

        var secondPayload = "pong"u8;
        Assert.AreEqual(secondPayload.Length, sender.SendTo(receiverAddress, secondPayload));

        Span<byte> secondBuffer = stackalloc byte[secondPayload.Length];
        Assert.IsTrue(receiver.TryReceiveFrom(out var tryReceivedAddress, secondBuffer, out var receivedCount));
        Assert.AreEqual((nuint)secondPayload.Length, receivedCount);
        Assert.AreEqual(senderAddress, tryReceivedAddress);
        CollectionAssert.AreEqual(secondPayload.ToArray(), secondBuffer.ToArray());
    }

    [TestMethod]
    public void UnixSocket_ReceiveFrom_Decodes_UnnamedSender_Address()
    {
        var receiverAddress = UnixSocketAddress.FromAbstractName(CreateAbstractPath());

        using var receiver = new UnixSocket(LinuxSocketType.Datagram);
        receiver.Bind(receiverAddress);

        using var sender = new UnixSocket(LinuxSocketType.Datagram);

        var payload = "left"u8;
        Assert.AreEqual(payload.Length, sender.SendTo(receiverAddress, payload));

        Span<byte> buffer = stackalloc byte[payload.Length];
        Assert.AreEqual(payload.Length, receiver.ReceiveFrom(out var receivedAddress, buffer));
        Assert.AreEqual(UnixSocketAddress.Unnamed, receivedAddress);
        CollectionAssert.AreEqual(payload.ToArray(), buffer.ToArray());

        var secondPayload = "right"u8;
        Assert.AreEqual(secondPayload.Length, sender.SendTo(receiverAddress, secondPayload));

        Span<byte> secondBuffer = stackalloc byte[secondPayload.Length];
        Assert.IsTrue(receiver.TryReceiveFrom(out var tryReceivedAddress, secondBuffer, out var receivedCount));
        Assert.AreEqual((nuint)secondPayload.Length, receivedCount);
        Assert.AreEqual(UnixSocketAddress.Unnamed, tryReceivedAddress);
        CollectionAssert.AreEqual(secondPayload.ToArray(), secondBuffer.ToArray());
    }

    [TestMethod]
    public void UnixSocket_GetLocalAddress_Decodes_108Byte_RawPathname()
    {
        var socketPath = CreateMaxSocketPath();
        try
        {
            var address = UnixSocketAddress.FromPath(socketPath);
            Assert.AreEqual(UnixSocketAddress.MaxPathLength, address.Length);

            using var socket = new UnixSocket();
            socket.Bind(address);

            var localAddress = socket.LocalAddress;
            Assert.AreEqual(address, localAddress);
        }
        finally
        {
            DeleteSocketPath(socketPath);
        }
    }

    [TestMethod]
    public void UnixSocketAddress_FromPath_Accepts_MaximumLength()
    {
        var path = new string('a', UnixSocketAddress.MaxPathLength);
        var address = UnixSocketAddress.FromPath(path);

        Assert.AreEqual(UnixSocketAddressKind.PathName, address.Kind);
        Assert.AreEqual(path, address.Path);
    }

    [TestMethod]
    public void UnixSocketAddress_FromPath_Oversized_ThrowsArgumentOutOfRangeException()
    {
        var path = new string('a', UnixSocketAddress.MaxPathLength + 1);
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => UnixSocketAddress.FromPath(path));
    }

    [TestMethod]
    public void UnixSocketAddress_FromPath_EmbeddedNull_ThrowsArgumentException()
    {
        _ = Assert.ThrowsExactly<ArgumentException>(() => UnixSocketAddress.FromPath("abc\0def"));
    }

    [TestMethod]
    public void UnixSocketAddress_FromAbstractName_Accepts_MaximumLength()
    {
        var name = new string('a', UnixSocketAddress.MaxAbstractNameLength);
        var address = UnixSocketAddress.FromAbstractName(name);

        Assert.AreEqual(UnixSocketAddressKind.Abstract, address.Kind);
        Assert.AreEqual(name, address.Path);
    }

    [TestMethod]
    public void UnixSocketAddress_FromAbstractName_Oversized_ThrowsArgumentOutOfRangeException()
    {
        var name = new string('a', UnixSocketAddress.MaxAbstractNameLength + 1);
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => UnixSocketAddress.FromAbstractName(name));
    }

    [TestMethod]
    public void UnixSocket_SendFileDescriptors_RoundTrips_FileDescriptor()
    {
        var address = UnixSocketAddress.FromAbstractName(CreateAbstractPath());

        using var sender = new UnixSocket(LinuxSocketType.Datagram);
        using var receiver = new UnixSocket(LinuxSocketType.Datagram);
        receiver.Bind(address);
        sender.Connect(address);

        using var memfd = new LinuxMemoryFile("test-fd-passing");
        var payload = "fd-content"u8;
        memfd.Write(payload);

        var body = "hello"u8;
        Assert.AreEqual(body.Length, sender.SendFileDescriptors(body, [memfd.Descriptor]));

        Span<byte> recvBuffer = stackalloc byte[body.Length];
        Span<FileDescriptor> recvFds = stackalloc FileDescriptor[1];
        Assert.AreEqual(body.Length, receiver.ReceiveFileDescriptors(recvBuffer, recvFds, out var fdCount, out var msgFlags));
        Assert.AreEqual(1, fdCount);
        Assert.AreEqual(LinuxSocketMessageFlags.None, msgFlags & LinuxSocketMessageFlags.ControlTruncated);
        CollectionAssert.AreEqual(body.ToArray(), recvBuffer.ToArray());
        var recvFd = recvFds[0];
        Assert.AreNotEqual(memfd.Descriptor, recvFd);

        using var recvFile = new LinuxMemoryFile(recvFd);
        Assert.IsTrue(recvFile.CloseOnExec);
        Assert.AreEqual(payload.Length, recvFile.Size);
    }

    [TestMethod]
    public void UnixSocket_SendFileDescriptors_WithMinimalPayload_Succeeds()
    {
        var address = UnixSocketAddress.FromAbstractName(CreateAbstractPath());

        using var sender = new UnixSocket(LinuxSocketType.Datagram);
        using var receiver = new UnixSocket(LinuxSocketType.Datagram);
        receiver.Bind(address);
        sender.Connect(address);

        using var memfd = new LinuxMemoryFile("test-fd-minimal");
        var payload = "memfd-data"u8;
        memfd.Write(payload);

        ReadOnlySpan<byte> body = stackalloc byte[1];
        Assert.AreEqual(1, sender.SendFileDescriptors(body, [memfd.Descriptor]));

        Span<byte> recvBuffer = stackalloc byte[1];
        Span<FileDescriptor> recvFds = stackalloc FileDescriptor[1];
        Assert.AreEqual(1, receiver.ReceiveFileDescriptors(recvBuffer, recvFds, out var fdCount, out _));
        Assert.AreEqual(1, fdCount);
        var recvFd = recvFds[0];
        Assert.AreNotEqual(memfd.Descriptor, recvFd);

        using var recvFile = new LinuxMemoryFile(recvFd);
        Assert.IsTrue(recvFile.CloseOnExec);
        Assert.AreEqual(payload.Length, recvFile.Size);
    }

    private static string CreateAbstractPath() => Encoding.ASCII.GetString([0x80, 0xFF, 0x00, .. Guid.NewGuid().ToByteArray()]);

    private static string CreateSocketPath() => $"/tmp/linuxcore-{Guid.NewGuid():N}.sock";

    private static string CreateMaxSocketPath()
    {
        const string prefix = "/tmp/";
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return prefix + new string('p', UnixSocketAddress.MaxPathLength - prefix.Length - suffix.Length) + suffix;
    }

    private static void DeleteSocketPath(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}