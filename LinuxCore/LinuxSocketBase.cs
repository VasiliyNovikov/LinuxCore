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

    protected FileDescriptor Accept() => AcceptCore().ThrowIfError();

    protected bool TryAccept(out FileDescriptor descriptor)
    {
        return IsNonBlocking
            ? TryComplete(AcceptCore(), out descriptor)
            : throw new InvalidOperationException("TryAccept requires a non-blocking listening socket.");
    }

    private LinuxResult<FileDescriptor> AcceptCore() => accept4(Descriptor, null, null, (IsNonBlocking ? LinuxSocketType.NonBlocking : 0) | LinuxSocketType.CloseOnExec);

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
        flags |= LinuxSocketMessageFlags.DontWait;
        fixed (byte* bufferPtr = buffer)
            return TryComplete(send_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags), out sentCount);
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
        flags |= LinuxSocketMessageFlags.DontWait;
        fixed (TAddress* addressPtr = &address)
        fixed (byte* bufferPtr = buffer)
            return TryComplete(sendto_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags, (sockaddr*)addressPtr, addressLength), out sentCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReceive(Span<byte> buffer, out nuint receivedCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        flags |= LinuxSocketMessageFlags.DontWait;
        fixed (byte* bufferPtr = buffer)
            return TryComplete(recv_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags), out receivedCount);
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
        flags |= LinuxSocketMessageFlags.DontWait;
        addressLength = (uint)sizeof(TAddress);
        fixed (TAddress* addressPtr = &address)
        fixed (byte* bufferPtr = buffer)
            return TryComplete(recvfrom_noblock(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags, (sockaddr*)addressPtr, ref addressLength), out receivedCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public int SendMessage(ReadOnlySpan<byte> buffer, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, ReadOnlySpan<byte> controlBuffer, LinuxSocketMessageFlags flags = default)
    {
        var controlLen = ControlMessageSpace(controlBuffer);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildSendMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov, controlMessageLevel, controlMessageType, controlBuffer);
            return (int)sendmsg(Descriptor, &msg, (int)flags).ThrowIfError();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public bool TrySendMessage(ReadOnlySpan<byte> buffer, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, ReadOnlySpan<byte> controlBuffer, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        flags |= LinuxSocketMessageFlags.DontWait;
        var controlLen = ControlMessageSpace(controlBuffer);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildSendMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov, controlMessageLevel, controlMessageType, controlBuffer);
            return TryComplete(sendmsg_noblock(Descriptor, &msg, (int)flags), out sentCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public int ReceiveMessage(Span<byte> buffer, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, Span<byte> controlBuffer, out int receivedControlCount, out LinuxSocketMessageFlags receivedMessageFlags, LinuxSocketMessageFlags flags = default)
    {
        var controlLen = ControlMessageSpace(controlBuffer);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov);
            var result = (int)recvmsg(Descriptor, &msg, (int)flags).ThrowIfError();
            receivedMessageFlags = (LinuxSocketMessageFlags)msg.msg_flags;
            receivedControlCount = ParseControlMessage(controlMessageLevel, controlMessageType, &msg, controlBuffer);
            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public bool TryReceiveMessage(Span<byte> buffer, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, Span<byte> controlBuffer, out nuint receivedCount, out int receivedControlCount, out LinuxSocketMessageFlags receivedMessageFlags, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
    {
        flags |= LinuxSocketMessageFlags.DontWait;
        var controlLen = ControlMessageSpace(controlBuffer);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov);
            if (TryComplete(recvmsg_noblock(Descriptor, &msg, (int)flags), out receivedCount))
            {
                receivedMessageFlags = (LinuxSocketMessageFlags)msg.msg_flags;
                receivedControlCount = ParseControlMessage(controlMessageLevel, controlMessageType, &msg, controlBuffer);
                return true;
            }
            receivedControlCount = 0;
            receivedMessageFlags = default;
            return false;
        }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildSendMessageHeader(byte* bufferPtr, nuint bufferLen, byte* controlPtr, nuint controlLen, msghdr* msg, iovec* iov, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, ReadOnlySpan<byte> controlBuffer)
    {
        var cmsg = (cmsghdr*)controlPtr;
        cmsg->cmsg_len = (nuint)(sizeof(cmsghdr) + controlBuffer.Length);
        cmsg->cmsg_level = (int)controlMessageLevel;
        cmsg->cmsg_type = (int)controlMessageType;
        controlBuffer.CopyTo(new Span<byte>(ControlMessageData(cmsg), controlBuffer.Length));
        BuildMessageHeader(bufferPtr, bufferLen, controlPtr, controlLen, msg, iov);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildMessageHeader(byte* bufferPtr, nuint bufferLen, byte* controlPtr, nuint controlLen, msghdr* msg, iovec* iov)
    {
        *iov = new iovec { iov_base = bufferPtr, iov_len = bufferLen };
        *msg = new msghdr
        {
            msg_iov = iov,
            msg_iovlen = 1,
            msg_control = controlPtr,
            msg_controllen = controlLen
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ParseControlMessage(LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, msghdr* msg, Span<byte> controlBuffer)
    {
        for (var cmsg = ControlMessageFirst(msg); cmsg != null; cmsg = ControlMessageNext(msg, cmsg))
        {
            if (cmsg->cmsg_level != (int)controlMessageLevel || cmsg->cmsg_type != (int)controlMessageType)
                continue;

            var dataLen = (int)cmsg->cmsg_len - sizeof(cmsghdr);
            if (dataLen > 0)
            {
                var count = Math.Min(dataLen, controlBuffer.Length);
                new ReadOnlySpan<byte>(ControlMessageData(cmsg), count).CopyTo(controlBuffer);
                return count;
            }
            break;
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ControlMessageAlign(int len) => (len + (sizeof(nuint) - 1)) & ~(sizeof(nuint) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ControlMessageSpace(ReadOnlySpan<byte> cmsgData) => sizeof(cmsghdr) + ControlMessageAlign(cmsgData.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* ControlMessageData(cmsghdr* cmsg) => (byte*)cmsg + sizeof(cmsghdr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static cmsghdr* ControlMessageFirst(msghdr* msg) => msg->msg_controllen >= (nuint)sizeof(cmsghdr) ? (cmsghdr*)msg->msg_control : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static cmsghdr* ControlMessageNext(msghdr* msg, cmsghdr* cmsg)
    {
        var next = (cmsghdr*)((byte*)cmsg + ControlMessageAlign((int)cmsg->cmsg_len));
        var end = (byte*)msg->msg_control + msg->msg_controllen;
        return (byte*)next + sizeof(cmsghdr) > end ? null : next;
    }
}