using System;
using System.Runtime.ConstrainedExecution;

namespace LinuxCore;

public abstract class NativeObject : CriticalFinalizerObject, IDisposable
{
    protected abstract void ReleaseUnmanagedResources();

    ~NativeObject() => ReleaseUnmanagedResources();

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}