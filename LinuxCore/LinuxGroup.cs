using System.Collections.Immutable;
using System.Runtime.InteropServices.Marshalling;

using static LinuxCore.Interop.User;

namespace LinuxCore;

public class LinuxGroup : LinuxSecurityObject
{
    public ImmutableArray<string> Members { get; }

    private unsafe LinuxGroup(group* group)
        : base(group->gr_gid, Utf8StringMarshaller.ConvertToManaged(group->gr_name)!)
    {
        var membersBuilder = ImmutableArray.CreateBuilder<string>();
        for (var memberPtr = group->gr_mem; *memberPtr != null; ++memberPtr)
            membersBuilder.Add(Utf8StringMarshaller.ConvertToManaged(*memberPtr)!);
        Members = membersBuilder.ToImmutable();
    }

    public static LinuxGroup? Get(string name) => ByNameQueryHelper.Instance.Get(name);
    public static LinuxGroup? Get(uint id) => ByGidQueryHelper.Instance.Get(id);

    private abstract class GroupQueryHelper<TId> : QueryHelper<LinuxGroup, group, TId>
    {
        protected override SysConfName BufferSizeConst => SysConfName.GetGrRSizeMax;
        protected override unsafe LinuxGroup FromNative(group* group) => new(group);
    }

    private sealed class ByNameQueryHelper : GroupQueryHelper<string>
    {
        protected override unsafe LinuxErrorNumber NativeGetReturn(string name, out group objectBuffer, byte* buffer, nuint bufferLen, out group* result) => getgrnam_r(name, out objectBuffer, buffer, bufferLen, out result);
        public static readonly ByNameQueryHelper Instance = new();
    }

    private sealed class ByGidQueryHelper : GroupQueryHelper<uint>
    {
        protected override unsafe LinuxErrorNumber NativeGetReturn(uint gid, out group objectBuffer, byte* buffer, nuint bufferLen, out group* result) => getgrgid_r(gid, out objectBuffer, buffer, bufferLen, out result);
        public static readonly ByGidQueryHelper Instance = new();
    }
}