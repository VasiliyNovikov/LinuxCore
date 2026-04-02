using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class SysCall
{
    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<long> syscall(long number, int arg1);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<long> syscall_noblock(long number, int arg1);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<long> syscall(long number, int arg1, void* arg2);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<long> syscall_noblock(long number, int arg1, void* arg2);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<long> syscall(long number, int arg1, int arg2, void* arg3);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<long> syscall_noblock(long number, int arg1, int arg2, void* arg3);
}