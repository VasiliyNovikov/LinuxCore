using System.Runtime.CompilerServices;

using LinuxCore.Interop;

namespace LinuxCore;

public abstract unsafe class LinuxEventBase(uint initialValue, int flags)
    : FileObject(LibC.eventfd(initialValue, flags | LibC.EFD_CLOEXEC).ThrowIfError())
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