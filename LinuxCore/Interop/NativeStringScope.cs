using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LinuxCore.Interop;

internal readonly unsafe ref struct NativeStringScope : IDisposable
{
    public const int BufferSize = 0x100;

    private readonly bool _allocated = false;

    public byte* NativeValue { get; } = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeStringScope(string? value, Span<byte> scratchBuffer)
    {
        if (value is null)
            return;

        if (3L * value.Length >= scratchBuffer.Length)
        {
            var byteCount = checked(Encoding.UTF8.GetByteCount(value) + 1);
            if (byteCount > scratchBuffer.Length)
            {
                scratchBuffer = new Span<byte>((byte*)NativeMemory.Alloc((nuint)byteCount), byteCount);
                _allocated = true;
            }
        }

        NativeValue = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(scratchBuffer));
        scratchBuffer[Encoding.UTF8.GetBytes(value, scratchBuffer)] = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_allocated)
            NativeMemory.Free(NativeValue);
    }
}