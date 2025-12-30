using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using LinuxCore.Interop;

namespace LinuxCore;

public abstract unsafe class LinuxSocketBase(LinuxAddressFamily domain, LinuxSocketType type, ProtocolType protocol)
    : FileObject(LibC.socket(domain, type | LinuxSocketType.CloseOnExec, protocol).ThrowIfError())
{
    protected void Bind<TAddress>(in TAddress address) where TAddress : unmanaged
    {
        fixed (TAddress* addressPtr = &address)
            LibC.bind(Descriptor, (LibC.sockaddr*)addressPtr, (uint)sizeof(TAddress)).ThrowIfError();
    }

    protected void GetAddress<TAddress>(out TAddress address) where TAddress : unmanaged
    {
        var addressLength = (uint)sizeof(TAddress);
        fixed (TAddress* addressPtr = &address)
            LibC.getsockname(Descriptor, (LibC.sockaddr*)addressPtr, ref addressLength).ThrowIfError();
    }

    protected void Connect<TAddress>(in TAddress address) where TAddress : unmanaged
    {
        fixed (TAddress* addressPtr = &address)
            LibC.connect(Descriptor, (LibC.sockaddr*)addressPtr, (uint)sizeof(TAddress)).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Send(ReadOnlySpan<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        fixed (byte* bufferPtr = buffer)
            return (int)LibC.recv(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags).ThrowIfError();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Receive(Span<byte> buffer, LinuxSocketMessageFlags flags = default)
    {
        fixed (byte* bufferPtr = buffer)
            return (int)LibC.recv(Descriptor, bufferPtr, (uint)buffer.Length, (int)flags).ThrowIfError();
    }

    protected T GetOption<T>(LinuxSocketOptionLevel level, int option) where T : unmanaged
    {
        var valueLength = (uint)sizeof(T);
        T value = default;
        LibC.getsockopt(Descriptor, level, option, &value, ref valueLength).ThrowIfError();
        return value;
    }

    protected void SetOption<T>(LinuxSocketOptionLevel level, int option, T value) where T : unmanaged
    {
        LibC.setsockopt(Descriptor, level, option, &value, (uint)sizeof(T)).ThrowIfError();
    }

    protected ReadOnlySpan<byte> GetOption(LinuxSocketOptionLevel level, int option, Span<byte> buffer)
    {
        var valueLength = (uint)buffer.Length;
        fixed (byte* valuePtr = buffer)
            LibC.getsockopt(Descriptor, level, option, valuePtr, ref valueLength).ThrowIfError();
        return buffer[..(int)valueLength];
    }

    protected void SetOption(LinuxSocketOptionLevel level, int option, ReadOnlySpan<byte> value)
    {
        fixed (byte* valuePtr = value)
            LibC.setsockopt(Descriptor, level, option, valuePtr, (uint)value.Length).ThrowIfError();
    }
}