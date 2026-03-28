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

    protected FileDescriptor Accept() => AcceptCore(Flags).ThrowIfError();

    protected bool TryAccept(out FileDescriptor descriptor)
    {
        var flags = Flags;
        return (flags & LinuxFileFlags.NonBlock) == 0
            ? throw new InvalidOperationException("TryAccept requires a nonblocking listening socket.")
            : TryComplete(AcceptCore(flags), out descriptor);
    }

    private LinuxResult<FileDescriptor> AcceptCore(LinuxFileFlags flags) => accept4(Descriptor, null, null, (LinuxSocketType)(flags & LinuxFileFlags.NonBlock) | LinuxSocketType.CloseOnExec);

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
    public int SendMessage<T>(ReadOnlySpan<byte> buffer, LinuxSocketOptionLevel cmsgLevel, LinuxControlMessageType cmsgType, ReadOnlySpan<T> cmsgData, LinuxSocketMessageFlags flags = default)
        where T : unmanaged
    {
        var controlLen = CmsgSpace(cmsgData);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildSendMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov, cmsgLevel, cmsgType, cmsgData);
            return (int)sendmsg(Descriptor, &msg, (int)flags).ThrowIfError();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public bool TrySendMessage<T>(ReadOnlySpan<byte> buffer, LinuxSocketOptionLevel cmsgLevel, LinuxControlMessageType cmsgType, ReadOnlySpan<T> cmsgData, out nuint sentCount, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
        where T : unmanaged
    {
        flags |= LinuxSocketMessageFlags.DontWait;
        var controlLen = CmsgSpace(cmsgData);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildSendMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov, cmsgLevel, cmsgType, cmsgData);
            return TryComplete(sendmsg_noblock(Descriptor, &msg, (int)flags), out sentCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public int ReceiveMessage<T>(Span<byte> buffer, LinuxSocketOptionLevel cmsgLevel, LinuxControlMessageType cmsgType, Span<T> cmsgData, out int cmsgDataCount, out LinuxSocketMessageFlags messageFlags, LinuxSocketMessageFlags flags = default)
        where T : unmanaged
    {
        var controlLen = CmsgSpace(cmsgData);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov);
            var result = (int)recvmsg(Descriptor, &msg, (int)flags).ThrowIfError();
            messageFlags = (LinuxSocketMessageFlags)msg.msg_flags;
            cmsgDataCount = ParseControlMessage(cmsgLevel, cmsgType, &msg, cmsgData);
            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public bool TryReceiveMessage<T>(Span<byte> buffer, LinuxSocketOptionLevel cmsgLevel, LinuxControlMessageType cmsgType, Span<T> cmsgData, out nuint receivedCount, out int cmsgDataCount, out LinuxSocketMessageFlags messageFlags, LinuxSocketMessageFlags flags = LinuxSocketMessageFlags.DontWait)
        where T : unmanaged
    {
        flags |= LinuxSocketMessageFlags.DontWait;
        var controlLen = CmsgSpace(cmsgData);
        var controlPtr = stackalloc byte[controlLen];
        fixed (byte* bufferPtr = buffer)
        {
            msghdr msg;
            iovec iov;
            BuildMessageHeader(bufferPtr, (nuint)buffer.Length, controlPtr, (nuint)controlLen, &msg, &iov);
            if (TryComplete(recvmsg_noblock(Descriptor, &msg, (int)flags), out receivedCount))
            {
                messageFlags = (LinuxSocketMessageFlags)msg.msg_flags;
                cmsgDataCount = ParseControlMessage(cmsgLevel, cmsgType, &msg, cmsgData);
                return true;
            }
            cmsgDataCount = 0;
            messageFlags = default;
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
    private static void BuildSendMessageHeader<T>(byte* bufferPtr, nuint bufferLen, byte* controlPtr, nuint controlLen, msghdr* msg, iovec* iov, LinuxSocketOptionLevel cmsgLevel, LinuxControlMessageType cmsgType, ReadOnlySpan<T> cmsgData)
        where T : unmanaged
    {
        var cmsg = (cmsghdr*)controlPtr;
        cmsg->cmsg_len = (nuint)(sizeof(cmsghdr) + cmsgData.Length * sizeof(T));
        cmsg->cmsg_level = (int)cmsgLevel;
        cmsg->cmsg_type = (int)cmsgType;
        cmsgData.CopyTo(new Span<T>(CmsgData(cmsg), cmsgData.Length));
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
    private static int ParseControlMessage<T>(LinuxSocketOptionLevel cmsgLevel, LinuxControlMessageType cmsgType, msghdr* msg, Span<T> cmsgData)
        where T : unmanaged
    {
        for (var cmsg = CmsgFirstHeader(msg); cmsg != null; cmsg = CmsgNextHeader(msg, cmsg))
        {
            if (cmsg->cmsg_level == (int)cmsgLevel && cmsg->cmsg_type == (int)cmsgType)
            {
                var dataLen = (int)cmsg->cmsg_len - sizeof(cmsghdr);
                if (dataLen > 0)
                {
                    var count = Math.Min(dataLen / sizeof(T), cmsgData.Length);
                    new ReadOnlySpan<T>(CmsgData(cmsg), count).CopyTo(cmsgData);
                    return count;
                }
                break;
            }
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CmsgAlign(int len) => (len + (sizeof(nuint) - 1)) & ~(sizeof(nuint) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CmsgSpace<T>(ReadOnlySpan<T> cmsgData) where T : unmanaged => sizeof(cmsghdr) + CmsgAlign(cmsgData.Length * sizeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* CmsgData(cmsghdr* cmsg) => (byte*)cmsg + sizeof(cmsghdr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static cmsghdr* CmsgFirstHeader(msghdr* msg) => msg->msg_controllen >= (nuint)sizeof(cmsghdr) ? (cmsghdr*)msg->msg_control : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static cmsghdr* CmsgNextHeader(msghdr* msg, cmsghdr* cmsg)
    {
        var next = (cmsghdr*)((byte*)cmsg + CmsgAlign((int)cmsg->cmsg_len));
        var end = (byte*)msg->msg_control + msg->msg_controllen;
        return (byte*)next + sizeof(cmsghdr) > end ? null : next;
    }
}