using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore;

internal static class NativeLinuxMemoryMapFlags
{
    private const int StableMapLocked = 0x2000;
    private const int StableMapNoReserve = 0x4000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToNative(LinuxMemoryMapFlags flags)
    {
        var value = (int)flags;
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
            case Architecture.X64:
            case Architecture.Arm:
            case Architecture.Armv6:
            case Architecture.Arm64:
            case Architecture.S390x:
            case Architecture.LoongArch64:
            case Architecture.RiscV64:
                return value;
            case Architecture.Ppc64le:
            {
                var result = value & ~(StableMapLocked | StableMapNoReserve);
                if ((value & StableMapLocked) != 0)
                    result |= 0x80;
                if ((value & StableMapNoReserve) != 0)
                    result |= 0x40;
                return result;
            }
            default:
                throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}");
        }
    }
}