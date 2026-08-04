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

    [StructLayout(LayoutKind.Sequential)]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal struct timespec64(long seconds, long nanoseconds)
    {
        public readonly long tv_sec = seconds;
        public readonly int tv_nsec = checked((int)nanoseconds);
        public int pad = 0;
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
        if (NativeAbi.Is64Bit || NativeAbi.LibCImplementation == LibCImplementation.Musl)
            return new(clock_gettime_raw(clockid, tp));

        var tp64 = (timespec64*)tp;
        var result = clock_gettime64_raw(clockid, tp64);
        if (result == 0)
            tp64->pad = 0;
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
        if (NativeAbi.Is64Bit || NativeAbi.LibCImplementation == LibCImplementation.Musl)
            return (LinuxErrorNumber)clock_nanosleep_raw(clockid, flags, request, remain);

        var request64 = (timespec64*)request;
        var remain64 = (timespec64*)remain;
        if (request64->pad != 0)
            throw new ArgumentException("request->tv_nsec is too big for 32-bit platform");
        var result = (LinuxErrorNumber)clock_nanosleep64_raw(clockid, flags, request64, remain64);
        if (result == LinuxErrorNumber.InterruptedSystemCall)
            remain64->pad = 0;
        return result;
    }
}