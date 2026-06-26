using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.SysCall;

namespace LinuxCore;

public static unsafe class SystemCall
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke(SystemCallNumber number) 
    {
        return Result(syscall(number));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<TResult>(SystemCallNumber number) 
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke<T>(SystemCallNumber number, T arg) 
        where T : unmanaged
    {
        return Result(syscall(number, Param(arg)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<T, TResult>(SystemCallNumber number, T arg) 
        where T : unmanaged
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number, Param(arg)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke<T1, T2>(SystemCallNumber number, T1 arg1, T2 arg2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        return Result(syscall(number, Param(arg1), Param(arg2)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<T1, T2, TResult>(SystemCallNumber number, T1 arg1, T2 arg2)
        where T1 : unmanaged
        where T2 : unmanaged
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number, Param(arg1), Param(arg2)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke<T1, T2, T3>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        return Result(syscall(number, Param(arg1), Param(arg2), Param(arg3)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<T1, T2, T3, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number, Param(arg1), Param(arg2), Param(arg3)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke<T1, T2, T3, T4>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        return Result(syscall(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<T1, T2, T3, T4, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke<T1, T2, T3, T4, T5>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
    {
        return Result(syscall(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult Invoke<T1, T2, T3, T4, T5, T6>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
    {
        return Result(syscall(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5), Param(arg6)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where TResult : unmanaged
    {
        return Result<TResult>(syscall(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5), Param(arg6)));
    }

    public static class NonBlocking
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult Invoke<T>(SystemCallNumber number, T arg) 
            where T : unmanaged
        {
            return Result(syscall_noblock(number, Param(arg)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult<TResult> Invoke<T, TResult>(SystemCallNumber number, T arg) 
            where T : unmanaged
            where TResult : unmanaged
        {
            return Result<TResult>(syscall_noblock(number, Param(arg)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult Invoke<T1, T2>(SystemCallNumber number, T1 arg1, T2 arg2)
            where T1 : unmanaged
            where T2 : unmanaged
        {
            return Result(syscall_noblock(number, Param(arg1), Param(arg2)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult<TResult> Invoke<T1, T2, TResult>(SystemCallNumber number, T1 arg1, T2 arg2)
            where T1 : unmanaged
            where T2 : unmanaged
            where TResult : unmanaged
        {
            return Result<TResult>(syscall_noblock(number, Param(arg1), Param(arg2)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult Invoke<T1, T2, T3>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            return Result(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult<TResult> Invoke<T1, T2, T3, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where TResult : unmanaged
        {
            return Result<TResult>(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult Invoke<T1, T2, T3, T4>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            return Result(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult<TResult> Invoke<T1, T2, T3, T4, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where TResult : unmanaged
        {
            return Result<TResult>(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult Invoke<T1, T2, T3, T4, T5>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            return Result(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where TResult : unmanaged
        {
            return Result<TResult>(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult Invoke<T1, T2, T3, T4, T5, T6>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
        {
            return Result(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5), Param(arg6)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinuxResult<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(SystemCallNumber number, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
            where TResult : unmanaged
        {
            return Result<TResult>(syscall_noblock(number, Param(arg1), Param(arg2), Param(arg3), Param(arg4), Param(arg5), Param(arg6)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint Param<T>(T param) where T : unmanaged
    {
        return sizeof(T) switch
        {
            1 => Unsafe.BitCast<T, byte>(param),
            2 => Unsafe.BitCast<T, ushort>(param),
            4 => Unsafe.BitCast<T, int>(param),
            8 => (nint)Unsafe.BitCast<T, long>(param),
            _ => throw new ArgumentException($"Unsupported parameter size: {sizeof(T)}", nameof(param))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LinuxResult Result(nint result) => Unsafe.BitCast<int, LinuxResult>((int)result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LinuxResult<T> Result<T>(nint result) where T : unmanaged
    {
        return sizeof(T) switch
        {
            4 => Unsafe.BitCast<int, LinuxResult<T>>((int)result),
            8 => Unsafe.BitCast<long, LinuxResult<T>>(result),
            _ => throw new ArgumentException($"Unsupported result size: {sizeof(T)}", nameof(result))
        };
    }
}