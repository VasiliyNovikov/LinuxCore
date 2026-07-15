using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

// long syscall(long number, ...);
internal static partial class SysCall
{
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number, nint arg1);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number, nint arg1, nint arg2);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number, nint arg1, nint arg2, nint arg3);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number, nint arg1, nint arg2, nint arg3, nint arg4);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint syscall(nint number, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5, nint arg6);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number, nint arg1);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number, nint arg1, nint arg2);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number, nint arg1, nint arg2, nint arg3);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number, nint arg1, nint arg2, nint arg3, nint arg4);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);

    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial nint syscall_noblock(nint number, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5, nint arg6);
}