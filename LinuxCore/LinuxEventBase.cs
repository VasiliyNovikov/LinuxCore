using System.Runtime.CompilerServices;

using static LinuxCore.Interop.EventFd;

namespace LinuxCore;

public abstract unsafe class LinuxEventBase(uint initialValue, int flags)
    : FileObject(eventfd(initialValue, flags | EFD_CLOEXEC).ThrowIfError())
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void WriteOne()
    {
        var value = 1ul;
        Write(&value, sizeof(ulong));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Read()
    {
        Unsafe.SkipInit(out ulong buffer);
        Read(&buffer, sizeof(ulong));
    }
}