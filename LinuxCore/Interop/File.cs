using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static LinuxCore.Interop.Time;

namespace LinuxCore.Interop;

internal static unsafe partial class File
{
    public const int F_GETFD     = 1; // Get file descriptor flags
    public const int F_SETFD     = 2; // Set file descriptor flags
    public const int F_GETFL     = 3; // Get file status flags

    public const int F_ADD_SEALS = 1033; // Add seals to file
    public const int F_GET_SEALS = 1034; // Get seals for file

    public const int FD_CLOEXEC = 1;

    public const int F_SEAL_SEAL         = 0x0001; // Prevent further seals from being set
    public const int F_SEAL_SHRINK       = 0x0002; // Prevent file from shrinking
    public const int F_SEAL_GROW         = 0x0004; // Prevent file from growing
    public const int F_SEAL_WRITE        = 0x0008; // Prevent writes
    public const int F_SEAL_FUTURE_WRITE = 0x0010; // Prevent future writes while mapped

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

    // int dup(int oldfd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "dup")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<FileDescriptor> dup(FileDescriptor oldfd);

    // ssize_t read(int fd, void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "read")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> read(FileDescriptor fd, void* buf, nuint count);

    // ssize_t read(int fd, void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "read")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> read_noblock(FileDescriptor fd, void* buf, nuint count);

    // ssize_t write(int fd, const void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "write")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<nuint> write(FileDescriptor fd, void* buf, nuint count);

    // ssize_t write(int fd, const void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "write")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<nuint> write_noblock(FileDescriptor fd, void* buf, nuint count);

    // int ioctl(int fd, unsigned long operation, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "ioctl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult ioctl(FileDescriptor fd, ulong operation, void* argp);

    // int fcntl(int fd, int cmd, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fcntl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<int> fcntl(FileDescriptor fd, int cmd);

    // int fcntl(int fd, int cmd, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fcntl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<int> fcntl(FileDescriptor fd, int cmd, int arg);

    private static readonly bool HasFstat = NativeLibrary.TryGetExport(NativeLibrary.Load(LinuxLibraries.LibC), "fstat", out _);

    // int fstat(int fd, struct stat *statbuf);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fstat")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial LinuxResult fstat_direct(FileDescriptor fd, out stat statbuf);

    // int __fxstat(int ver, int fd, struct stat *statbuf);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "__fxstat")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial LinuxResult __fxstat(int ver, FileDescriptor fd, out stat statbuf);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult fstat(FileDescriptor fd, out stat statbuf) => HasFstat ? fstat_direct(fd, out statbuf) : __fxstat(1, fd, out statbuf);

    // off_t lseek(int fd, off_t offset, int whence);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "lseek")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial LinuxResult<long> lseek(FileDescriptor fd, long offset, LinuxSeekOrigin whence);
}