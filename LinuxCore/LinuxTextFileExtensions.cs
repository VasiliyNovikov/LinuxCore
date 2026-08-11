using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace LinuxCore;

public static class LinuxTextFileExtensions
{
    private const int DefaultBufferSize = 4096;
    private static readonly UTF8Encoding Encoding = new(false);

    extension(LinuxFile file)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadAllText(int bufferSize = DefaultBufferSize)
        {
            var buffer = new ArrayBufferWriter<byte>(Math.Max(bufferSize, file.CanSeek ? (int)(file.Size - file.Position) : 0));
            while (true)
            {
                var bytes = buffer.GetSpan(bufferSize);
                var read = file.Read(bytes);
                if (read == 0)
                    return Encoding.GetString(buffer.WrittenSpan);
                buffer.Advance(read);
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