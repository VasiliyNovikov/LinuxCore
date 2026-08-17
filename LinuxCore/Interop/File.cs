using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe class File
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

    internal const int  AT_FDCWD          = -100;
    internal const int  AT_EMPTY_PATH     = 0x1000;
    internal const uint STATX_BASIC_STATS = 0x07ff;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Optimizes ordinary descriptor cleanup; close can block for options such as SO_LINGER
    public static LinuxResult close(FileDescriptor fd) => SystemCall.NonBlocking.Invoke(SystemCallTable.Current.Close, fd);

    // int openat(int dirfd, const char *pathname, int flags, mode_t mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static LinuxResult<FileDescriptor> open(string path, int flags, LinuxFileMode mode)
    {
        LinuxResult<FileDescriptor> result;
        var error = LinuxErrorNumber.OK;
        using (var pathScope = new NativeStringScope(path, stackalloc byte[NativeStringScope.BufferSize]))
        {
            result = SystemCall.Invoke<int, nint, int, LinuxFileMode, FileDescriptor>(SystemCallTable.Current.OpenAt, AT_FDCWD, (nint)pathScope.NativeValue, flags, mode);
            if (result.IsError)
                error = LinuxErrorNumber.Last;
        }
        if (error != LinuxErrorNumber.OK)
            LinuxErrorNumber.Last = error;
        return result;
    }

    // int dup(int oldfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> dup(FileDescriptor oldfd) => SystemCall.NonBlocking.Invoke<FileDescriptor, FileDescriptor>(SystemCallTable.Current.Dup, oldfd);

    // ssize_t read(int fd, void* buf, size_t count);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nuint> read(FileDescriptor fd, void* buf, nuint count) => SystemCall.Invoke<FileDescriptor, nint, nuint, nuint>(SystemCallTable.Current.Read, fd, (nint)buf, count);

    // ssize_t read(int fd, void* buf, size_t count);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Caller must ensure the descriptor and operation cannot block
    public static LinuxResult<nuint> read_noblock(FileDescriptor fd, void* buf, nuint count) => SystemCall.NonBlocking.Invoke<FileDescriptor, nint, nuint, nuint>(SystemCallTable.Current.Read, fd, (nint)buf, count);

    // ssize_t write(int fd, const void* buf, size_t count);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<nuint> write(FileDescriptor fd, void* buf, nuint count) => SystemCall.Invoke<FileDescriptor, nint, nuint, nuint>(SystemCallTable.Current.Write, fd, (nint)buf, count);

    // ssize_t write(int fd, const void* buf, size_t count);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Caller must ensure the descriptor and operation cannot block
    public static LinuxResult<nuint> write_noblock(FileDescriptor fd, void* buf, nuint count) => SystemCall.NonBlocking.Invoke<FileDescriptor, nint, nuint, nuint>(SystemCallTable.Current.Write, fd, (nint)buf, count);

    // int ioctl(int fd, unsigned long operation, ...);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Optimizes expected fast requests; a blocking ioctl can delay GC
    public static LinuxResult ioctl(FileDescriptor fd, ulong operation, void* argp) => SystemCall.NonBlocking.Invoke(SystemCallTable.Current.Ioctl, fd, checked((nuint)operation), (nint)argp);

    // int fcntl(int fd, int cmd, ...);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<int> fcntl(FileDescriptor fd, int cmd) => SystemCall.NonBlocking.Invoke<FileDescriptor, int, int>(SystemCallTable.Current.Fcntl, fd, cmd);

    // int fcntl(int fd, int cmd, ...);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<int> fcntl(FileDescriptor fd, int cmd, int arg) => SystemCall.NonBlocking.Invoke<FileDescriptor, int, int, int>(SystemCallTable.Current.Fcntl, fd, cmd, arg);

    // int statx(int dirfd, const char *restrict pathname, int flags, unsigned int mask, struct statx *restrict statxbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Optimizes the common local-filesystem path; a blocking remote filesystem can delay GC
    public static LinuxResult statx_fd(FileDescriptor fd, out statx statbuf)
    {
        byte emptyPath = 0;
        fixed (statx* statbufPtr = &statbuf)
            return SystemCall.NonBlocking.Invoke(SystemCallTable.Current.Statx, fd, (nint)(&emptyPath), AT_EMPTY_PATH, STATX_BASIC_STATS, (nint)statbufPtr);
    }

    // off_t lseek(int fd, off_t offset, int whence); or int _llseek(int fd, unsigned long offset_high, unsigned long offset_low, loff_t *result, unsigned int whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Optimizes the common local-filesystem path; a blocking filesystem can delay GC
    public static LinuxResult<long> lseek(FileDescriptor fd, long offset, LinuxSeekOrigin whence)
    {
        if (NativeAbi.Is64Bit)
            return SystemCall.NonBlocking.Invoke<FileDescriptor, long, LinuxSeekOrigin, long>(SystemCallTable.Current.Lseek, fd, offset, whence);

        long result;
        var offsetBits = (ulong)offset;
        var seekResult = SystemCall.NonBlocking.Invoke(SystemCallTable.Current.Llseek, fd, (uint)(offsetBits >> 32), (uint)offsetBits, (nint)(&result), whence);
        return seekResult.IsError ? new(-1L) : new(result);
    }
}