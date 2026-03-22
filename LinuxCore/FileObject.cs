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

    public LinuxFileFlags Flags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (LinuxFileFlags)FileControl(F_GETFL);
    }

    public bool CloseOnExec
    {
        get => (FileControl(F_GETFD) & FD_CLOEXEC) != 0;
        set => FileControl(F_SETFD, value ? FD_CLOEXEC : 0);
    }

    protected override void ReleaseUnmanagedResources()
    {
        if (ownsDescriptor)
            descriptor.Close();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected nuint Read(void* buffer, nuint count) => read(descriptor, buffer, count).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TryRead(void* buffer, nuint count, out nuint readCount) => TryComplete(read_noblock(descriptor, buffer, count), out readCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected nuint Write(void* buffer, nuint count) => write(descriptor, buffer, count).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TryWrite(void* buffer, nuint count, out nuint writtenCount) => TryComplete(write_noblock(descriptor, buffer, count), out writtenCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IOControl(ulong request, void* arg) => ioctl(descriptor, request, arg).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void IOControl(ulong request, ulong arg) => IOControl(request, (void*)arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void IOControl<T>(ulong request, ref T arg) where T : unmanaged
    {
        fixed (T* pArg = &arg)
            IOControl(request, pArg);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int FileControl(int cmd) => fcntl(descriptor, cmd).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void FileControl(int cmd, int arg) => fcntl(descriptor, cmd, arg).ThrowIfError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static bool TryComplete(LinuxResult<nuint> result, out nuint count)
    {
        if (result.IsError)
        {
            var error = LinuxErrorNumber.Last;
            if (error is LinuxErrorNumber.TryAgain or LinuxErrorNumber.InterruptedSystemCall)
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
