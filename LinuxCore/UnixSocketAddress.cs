using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using SocketInterop = LinuxCore.Interop.Socket;

namespace LinuxCore;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct UnixSocketAddress : IEquatable<UnixSocketAddress>
{
    public const int MaxPayloadLength = 107;

    private const int MaxStoredLength = 108;
    private const byte PathnameKind = 1;
    private const byte AbstractKind = 2;

    private byte _kind;
    private byte _length;
    private fixed byte _payload[MaxStoredLength];

    public static UnixSocketAddress Unnamed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    public bool IsUnnamed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _kind == 0;
    }

    public bool IsPathname
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _kind == PathnameKind;
    }

    public bool IsAbstract
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _kind == AbstractKind;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }

    public static UnixSocketAddress FromPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return FromPath(Encoding.UTF8.GetBytes(path), nameof(path));
    }

    public static UnixSocketAddress FromPath(ReadOnlySpan<byte> path) => FromPath(path, nameof(path));

    public static UnixSocketAddress FromAbstractName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return FromAbstractName(Encoding.UTF8.GetBytes(name), nameof(name));
    }

    public static UnixSocketAddress FromAbstractName(ReadOnlySpan<byte> name) => FromAbstractName(name, nameof(name));

    public int CopyNameTo(Span<byte> destination)
    {
        if (destination.Length < _length)
            throw new ArgumentException("Destination is too short.", nameof(destination));

        GetPayloadSpan().CopyTo(destination);
        return _length;
    }

    public byte[] ToArray()
    {
        var buffer = new byte[_length];
        CopyNameTo(buffer);
        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToUtf8String() => Encoding.UTF8.GetString(ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UnixSocketAddress other)
    {
        if (_kind != other._kind || _length != other._length)
            return false;

        return GetPayloadSpan().SequenceEqual(other.GetPayloadSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is UnixSocketAddress other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_kind);
        hash.Add(_length);

        foreach (var payloadByte in GetPayloadSpan())
            hash.Add(payloadByte);

        return hash.ToHashCode();
    }

    public override string ToString() => IsUnnamed
        ? "unnamed"
        : $"{(IsPathname ? "pathname" : "abstract")}:{Convert.ToHexString(ToArray())}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UnixSocketAddress left, UnixSocketAddress right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UnixSocketAddress left, UnixSocketAddress right) => !left.Equals(right);

    internal void WriteTo(out SocketInterop.sockaddr_un address, out uint addressLength)
    {
        address = default;
        address.sun_family = (ushort)LinuxAddressFamily.Unix;

        var nativePath = MemoryMarshal.CreateSpan(ref address.sun_path[0], SocketInterop.SOCKADDR_UN_PATH_LENGTH);
        nativePath.Clear();

        if (IsUnnamed)
        {
            addressLength = SocketInterop.SOCKADDR_UN_PATH_OFFSET;
            return;
        }

        var payload = GetPayloadSpan();
        if (IsPathname)
        {
            payload.CopyTo(nativePath);
            if (_length < MaxStoredLength)
            {
                nativePath[_length] = 0;
                addressLength = SocketInterop.SOCKADDR_UN_PATH_OFFSET + (uint)_length + 1;
                return;
            }

            addressLength = SocketInterop.SOCKADDR_UN_PATH_OFFSET + MaxStoredLength;
            return;
        }

        nativePath[0] = 0;
        payload.CopyTo(nativePath[1..]);
        addressLength = SocketInterop.SOCKADDR_UN_PATH_OFFSET + 1u + (uint)_length;
    }

    internal static UnixSocketAddress FromNative(SocketInterop.sockaddr_un address, uint addressLength)
    {
        if (addressLength <= SocketInterop.SOCKADDR_UN_PATH_OFFSET)
            return Unnamed;

        if (address.sun_family != (ushort)LinuxAddressFamily.Unix)
            throw new InvalidOperationException($"Expected AF_UNIX but received family {(LinuxAddressFamily)address.sun_family}.");

        var nativeLength = (int)Math.Min(addressLength - SocketInterop.SOCKADDR_UN_PATH_OFFSET, (uint)SocketInterop.SOCKADDR_UN_PATH_LENGTH);
        var nativePath = MemoryMarshal.CreateReadOnlySpan(ref address.sun_path[0], nativeLength);

        if (nativePath[0] == 0)
            return Create(AbstractKind, nativePath[1..]);

        var terminatorIndex = nativePath.IndexOf((byte)0);
        var payload = terminatorIndex >= 0 ? nativePath[..terminatorIndex] : nativePath;
        return payload.IsEmpty
            ? throw new InvalidOperationException("Pathname Unix socket addresses cannot be empty.")
            : Create(PathnameKind, payload);
    }

    private static UnixSocketAddress FromPath(ReadOnlySpan<byte> path, string paramName)
    {
        if (path.IsEmpty)
            throw new ArgumentException("Pathname Unix socket addresses cannot be empty.", paramName);

        ValidateLength(path.Length, paramName);
        if (path.Contains((byte)0))
            throw new ArgumentException("Pathname Unix socket addresses cannot contain embedded NUL bytes.", paramName);

        return Create(PathnameKind, path);
    }

    private static UnixSocketAddress FromAbstractName(ReadOnlySpan<byte> name, string paramName)
    {
        ValidateLength(name.Length, paramName);
        return Create(AbstractKind, name);
    }

    private static void ValidateLength(int length, string paramName)
    {
        if (length > MaxPayloadLength)
            throw new ArgumentOutOfRangeException(paramName, $"Unix socket payload must be at most {MaxPayloadLength} bytes.");
    }

    private static UnixSocketAddress Create(byte kind, ReadOnlySpan<byte> payload)
    {
        UnixSocketAddress address = default;
        address._kind = kind;
        address._length = checked((byte)payload.Length);

        payload.CopyTo(MemoryMarshal.CreateSpan(ref address._payload[0], MaxStoredLength));

        return address;
    }

    private ReadOnlySpan<byte> GetPayloadSpan() => MemoryMarshal.CreateReadOnlySpan(ref _payload[0], _length);
}