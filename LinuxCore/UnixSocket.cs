using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static LinuxCore.Interop.Socket;

namespace LinuxCore;

public sealed unsafe class UnixSocket : LinuxSocketBase
{
    public UnixSocketAddress LocalAddress
    {
        get
        {
            GetAddress(out sockaddr_un nativeAddress, out var addressLength);
            return UnixSocketAddress.FromNative(nativeAddress, addressLength);
        }
    }

    /// <summary>
    /// Wraps an existing Unix socket descriptor.
    /// </summary>
    /// <param name="descriptor">The socket descriptor to wrap. It is not duplicated.</param>
    /// <param name="ownsDescriptor">
    /// Whether disposal closes <paramref name="descriptor"/>. When <see langword="false"/>, the external
    /// owner must keep the descriptor open and prevent concurrent closure while this object is in use.
    /// </param>
    public UnixSocket(FileDescriptor descriptor, bool ownsDescriptor = true)
        : base(descriptor, ownsDescriptor)
    {
    }

    public UnixSocket(LinuxSocketType type = LinuxSocketType.Stream, ProtocolType protocol = default)
        : base(LinuxAddressFamily.Unix, type, protocol)
    {
    }

    public void Bind(in UnixSocketAddress address)
    {
        address.ToNative(out sockaddr_un nativeAddress, out var addressLength);
        Bind(in nativeAddress, addressLength);
    }

    public void Connect(in UnixSocketAddress address)
    {
        address.ToNative(out sockaddr_un nativeAddress, out var addressLength);
        Connect(in nativeAddress, addressLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SendTo(in UnixSocketAddress address, ReadOnlySpan<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        address.ToNative(out sockaddr_un nativeAddress, out var addressLength);
        return SendTo(in nativeAddress, addressLength, buffer, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReceiveFrom(out UnixSocketAddress address, Span<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        var receivedCount = ReceiveFrom(out sockaddr_un nativeAddress, out var addressLength, buffer, flags);
        address = UnixSocketAddress.FromNative(nativeAddress, addressLength);
        return receivedCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySendTo(in UnixSocketAddress address, ReadOnlySpan<byte> buffer, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        address.ToNative(out var nativeAddress, out var addressLength);
        return TrySendTo(in nativeAddress, addressLength, buffer, out sentCount, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReceiveFrom(out UnixSocketAddress address, Span<byte> buffer, out nuint receivedCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        if (TryReceiveFrom(out sockaddr_un nativeAddress, out var addressLength, buffer, out receivedCount, flags))
        {
            address = UnixSocketAddress.FromNative(nativeAddress, addressLength);
            return true;
        }

        address = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SendFileDescriptors(ReadOnlySpan<byte> buffer, ReadOnlySpan<FileDescriptor> fileDescriptors, LinuxSocketMessageFlags flags = default)
    {
        return SendMessage(buffer, LinuxSocketOptionLevel.Socket, LinuxControlMessageType.ScmRights, MemoryMarshal.AsBytes(fileDescriptors), flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReceiveFileDescriptors(Span<byte> buffer, Span<FileDescriptor> fileDescriptors, out int receivedDescriptorCount, out LinuxSocketMessageFlags receivedMessageFlags, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.CmsgCloseOnExec)
    {
        var receivedCount = ReceiveMessage(buffer, LinuxSocketOptionLevel.Socket, LinuxControlMessageType.ScmRights, MemoryMarshal.AsBytes(fileDescriptors), out var receivedControlCount, out receivedMessageFlags, flags);
        receivedDescriptorCount = receivedControlCount / sizeof(FileDescriptor);
        return receivedCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new UnixSocket Accept() => new(base.Accept());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAccept([NotNullWhen(true)] out UnixSocket? socket)
    {
        if (base.TryAccept(out var descriptor))
        {
            socket = new UnixSocket(descriptor);
            return true;
        }

        socket = null;
        return false;
    }
}