using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class File
{
    public const int F_GETFD         = 1;    // Get file descriptor flags
    public const int F_SETFD         = 2;    // Set file descriptor flags
    public const int F_GETFL         = 3;    // Get file status flags
    public const int F_DUPFD_CLOEXEC = 1030; // Duplicate descriptor with close-on-exec

    public const int F_ADD_SEALS = 1033; // Add seals to file
    public const int F_GET_SEALS = 1034; // Get seals for file

    public const int FD_CLOEXEC = 1;

    public const int F_SEAL_SEAL         = 0x0001; // Prevent further seals from being set
    public const int F_SEAL_SHRINK       = 0x0002; // Prevent file from shrinking
    public const int F_SEAL_GROW         = 0x0004; // Prevent file from growing
    public const int F_SEAL_WRITE        = 0x0008; // Prevent writes
    public const int F_SEAL_FUTURE_WRITE = 0x0010; // Prevent future writes while mapped

    private const int  AT_EMPTY_PATH     = 0x1000;
    private const uint STATX_BASIC_STATS = 0x07ff;

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct statx_timestamp
    {
        public readonly long tv_sec;
        public readonly uint tv_nsec;
        private readonly int __reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct statx
    {
        public readonly uint stx_mask;
        public readonly uint stx_blksize;
        public readonly ulong stx_attributes;
        public readonly uint stx_nlink;
        public readonly uint stx_uid;
        public readonly uint stx_gid;
        public readonly ushort stx_mode;
        private readonly ushort __spare0;
        public readonly ulong stx_ino;
        public readonly ulong stx_size;
        public readonly ulong stx_blocks;
        public readonly ulong stx_attributes_mask;
        public readonly statx_timestamp stx_atime;
        public readonly statx_timestamp stx_btime;
        public readonly statx_timestamp stx_ctime;
        public readonly statx_timestamp stx_mtime;
        public readonly uint stx_rdev_major;
        public readonly uint stx_rdev_minor;
        public readonly uint stx_dev_major;
        public readonly uint stx_dev_minor;
        public readonly ulong stx_mnt_id;
        public readonly uint stx_dio_mem_align;
        public readonly uint stx_dio_offset_align;
        public readonly ulong stx_subvol;
        public readonly uint stx_atomic_write_unit_min;
        public readonly uint stx_atomic_write_unit_max;
        public readonly uint stx_atomic_write_segments_max;
        public readonly uint stx_dio_read_offset_align;
        public readonly uint stx_atomic_write_unit_max_opt;
        private readonly uint __spare2;
        private readonly InlineArray8<ulong> __spare3;
    }

    // int close(int fd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "close")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Optimizes ordinary descriptor cleanup; close can block for options such as SO_LINGER
    private static partial int close_raw(int fd);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult close(FileDescriptor fd) => new(close_raw(fd.Value));

    // int open(const char *pathname, int flags, mode_t mode);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "open", StringMarshalling = StringMarshalling.Utf8)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial int open_raw(string path, int flags, LinuxFileMode mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> open(string path, int flags, LinuxFileMode mode) => new(new(open_raw(path, flags, mode)));

    // int dup(int oldfd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "dup")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int dup_raw(int oldfd);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> dup(FileDescriptor oldfd) => new(new(dup_raw(oldfd.Value)));

    // ssize_t read(int fd, void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "read")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial nint read_raw(int fd, void* buf, nuint count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nuint> read(FileDescriptor fd, void* buf, nuint count) => new((nuint)read_raw(fd.Value, buf, count));

    // ssize_t read(int fd, void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "read")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Caller must ensure the descriptor and operation cannot block
    private static partial nint read_noblock_raw(int fd, void* buf, nuint count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nuint> read_noblock(FileDescriptor fd, void* buf, nuint count) => new((nuint)read_noblock_raw(fd.Value, buf, count));

    // ssize_t write(int fd, const void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "write")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial nint write_raw(int fd, void* buf, nuint count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nuint> write(FileDescriptor fd, void* buf, nuint count) => new((nuint)write_raw(fd.Value, buf, count));

    // ssize_t write(int fd, const void* buf, size_t count);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "write")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Caller must ensure the descriptor and operation cannot block
    private static partial nint write_noblock_raw(int fd, void* buf, nuint count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nuint> write_noblock(FileDescriptor fd, void* buf, nuint count) => new((nuint)write_noblock_raw(fd.Value, buf, count));

    // int ioctl(int fd, unsigned long operation, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "ioctl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Optimizes expected fast requests; a blocking ioctl can delay GC
    private static partial int ioctl_raw(int fd, nuint operation, void* argp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult ioctl(FileDescriptor fd, ulong operation, void* argp) => new(ioctl_raw(fd.Value, checked((nuint)operation), argp));

    // int fcntl(int fd, int cmd, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fcntl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int fcntl_raw(int fd, int cmd);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<int> fcntl(FileDescriptor fd, int cmd) => new(fcntl_raw(fd.Value, cmd));

    // int fcntl(int fd, int cmd, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fcntl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int fcntl_raw(int fd, int cmd, int arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<int> fcntl(FileDescriptor fd, int cmd, int arg) => new(fcntl_raw(fd.Value, cmd, arg));

    // int statx(int dirfd, const char *restrict pathname, int flags, unsigned int mask, struct statx *restrict statxbuf);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "statx")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Optimizes the common local-filesystem path; a blocking remote filesystem can delay GC
    private static partial int statx_raw(int dirfd, byte* pathname, int flags, uint mask, statx* statxbuf);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult statx_fd(FileDescriptor fd, out statx statbuf)
    {
        byte emptyPath = 0;
        fixed(statx* statbufPtr = &statbuf)
            return new(statx_raw(fd.Value, &emptyPath, AT_EMPTY_PATH, STATX_BASIC_STATS, statbufPtr));
    }

    // off_t lseek(int fd, off_t offset, int whence);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "lseek")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Optimizes the common local-filesystem path; a blocking filesystem can delay GC
    private static partial long lseek_raw(int fd, long offset, LinuxSeekOrigin whence);

    // off64_t lseek64(int fd, off64_t offset, int whence);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "lseek64")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition] // Optimizes the common local-filesystem path; a blocking filesystem can delay GC
    private static partial long lseek64_raw(int fd, long offset, LinuxSeekOrigin whence);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<long> lseek(FileDescriptor fd, long offset, LinuxSeekOrigin whence)
    {
        return new(NativeAbi.Is64Bit || NativeAbi.LibCImplementation == LibCImplementation.Musl
            ? lseek_raw(fd.Value, offset, whence)
            : lseek64_raw(fd.Value, offset, whence));
    }
}