using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static LinuxCore.Interop.Time;

namespace LinuxCore.Interop;

internal static unsafe partial class File
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct stat
    {
        public readonly ulong st_dev;
        public readonly ulong st_ino;
        public readonly ulong st_nlink;
        public readonly uint st_mode;
        public readonly uint st_uid;
        public readonly uint st_gid;
        public readonly uint __pad0;
        public readonly ulong st_rdev;
        public readonly long st_size;
        public readonly long st_blksize;
        public readonly long st_blocks;
        public readonly timespec st_atim;
        public readonly timespec st_mtim;
        public readonly timespec st_ctim;
        public readonly long __glibc_reserved0;
        public readonly long __glibc_reserved1;
        public readonly long __glibc_reserved2;
    }

    // int close(int fd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "close")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult close(FileDescriptor fd);

    // int open(const char *pathname, int flags, mode_t mode);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "open", StringMarshalling = StringMarshalling.Utf8)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<FileDescriptor> open(string path, LinuxFileFlags flags, LinuxFileMode mode);

    // ssize_t read(int fd, void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "read")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> read(FileDescriptor fd, void* buf, nuint count);

    // ssize_t write(int fd, const void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "write")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> write(FileDescriptor fd, void* buf, nuint count);

    // int ioctl(int fd, unsigned long operation, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "ioctl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult ioctl(FileDescriptor fd, ulong operation, void* argp);

    // int fstat(int fd, struct stat *statbuf);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fstat")]
    public static partial LinuxResult fstat(FileDescriptor fd, out stat statbuf);
}