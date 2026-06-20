using System.Runtime.CompilerServices;

using static LinuxCore.Interop.File;

namespace LinuxCore;

public abstract unsafe class FileObject(FileDescriptor descriptor, bool ownsDescriptor = true) : NativeObject, IFileObject
{
    private bool? _isNonBlocking;

    /// <summary>
    /// Gets the underlying Linux file descriptor.
    /// </summary>
    public FileDescriptor Descriptor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => descriptor;
    }

    /// <summary>
    /// Gets the current file status flags via <c>fcntl(F_GETFL)</c>.
    /// </summary>
    public LinuxFileFlags Flags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (LinuxFileFlags)FileControl(F_GETFL);
    }

    /// <summary>
    /// Gets whether the file descriptor is in non-blocking mode (<see cref="LinuxFileFlags.NonBlock"/>).
    /// The result is cached after the first observation; use <see cref="Flags"/> for a live check.
    /// </summary>
    // Cached under the assumption descriptor flags are not externally changed after first observation.
    public bool IsNonBlocking
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isNonBlocking ??= (Flags & LinuxFileFlags.NonBlock) != 0;
    }

    /// <summary>
    /// Gets or sets the close-on-exec flag (<c>FD_CLOEXEC</c>) via <c>fcntl(F_GETFD/F_SETFD)</c>.
    /// When <see langword="true"/>, the descriptor is automatically closed when a new process image is executed.
    /// </summary>
    public bool CloseOnExec
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    protected static bool TryComplete<T>(LinuxResult<T> result, out T value) where T : unmanaged
    {
        if (result.IsError)
        {
            var error = LinuxErrorNumber.Last;
            if (error is LinuxErrorNumber.TryAgain or LinuxErrorNumber.InterruptedSystemCall)
            {
                value = default;
                return false;
            }
            throw new LinuxException(error);
        }
        value = result;
        return true;
    }
}