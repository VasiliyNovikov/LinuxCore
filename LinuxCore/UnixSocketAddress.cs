using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using SocketInterop = LinuxCore.Interop.Socket;

namespace LinuxCore;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct UnixSocketAddress : IEquatable<UnixSocketAddress>
{
    public const byte MaxPathLength = SocketInterop.SOCKADDR_UN_PATH_LENGTH;
    public const byte MaxAbstractNameLength = MaxPathLength - 1;

    public static readonly UnixSocketAddress Unnamed;


    private readonly UnixSocketAddressKind _kind;
    private readonly byte _length;
    private fixed byte _path[MaxPathLength];
    
    public UnixSocketAddressKind Kind => _kind;

    public readonly int Length => _length;

    public readonly ReadOnlySpan<byte> PathBytes => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _path[0]), _length);

    public readonly string Path => Encoding.UTF8.GetString(PathBytes);

    private UnixSocketAddress(UnixSocketAddressKind kind, ReadOnlySpan<byte> path)
    {
        _kind = kind;
        _length = checked((byte)path.Length);
        path.CopyTo(MemoryMarshal.CreateSpan(ref _path[0], MaxPathLength));
    }

    [SkipLocalsInit]
    public static UnixSocketAddress FromPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        Span<byte> pathBytes = stackalloc byte[Encoding.UTF8.GetMaxByteCount(path.Length)];
        pathBytes = pathBytes[..Encoding.UTF8.GetBytes(path, pathBytes)];
        return FromPath(pathBytes);
    }

    public static UnixSocketAddress FromPath(ReadOnlySpan<byte> path)
    {
        if (path.IsEmpty)
            throw new ArgumentException("Pathname Unix socket addresses cannot be empty.", nameof(path));
        if (path.Contains((byte)0))
            throw new ArgumentException("Pathname Unix socket addresses cannot contain embedded NULL bytes.", nameof(path));
        if (path.Length > MaxPathLength)
            throw new ArgumentOutOfRangeException(nameof(path), $"Pathname Unix socket path must be at most {MaxPathLength} bytes.");
        return new(UnixSocketAddressKind.PathName, path);
    }

    [SkipLocalsInit]
    public static UnixSocketAddress FromAbstractName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Span<byte> nameBytes = stackalloc byte[Encoding.UTF8.GetMaxByteCount(name.Length)];
        nameBytes = nameBytes[..Encoding.UTF8.GetBytes(name, nameBytes)];
        return FromAbstractName(nameBytes);
    }

    public static UnixSocketAddress FromAbstractName(ReadOnlySpan<byte> name)
    {
        if (name.Length > MaxAbstractNameLength)
            throw new ArgumentOutOfRangeException(nameof(name), $"Abstract Unix socket name must be at most {MaxAbstractNameLength} bytes.");
        return new(UnixSocketAddressKind.Abstract, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(UnixSocketAddress other) => _kind == other._kind && _length == other._length && PathBytes.SequenceEqual(other.PathBytes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj) => obj is UnixSocketAddress other && Equals(other);

    public readonly override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_kind);
        hash.Add(_length);
        hash.AddBytes(PathBytes);
        return hash.ToHashCode();
    }

    public readonly override string ToString() =>
        _kind switch
        {
            UnixSocketAddressKind.Unnamed => "(unnamed)",
            UnixSocketAddressKind.Abstract => $"@{Path}",
            _ => Path
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UnixSocketAddress left, UnixSocketAddress right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UnixSocketAddress left, UnixSocketAddress right) => !left.Equals(right);

    internal readonly void WriteTo(out SocketInterop.sockaddr_un address, out uint addressLength)
    {
        address.sun_family = (ushort)LinuxAddressFamily.Unix;

        var nativePath = MemoryMarshal.CreateSpan(ref address.sun_path[0], SocketInterop.SOCKADDR_UN_PATH_LENGTH);
        uint nativePathLength;
        switch (_kind)
        {
            case UnixSocketAddressKind.Unnamed:
                nativePathLength = 0;
                break;
            case UnixSocketAddressKind.Abstract:
                nativePath[0] = 0;
                PathBytes.CopyTo(nativePath[1..]);
                nativePathLength = _length + 1u;
                break;
            default:
                PathBytes.CopyTo(nativePath);
                if (_length < MaxPathLength)
                {
                    nativePath[_length] = 0;
                    nativePathLength = _length + 1u;
                }
                else
                    nativePathLength = MaxPathLength;
                break;
        }
        addressLength = SocketInterop.SOCKADDR_UN_PATH_OFFSET + nativePathLength;
    }

    internal static UnixSocketAddress FromNative(SocketInterop.sockaddr_un address, uint addressLength)
    {
        if (addressLength >= SocketInterop.SOCKADDR_UN_PATH_OFFSET && address.sun_family != (ushort)LinuxAddressFamily.Unix)
            throw new InvalidOperationException($"Expected AF_UNIX but received family {(LinuxAddressFamily)address.sun_family}.");

        if (addressLength <= SocketInterop.SOCKADDR_UN_PATH_OFFSET)
            return Unnamed;

        var nativeLength = (int)Math.Min(addressLength - SocketInterop.SOCKADDR_UN_PATH_OFFSET, SocketInterop.SOCKADDR_UN_PATH_LENGTH);
        var nativePath = MemoryMarshal.CreateReadOnlySpan(ref address.sun_path[0], nativeLength);

        if (nativePath[0] == 0)
            return new(UnixSocketAddressKind.Abstract, nativePath[1..]);

        var terminatorIndex = nativePath.IndexOf((byte)0);
        var path = terminatorIndex >= 0 ? nativePath[..terminatorIndex] : nativePath;
        return path.IsEmpty
            ? throw new InvalidOperationException("Pathname Unix socket addresses cannot be empty.")
            : new(UnixSocketAddressKind.PathName, path);
    }
}