using System.Buffers;
using System.Runtime.CompilerServices;
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
        public unsafe T? Get(TId id)
        {
            [SkipLocalsInit]
            static bool TryGet(QueryHelper<T, TNative, TId> self, TId id, int bufferSize, out T? @object)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    fixed (byte* bufferPtr = buffer)
                    {
                        var error = self.NativeGetReturn(id, out var nativeObject, bufferPtr, (nuint)bufferSize, out var result);
                        if (error == LinuxErrorNumber.OK)
                        {
                            @object = result is null ? null : self.FromNative(nativeObject);
                            return true;
                        }

                        if (error != LinuxErrorNumber.OutOfRange)
                            throw new LinuxException(error);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                @object = null;
                return false;
            }

            var bufferSize = (int)SystemConfiguration.Get(BufferSizeConst);
            if (bufferSize <= 0)
                bufferSize = 1024;
            while (true)
            {
                if (TryGet(this, id, bufferSize, out var @object))
                    return @object;
                bufferSize *= 2;
            }
        }

        protected abstract SystemConfigurationName BufferSizeConst { get; }
        protected abstract unsafe LinuxErrorNumber NativeGetReturn(TId id, out TNative nativeObject, byte* buffer, nuint bufferLen, out TNative* result);
        protected abstract T FromNative(in TNative nativeObject);
    }
}