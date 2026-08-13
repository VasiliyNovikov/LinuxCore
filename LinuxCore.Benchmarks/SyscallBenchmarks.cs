using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace LinuxCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public partial class SyscallBenchmarks
{
    private const int F_GETFD = 1;
    private const int SYS_CLOSE = 3;
    private const int SYS_DUP = 32;
    private const int SYS_GETPID = 39;
    private const int SYS_FCNTL = 72;

    private int _fd = -1;

    [GlobalSetup]
    public void Setup()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("Syscall benchmarks are implemented only for x64");
        _fd = Open("/dev/null", 0);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_fd >= 0 && Close(_fd) != 0)
            throw new InvalidOperationException("Failed to close /dev/null");
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("getpid")]
    public int LibC_GetPid() => GetPid();

    [Benchmark]
    [BenchmarkCategory("getpid")]
    public int Syscall_GetPid() => (int)Syscall(SYS_GETPID);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("fcntl(F_GETFD)")]
    public int LibC_FcntlGetFd() => Fcntl(_fd, F_GETFD);

    [Benchmark]
    [BenchmarkCategory("fcntl(F_GETFD)")]
    public int Syscall_FcntlGetFd() => (int)Syscall(SYS_FCNTL, _fd, F_GETFD);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("dup+close")]
    public int LibC_DupClose() => Close(Dup(_fd));

    [Benchmark]
    [BenchmarkCategory("dup+close")]
    public int Syscall_DupClose()
    {
        var duplicate = Syscall(SYS_DUP, _fd);
        return (int)Syscall(SYS_CLOSE, duplicate);
    }

    // int open(const char *pathname, int flags, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "open", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Open(string path, int flags);

    // pid_t getpid(void);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getpid")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int GetPid();

    // int fcntl(int fd, int op, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "fcntl")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int Fcntl(int fd, int command);

    // int dup(int oldfd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "dup")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int Dup(int fd);

    // int close(int fd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "close")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int Close(int fd);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial nint Syscall(nint number);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial nint Syscall(nint number, nint arg1);

    // long syscall(long number, ...);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "syscall")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial nint Syscall(nint number, nint arg1, nint arg2);
}