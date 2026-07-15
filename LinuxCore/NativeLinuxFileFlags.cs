using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore;

internal static class NativeLinuxFileFlags
{
    private const int StableDirect = 0x004000;
    private const int StableLargeFile = 0x008000;
    private const int StableDirectory = 0x010000;
    private const int StableNoFollow = 0x020000;
    private const int StableFileMask = StableDirect | StableLargeFile | StableDirectory | StableNoFollow;

    private static bool IsIdentityFileArchitecture
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => RuntimeInformation.ProcessArchitecture is Architecture.X86 or Architecture.X64 or Architecture.S390x or Architecture.LoongArch64 or Architecture.RiscV64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToNative(LinuxFileFlags flags)
    {
        if (IsIdentityFileArchitecture)
            return (int)flags;

        var value = (int)flags;
        GetFileFlags(out var direct, out var largeFile, out var directory, out var noFollow);
        var result = value & ~StableFileMask;
        if ((value & StableDirect) != 0)
            result |= direct;
        if ((value & StableLargeFile) != 0)
            result |= largeFile;
        if ((value & StableDirectory) != 0)
            result |= directory;
        if ((value & StableNoFollow) != 0)
            result |= noFollow;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxFileFlags FromNative(int flags)
    {
        if (IsIdentityFileArchitecture)
            return (LinuxFileFlags)flags;

        GetFileFlags(out var direct, out var largeFile, out var directory, out var noFollow);
        var nativeMask = direct | largeFile | directory | noFollow;
        var result = flags & ~nativeMask;
        if ((flags & direct) != 0)
            result |= StableDirect;
        if ((flags & largeFile) != 0)
            result |= StableLargeFile;
        if ((flags & directory) != 0)
            result |= StableDirectory;
        if ((flags & noFollow) != 0)
            result |= StableNoFollow;
        return (LinuxFileFlags)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetFileFlags(out int direct, out int largeFile, out int directory, out int noFollow)
    {
        directory = 0x004000;
        noFollow = 0x008000;
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.Arm:
            case Architecture.Armv6:
            case Architecture.Arm64:
                direct = 0x010000;
                largeFile = 0x020000;
                break;
            case Architecture.Ppc64le:
                direct = 0x020000;
                largeFile = 0x010000;
                break;
            default:
                throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}");
        }
    }
}