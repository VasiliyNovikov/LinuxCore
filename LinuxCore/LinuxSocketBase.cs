using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.Socket;

namespace LinuxCore;

public abstract unsafe class LinuxSocketBase(FileDescriptor descriptor, bool ownsDescriptor = true) : FileObject(descriptor, ownsDescriptor)
{
    protected LinuxSocketBase(LinuxAddressFamily domain, LinuxSocketType type, ProtocolType protocol)
        : this(socket(domain, type | LinuxSocketType.CloseOnExec, protocol).ThrowIfError())
    {
    }

    protected void Bind<TAddress>(in TAddress address) where TAddress : unmanaged => Bind(in address, (uint)sizeof(TAddress));

    protected void Bind<TAddress>(in TAddress address, uint addressLength) where TAddress : unmanaged
    {
        fixed (TAddress* addressPtr = &address)
            bind(Descriptor, (sockaddr*)addressPtr, addressLength).ThrowIfError();
    }

    protected void GetAddress<TAddress>(out TAddress address) where TAddress : unmanaged => GetAddress(out address, out _);

    protected void GetAddress<TAddress>(out TAddress address, out uint addressLength) where TAddress : unmanaged
    {
        addressLength = (uint)sizeof(TAddress);
        fixed (TAddress* addressPtr = &address)
            getsockname(Descriptor, (sockaddr*)addressPtr, ref addressLength).ThrowIfError();
    }

    protected void Connect<TAddress>(in TAddress address) where TAddress : unmanaged => Connect(in address, (uint)sizeof(TAddress));

    protected void Connect<TAddress>(in TAddress address, uint addressLength) where TAddress : unmanaged
    {
        fixed (TAddress* addressPtr = &address)
            connect(Descriptor, (sockaddr*)addressPtr, addressLength).ThrowIfError();
    }

    public void Listen(int backlog)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(backlog);
        listen(Descriptor, backlog).ThrowIfError();
    }

    protected FileDescriptor Accept() => accept4(Descriptor, null, null, GetAcceptFlags(Flags)).ThrowIfError();

    protected bool TryAccept(out FileDescriptor descriptor)
    {
        var flags = Flags;
        if ((flags & LinuxFileFlags.NonBlock) == 0)
            throw new InvalidOperationException("TryAccept requires a nonblocking listening socket.");

        return TryComplete(accept4(Descriptor, null, null, GetAcceptFlags(flags)), out descriptor);
    }

    private static LinuxSocketType GetAcceptFlags(LinuxFileFlags flags) => (LinuxSocketType)(flags & LinuxFileFlags.NonBlock) | LinuxSocketType.CloseOnExec;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Send(ReadOnlySpan<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        fixed (byte* bufferPtr = buffer)
            return (int)send(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int SendTo<TAddress>(in TAddress address, ReadOnlySpan<byte> buffer, LinuxSocketMessageFlags flags = default)
        where TAddress : unmanaged
    {
        return SendTo(in address, (uint)sizeof(TAddress), buffer, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int SendTo<TAddress>(in TAddress address, uint addressLength, ReadOnlySpan<byte> buffer, LinuxSocketMessageFlags flags = default)
        where TAddress : unmanaged
    {
        fixed (TAddress* addressPtr = &address)
        fixed (byte* bufferPtr = buffer)
            return (int)sendto(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags, (sockaddr*)addressPtr, addressLength).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Receive(Span<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        fixed (byte* bufferPtr = buffer)
            return (int)recv(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int ReceiveFrom<TAddress>(out TAddress address, Span<byte> buffer, LinuxSocketMessageFlags flags = default)
        where TAddress : unmanaged
    {
        return ReceiveFrom(out address, out _, buffer, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int ReceiveFrom<TAddress>(out TAddress address, out uint addressLength, Span<byte> buffer, LinuxSocketMessageFlags flags = default)
        where TAddress : unmanaged
    {
        addressLength = (uint)sizeof(TAddress);
        fixed (TAddress* addressPtr = &address)
        fixed (byte* bufferPtr = buffer)
            return (int)recvfrom(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags, (sockaddr*)addressPtr, ref addressLength).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySend(ReadOnlySpan<byte> buffer, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        var effectiveFlags = flags | LinuxSocketMessageFlags.DontWait;
        fixed (byte* bufferPtr = buffer)
            return TryComplete(send_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)effectiveFlags), out sentCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TrySendTo<TAddress>(in TAddress address, ReadOnlySpan<byte> buffer, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
        where TAddress : unmanaged
    {
        return TrySendTo(in address, (uint)sizeof(TAddress), buffer, out sentCount, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TrySendTo<TAddress>(in TAddress address, uint addressLength, ReadOnlySpan<byte> buffer, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
        where TAddress : unmanaged
    {
        var effectiveFlags = flags | LinuxSocketMessageFlags.DontWait;
        fixed (TAddress* addressPtr = &address)
        fixed (byte* bufferPtr = buffer)
            return TryComplete(sendto_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)effectiveFlags, (sockaddr*)addressPtr, addressLength), out sentCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReceive(Span<byte> buffer, out nuint receivedCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        var effectiveFlags = flags | LinuxSocketMessageFlags.DontWait;
        fixed (byte* bufferPtr = buffer)
            return TryComplete(recv_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)effectiveFlags), out receivedCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TryReceiveFrom<TAddress>(out TAddress address, Span<byte> buffer, out nuint receivedCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
        where TAddress : unmanaged
    {
        return TryReceiveFrom(out address, out _, buffer, out receivedCount, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TryReceiveFrom<TAddress>(out TAddress address, out uint addressLength, Span<byte> buffer, out nuint receivedCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
        where TAddress : unmanaged
    {
        addressLength = (uint)sizeof(TAddress);
        var effectiveFlags = flags | LinuxSocketMessageFlags.DontWait;
        fixed (TAddress* addressPtr = &address)
        fixed (byte* bufferPtr = buffer)
            return TryComplete(recvfrom_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)effectiveFlags, (sockaddr*)addressPtr, ref addressLength), out receivedCount);
    }

    protected T GetOption<T>(LinuxSocketOptionLevel level, int option) where T : unmanaged
    {
        var valueLength = (uint)sizeof(T);
        T value = default;
        getsockopt(Descriptor, level, option, &value, ref valueLength).ThrowIfError();
        return value;
    }

    protected void SetOption<T>(LinuxSocketOptionLevel level, int option, T value) where T : unmanaged
    {
        setsockopt(Descriptor, level, option, &value, (uint)sizeof(T)).ThrowIfError();
    }

    protected ReadOnlySpan<byte> GetOption(LinuxSocketOptionLevel level, int option, Span<byte> buffer)
    {
        var valueLength = (uint)buffer.Length;
        fixed (byte* valuePtr = buffer)
            getsockopt(Descriptor, level, option, valuePtr, ref valueLength).ThrowIfError();
        return buffer[..(int)valueLength];
    }

    protected void SetOption(LinuxSocketOptionLevel level, int option, ReadOnlySpan<byte> value)
    {
        fixed (byte* valuePtr = value)
            setsockopt(Descriptor, level, option, valuePtr, (uint)value.Length).ThrowIfError();
    }
}