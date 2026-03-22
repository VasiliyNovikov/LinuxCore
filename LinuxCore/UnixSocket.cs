using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using SocketInterop = LinuxCore.Interop.Socket;
using static LinuxCore.Interop.Socket;

namespace LinuxCore;

public sealed unsafe class UnixSocket : LinuxSocketBase
{
    public UnixSocket(FileDescriptor descriptor, bool ownsDescriptor = true)
        : base(descriptor, ownsDescriptor)
    {
    }

    public UnixSocket(LinuxSocketType type = LinuxSocketType.Stream, ProtocolType protocol = default)
        : base(LinuxAddressFamily.Unix, type, protocol)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Bind(in UnixSocketAddress address)
    {
        address.WriteTo(out SocketInterop.sockaddr_un nativeAddress, out var addressLength);
        Bind(in nativeAddress, addressLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Connect(in UnixSocketAddress address)
    {
        address.WriteTo(out SocketInterop.sockaddr_un nativeAddress, out var addressLength);
        Connect(in nativeAddress, addressLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnixSocketAddress GetLocalAddress()
    {
        GetAddress(out SocketInterop.sockaddr_un nativeAddress, out var addressLength);
        return UnixSocketAddress.FromNative(nativeAddress, addressLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SendTo(in UnixSocketAddress address, ReadOnlySpan<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        address.WriteTo(out SocketInterop.sockaddr_un nativeAddress, out var addressLength);
        return SendTo(in nativeAddress, addressLength, buffer, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReceiveFrom(out UnixSocketAddress address, Span<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        var receivedCount = ReceiveFrom(out SocketInterop.sockaddr_un nativeAddress, out var addressLength, buffer, flags);
        address = UnixSocketAddress.FromNative(nativeAddress, addressLength);
        return receivedCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySendTo(in UnixSocketAddress address, ReadOnlySpan<byte> buffer, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        address.WriteTo(out SocketInterop.sockaddr_un nativeAddress, out var addressLength);
        return TrySendTo(in nativeAddress, addressLength, buffer, out sentCount, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReceiveFrom(out UnixSocketAddress address, Span<byte> buffer, out nuint receivedCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        if (TryReceiveFrom(out SocketInterop.sockaddr_un nativeAddress, out var addressLength, buffer, out receivedCount, flags))
        {
            address = UnixSocketAddress.FromNative(nativeAddress, addressLength);
            return true;
        }

        address = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Listen(int backlog)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(backlog);
        listen(Descriptor, backlog).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnixSocket Accept() => new(accept4(Descriptor, null, null, GetAcceptFlags(Flags)).ThrowIfError());

    public bool TryAccept([NotNullWhen(true)] out UnixSocket? socket)
    {
        var flags = Flags;
        if ((flags & LinuxFileFlags.NonBlock) == 0)
            throw new InvalidOperationException("TryAccept requires a nonblocking listening socket.");

        var result = accept4(Descriptor, null, null, GetAcceptFlags(flags));
        if (result.IsError)
        {
            var error = LinuxErrorNumber.Last;
            if (error is LinuxErrorNumber.TryAgain or LinuxErrorNumber.InterruptedSystemCall)
            {
                socket = null;
                return false;
            }
            throw new LinuxException(error);
        }

        socket = new UnixSocket(result);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LinuxSocketType GetAcceptFlags(LinuxFileFlags flags)
        => (flags & LinuxFileFlags.NonBlock) != 0
            ? LinuxSocketType.CloseOnExec | LinuxSocketType.NonBlocking
            : LinuxSocketType.CloseOnExec;
}
