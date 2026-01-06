using System.Runtime.CompilerServices;

using static LinuxCore.Interop.File;

namespace LinuxCore;

public abstract unsafe class FileObject(FileDescriptor descriptor, bool ownsDescriptor = true) : NativeObject, IFileObject
{
    public FileDescriptor Descriptor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => descriptor;
    }

    protected override void ReleaseUnmanagedResources()
    {
        if (ownsDescriptor)
            descriptor.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected nuint Read(void* buffer, nuint count) => read(descriptor, buffer, count).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TryRead(void* buffer, nuint count, out nuint readCount) => TryComplete(read(descriptor, buffer, count), out readCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected nuint Write(void* buffer, nuint count) => write(descriptor, buffer, count).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TryWrite(void* buffer, nuint count, out nuint writtenCount) => TryComplete(write(descriptor, buffer, count), out writtenCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IOCctl(ulong request, void* arg) => ioctl(descriptor, request, arg).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IOCctl(ulong request, ulong arg) => IOCctl(request, (void*)arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IOCctl<T>(ulong request, ref T arg) where T : unmanaged
    {
        fixed (T* pArg = &arg)
            IOCctl(request, pArg);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryComplete(LinuxResult<nuint> result, out nuint count)
    {
        if (result.IsError)
        {
            var error = LinuxErrorNumber.Last;
            if (error is LinuxErrorNumber.TryAgain or LinuxErrorNumber.OperationWouldBlock or LinuxErrorNumber.InterruptedSystemCall)
            {
                count = 0;
                return false;
            }
            throw new LinuxException(error);
        }
        count = result;
        return true;
    }
}