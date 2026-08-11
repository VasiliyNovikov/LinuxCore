using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace LinuxCore;

public static class LinuxTextFileExtensions
{
    private static readonly UTF8Encoding Encoding = new(false);

    extension(LinuxFile file)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadAllText()
        {
            var length = (int)(file.Size - file.Position);
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                var bytes = buffer[..length];
                file.ReadExactly(bytes);
                return Encoding.GetString(bytes);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAllText(string text)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(Encoding.GetMaxByteCount(text.Length));
            try
            {
                file.WriteExactly(buffer.AsSpan(0, Encoding.GetBytes(text, buffer)));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}