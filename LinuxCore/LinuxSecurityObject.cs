using System.Runtime.CompilerServices;

namespace LinuxCore;

public abstract class LinuxSecurityObject(uint id, string name)
{
    public uint Id => id;
    public string Name => name;

    protected abstract class QueryHelper<T, TNative, TId>
        where T : LinuxSecurityObject
        where TNative : unmanaged
    {
        public unsafe T? Get(TId id)
        {
            [SkipLocalsInit]
            static bool TryGet(QueryHelper<T, TNative, TId> self, TId id, int bufferSize, out T? @object)
            {
                var buffer = stackalloc byte[bufferSize];
                var error = self.NativeGetReturn(id, out _, buffer, (nuint)bufferSize, out var result);
                if (error == LinuxErrorNumber.OK)
                {
                    @object = result == null ? null : self.FromNative(result);
                    return true;
                }

                if (error != LinuxErrorNumber.OutOfRange)
                    throw new LinuxException(error);

                @object = null;
                return false;
            }

            var bufferSize = 256;
            while (true)
            {
                if (TryGet(this, id, bufferSize, out var @object))
                    return @object;
                bufferSize *= 2;
            }
        }

        protected abstract unsafe LinuxErrorNumber NativeGetReturn(TId id, out TNative nativeObject, byte* buffer, nuint bufferLen, out TNative* result);
        protected abstract unsafe T FromNative(TNative* nativeObject);
    }
}