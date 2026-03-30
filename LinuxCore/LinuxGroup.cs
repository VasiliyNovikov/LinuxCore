using System.Runtime.InteropServices.Marshalling;

using static LinuxCore.Interop.User;

namespace LinuxCore;

public class LinuxGroup : LinuxSecurityObject
{
    private unsafe LinuxGroup(group* group)
        : base(group->gr_gid, Utf8StringMarshaller.ConvertToManaged(group->gr_name)!)
    {
    }

    public static LinuxGroup? Get(string name) => ByNameQueryHelper.Instance.Get(name);
    public static LinuxGroup? Get(uint id) => ByGidQueryHelper.Instance.Get(id);

    private abstract class GroupQueryHelper<TId> : QueryHelper<LinuxGroup, group, TId>
    {
        protected override unsafe LinuxGroup FromNative(group* group) => new(group);
    }

    private sealed class ByNameQueryHelper : GroupQueryHelper<string>
    {
        protected override unsafe LinuxErrorNumber NativeGetReturn(string name, out group pwd, byte* buffer, nuint bufferLen, out group* result) => getgrnam_r(name, out pwd, buffer, bufferLen, out result);

        public static readonly ByNameQueryHelper Instance = new();
    }

    private sealed class ByGidQueryHelper : GroupQueryHelper<uint>
    {
        protected override unsafe LinuxErrorNumber NativeGetReturn(uint gid, out group pwd, byte* buffer, nuint bufferLen, out group* result) => getgrgid_r(gid, out pwd, buffer, bufferLen, out result);

        public static readonly ByGidQueryHelper Instance = new();
    }
}