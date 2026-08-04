using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LinuxCore.Interop;

using static LinuxCore.Interop.Socket;

namespace LinuxCore;

public sealed unsafe class UnixSocket : LinuxSocketBase
{
    private const int ScmRightsControlBufferMaxLength = 4 * 1024;

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
    [SkipLocalsInit]
    private protected override int ReceiveMessageCore(Span<byte> buffer, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, Span<byte> controlBuffer, out int receivedControlCount, out LinuxSocketMessageFlags receivedMessageFlags, LinuxSocketMessageFlags flags)
    {
        if (!ShouldUseScmRightsWorkaround(controlMessageLevel, controlMessageType))
            return base.ReceiveMessageCore(buffer, controlMessageLevel, controlMessageType, controlBuffer, out receivedControlCount, out receivedMessageFlags, flags);

        Span<byte> allDescriptors = stackalloc byte[ScmRightsControlBufferMaxLength];
        var result = base.ReceiveMessageCore(buffer, controlMessageLevel, controlMessageType, allDescriptors, out var allDescriptorBytesCount, out receivedMessageFlags, flags);
        receivedControlCount = CopyDescriptorsAndCloseExcess(allDescriptors, allDescriptorBytesCount, controlBuffer, ref receivedMessageFlags);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    private protected override bool TryReceiveMessageCore(Span<byte> buffer, LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType, Span<byte> controlBuffer, out nuint receivedCount, out int receivedControlCount, out LinuxSocketMessageFlags receivedMessageFlags, LinuxSocketMessageFlags flags)
    {
        flags |= LinuxSocketMessageFlags.DontWait;
        if (!ShouldUseScmRightsWorkaround(controlMessageLevel, controlMessageType))
            return base.TryReceiveMessageCore(buffer, controlMessageLevel, controlMessageType, controlBuffer, out receivedCount, out receivedControlCount, out receivedMessageFlags, flags);

        Span<byte> allDescriptors = stackalloc byte[ScmRightsControlBufferMaxLength];
        if (base.TryReceiveMessageCore(buffer, controlMessageLevel, controlMessageType, allDescriptors, out receivedCount, out var allDescriptorBytesCount, out receivedMessageFlags, flags))
        {
            receivedControlCount = CopyDescriptorsAndCloseExcess(allDescriptors, allDescriptorBytesCount, controlBuffer, ref receivedMessageFlags);
            return true;
        }
        receivedControlCount = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CopyDescriptorsAndCloseExcess(ReadOnlySpan<byte> source, int sourceLength, Span<byte> destination, ref LinuxSocketMessageFlags messageFlags)
    {
        var completeSourceLength = sourceLength / sizeof(FileDescriptor) * sizeof(FileDescriptor);
        var sourceDescriptors = MemoryMarshal.Cast<byte, FileDescriptor>(source[..completeSourceLength]);
        int copiedCount;
        if (completeSourceLength == sourceLength)
        {
            var destinationDescriptors = MemoryMarshal.Cast<byte, FileDescriptor>(destination[..(destination.Length / sizeof(FileDescriptor) * sizeof(FileDescriptor))]);
            copiedCount = Math.Min(sourceDescriptors.Length, destinationDescriptors.Length);
            sourceDescriptors[..copiedCount].CopyTo(destinationDescriptors);
        }
        else
            copiedCount = 0;
        foreach (var descriptor in sourceDescriptors[copiedCount..])
            descriptor.Close();
        if (copiedCount < sourceDescriptors.Length)
            messageFlags |= LinuxSocketMessageFlags.ControlTruncated;
        return copiedCount * sizeof(FileDescriptor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldUseScmRightsWorkaround(LinuxSocketOptionLevel controlMessageLevel, LinuxControlMessageType controlMessageType)
    {
        return controlMessageLevel == LinuxSocketOptionLevel.Socket
            && controlMessageType == LinuxControlMessageType.ScmRights
            && NativeAbi.IsLikelyQemuLinuxUser;
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