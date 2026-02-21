using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace LinuxCore;

public abstract class NativeObject : CriticalFinalizerObject, IDisposable
{
    private int _disposed;

    protected abstract void ReleaseUnmanagedResources();

    ~NativeObject() => DisposeCore();

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    private void DisposeCore()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            ReleaseUnmanagedResources();
    }
}