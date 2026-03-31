using System.Buffers;
using System.Runtime.InteropServices.Marshalling;

namespace LinuxCore;

public abstract unsafe class LinuxSecurityObject(uint id, byte* name)
{
    public uint Id => id;
    public string Name { get; } = Utf8StringMarshaller.ConvertToManaged(name)!;

    protected abstract class QueryHelper<T, TNative, TId>
        where T : LinuxSecurityObject
        where TNative : unmanaged
    {
        private const int MaxBufferSize = 0x100000;

        protected abstract SystemConfigurationName BufferSizeConst { get; }
        protected abstract LinuxErrorNumber NativeGet(TId id, out TNative nativeObject, byte* buffer, nuint bufferLen, out TNative* result);
        protected abstract T FromNative(in TNative nativeObject);

        public T? Get(TId id)
        {
            var bufferSize = (int)SystemConfiguration.Get(BufferSizeConst);
            if (bufferSize <= 0)
                bufferSize = 1024;
            while (bufferSize <= MaxBufferSize)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    fixed (byte* bufferPtr = buffer)
                    {
                        var error = NativeGet(id, out var nativeObject, bufferPtr, (nuint)bufferSize, out var result);
                        if (error == LinuxErrorNumber.OK)
                            return result is null ? null : FromNative(nativeObject);
                        if (error != LinuxErrorNumber.OutOfRange)
                            throw new LinuxException(error);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                bufferSize *= 2;
            }
            throw new LinuxException(LinuxErrorNumber.OutOfRange);
        }
    }
}