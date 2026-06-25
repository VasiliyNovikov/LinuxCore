using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace LinuxCore;

public abstract class NativeObject : CriticalFinalizerObject, IDisposable
{
    private int _disposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract void ReleaseUnmanagedResources();

    ~NativeObject() => DisposeCore();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed == 1, this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisposeCore()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            ReleaseUnmanagedResources();
    }
}