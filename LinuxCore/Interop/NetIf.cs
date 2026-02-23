using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class NetIf
{
    public const int IF_NAMESIZE = 16;

    // unsigned int if_nametoindex(const char *ifname);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "if_nametoindex", StringMarshalling = StringMarshalling.Utf8)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial uint if_nametoindex(string ifname);

    // char *if_indextoname(unsigned int ifindex, char *ifname);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "if_indextoname", StringMarshalling = StringMarshalling.Utf8)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial byte* if_indextoname(uint ifindex, Span<byte> ifname);
}