using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Time
{
    public const int CLOCK_REALTIME           = 0;
    public const int CLOCK_MONOTONIC          = 1;
    public const int CLOCK_PROCESS_CPUTIME_ID = 2;
    public const int CLOCK_THREAD_CPUTIME_ID  = 3;
    public const int CLOCK_BOOTTIME           = 7;

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct timespec
    {
        public readonly long tv_sec;  // seconds
        public readonly long tv_nsec; // nanoseconds

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public timespec(long seconds, long nanoseconds)
        {
            tv_sec = seconds;
            tv_nsec = nanoseconds;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct timespec64
    {
        [FieldOffset(0)]
        public readonly long tv_sec;

        [FieldOffset(8)]
        public readonly int tv_nsec;

        [FieldOffset(12)]
        private readonly int __pad;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public timespec64(long seconds, long nanoseconds)
        {
            tv_sec = seconds;
            tv_nsec = checked((int)nanoseconds);
            __pad = 0;
        }
    }

    // int clock_gettime(clockid_t clockid, struct timespec *tp);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "clock_gettime")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int clock_gettime_raw(int clockid, timespec* tp);

    // int __clock_gettime64(clockid_t clockid, struct __timespec64 *tp);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "__clock_gettime64")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int clock_gettime64_raw(int clockid, timespec64* tp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult clock_gettime(int clockid, timespec* tp)
    {
        if (NativeAbi.Is64Bit)
            return new(clock_gettime_raw(clockid, tp));

        Unsafe.SkipInit(out timespec64 nativeTime);
        var result = clock_gettime64_raw(clockid, &nativeTime);
        if (result == 0)
            *tp = new timespec(nativeTime.tv_sec, nativeTime.tv_nsec);
        return new(result);
    }

    // int clock_nanosleep(clockid_t clockid, int flags, const struct timespec *request, struct timespec *remain);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "clock_nanosleep")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial int clock_nanosleep_raw(int clockid, int flags, timespec* request, timespec* remain);

    // int __clock_nanosleep_time64(clockid_t clockid, int flags, const struct __timespec64 *request, struct __timespec64 *remain);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "__clock_nanosleep_time64")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial int clock_nanosleep64_raw(int clockid, int flags, timespec64* request, timespec64* remain);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxErrorNumber clock_nanosleep(int clockid, int flags, timespec* request, timespec* remain)
    {
        if (NativeAbi.Is64Bit)
            return (LinuxErrorNumber)clock_nanosleep_raw(clockid, flags, request, remain);

        var nativeRequest = new timespec64(request->tv_sec, request->tv_nsec);
        Unsafe.SkipInit(out timespec64 nativeRemaining);
        var result = (LinuxErrorNumber)clock_nanosleep64_raw(clockid, flags, &nativeRequest, &nativeRemaining);
        if (result == LinuxErrorNumber.InterruptedSystemCall)
            *remain = new timespec(nativeRemaining.tv_sec, nativeRemaining.tv_nsec);
        return result;
    }
}