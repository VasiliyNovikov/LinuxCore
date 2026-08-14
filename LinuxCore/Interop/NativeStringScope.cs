using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace LinuxCore.Interop;

internal ref struct NativeStringScope : IDisposable
{
    public const int BufferSize = 0x100;

    private Utf8StringMarshaller.ManagedToUnmanagedIn _marshaller = new();

    public unsafe byte* NativeValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _marshaller.ToUnmanaged();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeStringScope(string value, Span<byte> scratchBuffer) => _marshaller.FromManaged(value, scratchBuffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _marshaller.Free();
}