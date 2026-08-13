using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;

namespace LinuxCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Config(typeof(ArchitectureConfig))]
public unsafe partial class SyscallBenchmarks
{
    private const int F_GETFD = 1;
    private const int PROT_READ = 0x1;
    private const int PROT_WRITE = 0x2;
    private const int PROT_EXEC = 0x4;
    private const int MAP_PRIVATE = 0x2;
    private const int MAP_ANONYMOUS = 0x20;
    private const int StubSize = 16;

    private static readonly (nint GetPid, nint Dup, nint Close, nint Fcntl) SystemCalls = GetSystemCalls();

    private int _fd = -1;
    private void* _code;
    private nuint _codeLength;
    private static delegate* unmanaged[SuppressGCTransition]<int> _directGetPid;
    private static delegate* unmanaged[SuppressGCTransition]<int, int> _directDup;
    private static delegate* unmanaged[SuppressGCTransition]<int, int> _directClose;
    private static delegate* unmanaged[SuppressGCTransition]<int, int, int> _directFcntl;

    [GlobalSetup]
    public void Setup()
    {
        try
        {
            SetupCommonResources();
        }
        catch (Exception exception)
        {
            var release = ReleaseResources();
            if (CreateReleaseException(release) is { } releaseException)
                throw new AggregateException("Benchmark setup and rollback failed", exception, releaseException);
            throw;
        }
    }

    [GlobalSetup(Targets = new[] { nameof(Direct_GetPid), nameof(Direct_FcntlGetFd), nameof(Direct_DupClose) })]
    public void SetupDirectSyscalls()
    {
        try
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
                throw new PlatformNotSupportedException("Direct syscall stubs are implemented only for x64");

            SetupCommonResources();
            CreateDirectSyscalls();
            ValidateDirectSyscalls();
        }
        catch (Exception exception)
        {
            var release = ReleaseResources();
            if (CreateReleaseException(release) is { } releaseException)
                throw new AggregateException("Direct syscall setup and rollback failed", exception, releaseException);
            throw;
        }
    }

    [GlobalCleanup]
    public void Cleanup() => CleanupResources();

    [GlobalCleanup(Targets = new[] { nameof(Direct_GetPid), nameof(Direct_FcntlGetFd), nameof(Direct_DupClose) })]
    public void CleanupDirectSyscalls() => CleanupResources();

    private void CleanupResources()
    {
        if (CreateReleaseException(ReleaseResources()) is { } releaseException)
            throw releaseException;
    }

    private void SetupCommonResources()
    {
        _fd = Open("/dev/null", 0);
        if (_fd < 0)
            throw new InvalidOperationException("Failed to open /dev/null");

        if (GetPid() != (int)Syscall(SystemCalls.GetPid))
            throw new InvalidOperationException("getpid syscall number is invalid for this architecture");
        if (Fcntl(_fd, F_GETFD) != (int)Syscall(SystemCalls.Fcntl, _fd, F_GETFD))
            throw new InvalidOperationException("fcntl syscall number is invalid for this architecture");

        var duplicate = (int)Syscall(SystemCalls.Dup, _fd);
        if (duplicate < 0)
            throw new InvalidOperationException("dup syscall number is invalid for this architecture");
        if (Syscall(SystemCalls.Close, duplicate) != 0)
            throw new InvalidOperationException("close syscall number is invalid for this architecture");
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("getpid")]
    public int LibC_GetPid() => GetPid();

    [Benchmark]
    [BenchmarkCategory("getpid")]
    public int Syscall_GetPid() => (int)Syscall(SystemCalls.GetPid);

    [Benchmark]
    [BenchmarkCategory("getpid")]
    public int Direct_GetPid() => _directGetPid();

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("fcntl(F_GETFD)")]
    public int LibC_FcntlGetFd() => Fcntl(_fd, F_GETFD);

    [Benchmark]
    [BenchmarkCategory("fcntl(F_GETFD)")]
    public int Syscall_FcntlGetFd() => (int)Syscall(SystemCalls.Fcntl, _fd, F_GETFD);

    [Benchmark]
    [BenchmarkCategory("fcntl(F_GETFD)")]
    public int Direct_FcntlGetFd() => _directFcntl(_fd, F_GETFD);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("dup+close")]
    public int LibC_DupClose() => Close(Dup(_fd));

    [Benchmark]
    [BenchmarkCategory("dup+close")]
    public int Syscall_DupClose()
    {
        var duplicate = Syscall(SystemCalls.Dup, _fd);
        return (int)Syscall(SystemCalls.Close, duplicate);
    }

    [Benchmark]
    [BenchmarkCategory("dup+close")]
    public int Direct_DupClose()
    {
        var duplicate = _directDup(_fd);
        return _directClose(duplicate);
    }

    private void CreateDirectSyscalls()
    {
        _codeLength = (nuint)Environment.SystemPageSize;
        _code = Mmap(null, _codeLength, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
        if (_code == (void*)(nint)(-1))
        {
            _code = null;
            _codeLength = 0;
            throw new InvalidOperationException($"mmap failed with errno {Marshal.GetLastPInvokeError()}");
        }

        var getPid = (delegate* unmanaged[SuppressGCTransition]<int>)EmitStub((byte*)_code, _codeLength, 0, 39);
        var dup = (delegate* unmanaged[SuppressGCTransition]<int, int>)EmitStub((byte*)_code, _codeLength, 1, 32);
        var close = (delegate* unmanaged[SuppressGCTransition]<int, int>)EmitStub((byte*)_code, _codeLength, 2, 3);
        var fcntl = (delegate* unmanaged[SuppressGCTransition]<int, int, int>)EmitStub((byte*)_code, _codeLength, 3, 72);

        if (Mprotect(_code, _codeLength, PROT_READ | PROT_EXEC) != 0)
            throw new InvalidOperationException($"mprotect failed with errno {Marshal.GetLastPInvokeError()}");

        _directGetPid = getPid;
        _directDup = dup;
        _directClose = close;
        _directFcntl = fcntl;
    }

    private void ValidateDirectSyscalls()
    {
        if (_directGetPid() != GetPid())
            throw new InvalidOperationException("Direct getpid syscall returned an invalid result");
        if (_directFcntl(_fd, F_GETFD) != Fcntl(_fd, F_GETFD))
            throw new InvalidOperationException("Direct fcntl syscall returned an invalid result");

        var duplicate = _directDup(_fd);
        if (duplicate < 0)
            throw new InvalidOperationException($"Direct dup syscall failed with errno {-duplicate}");
        if (_directClose(duplicate) != 0)
            throw new InvalidOperationException("Direct close syscall failed");
    }

    private (int UnmapResult, int UnmapError, int CloseResult, int CloseError) ReleaseResources()
    {
        _directGetPid = null;
        _directDup = null;
        _directClose = null;
        _directFcntl = null;

        var unmapResult = 0;
        var unmapError = 0;
        if (_code != null)
        {
            unmapResult = Munmap(_code, _codeLength);
            if (unmapResult == 0)
            {
                _code = null;
                _codeLength = 0;
            }
            else
                unmapError = Marshal.GetLastPInvokeError();
        }

        var fd = _fd;
        _fd = -1;
        var closeResult = fd < 0 ? 0 : CloseResource(fd);
        var closeError = closeResult == 0 ? 0 : Marshal.GetLastPInvokeError();
        return (unmapResult, unmapError, closeResult, closeError);
    }

    private static InvalidOperationException? CreateReleaseException((int UnmapResult, int UnmapError, int CloseResult, int CloseError) release)
    {
        return release is (0, 0, 0, 0)
            ? null
            : new($"Failed to release benchmark resources: munmap={release.UnmapResult} errno={release.UnmapError}, close={release.CloseResult} errno={release.CloseError}");
    }

    private static byte* EmitStub(byte* code, nuint codeLength, int slot, int syscallNumber)
    {
        var offset = checked(slot * StubSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((nuint)(offset + StubSize), codeLength, nameof(slot));

        Span<byte> stub = new(code + offset, StubSize);
        stub.Fill(0xcc);
        stub[0] = 0xf3; // endbr64
        stub[1] = 0x0f;
        stub[2] = 0x1e;
        stub[3] = 0xfa;
        stub[4] = 0xb8; // mov eax, syscallNumber
        BinaryPrimitives.WriteInt32LittleEndian(stub[5..9], syscallNumber);
        stub[9] = 0x0f; // syscall
        stub[10] = 0x05;
        stub[11] = 0xc3; // ret
        return code + offset;
    }

    private static (nint GetPid, nint Dup, nint Close, nint Fcntl) GetSystemCalls()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 or Architecture.Arm or Architecture.Armv6 or Architecture.S390x or Architecture.Ppc64le => (20, 41, 6, 55),
            Architecture.X64 => (39, 32, 3, 72),
            Architecture.Arm64 or Architecture.LoongArch64 or Architecture.RiscV64 => (172, 23, 57, 25),
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
    }

    private sealed class ArchitectureConfig : ManualConfig
    {
        public ArchitectureConfig()
        {
            AddFilter(new SimpleFilter(benchmark => !benchmark.Descriptor.WorkloadMethod.Name.StartsWith("Direct_", StringComparison.Ordinal)
                || benchmark.Job.Environment.Platform == BenchmarkDotNet.Environments.Platform.X64
                || benchmark.Job.Environment.Platform == BenchmarkDotNet.Environments.Platform.AnyCpu
                    && RuntimeInformation.ProcessArchitecture == Architecture.X64));
        }
    }

    // void *mmap(void *addr, size_t length, int prot, int flags, int fd, off_t offset);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "mmap", SetLastError = true)]
    private static partial void* Mmap(void* address, nuint length, int protection, int flags, int fd, long offset);

    // int mprotect(void *addr, size_t len, int prot);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "mprotect", SetLastError = true)]
    private static partial int Mprotect(void* address, nuint length, int protection);

    // int munmap(void *addr, size_t length);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "munmap", SetLastError = true)]
    private static partial int Munmap(void* address, nuint length);

    // int close(int fd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "close", SetLastError = true)]
    private static partial int CloseResource(int fd);

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