using System.Runtime.InteropServices.Marshalling;

using static LinuxCore.Interop.User;

namespace LinuxCore;

public class LinuxUser : LinuxSecurityObject
{
    public const uint RootUserId = 0;

    public uint GroupId { get; }
    public string? Home { get; }
    public string? Shell { get; }
    public LinuxGroup Group => field ??= LinuxGroup.Get(GroupId)!;

    private unsafe LinuxUser(passwd* pwd)
        : base(pwd->pw_uid, Utf8StringMarshaller.ConvertToManaged(pwd->pw_name)!)
    {
        GroupId = pwd->pw_gid;
        Home = Utf8StringMarshaller.ConvertToManaged(pwd->pw_dir);
        Shell = Utf8StringMarshaller.ConvertToManaged(pwd->pw_shell);
    }

    public static LinuxUser? Get(string name) => ByNameQueryHelper.Instance.Get(name);
    public static LinuxUser? Get(uint id) => ByUidQueryHelper.Instance.Get(id);

    public static uint CurrentId => geteuid();
    public static LinuxUser Current => Get(CurrentId)!;
    public static bool IsRoot => CurrentId == RootUserId;

    private abstract class UserQueryHelper<TId> : QueryHelper<LinuxUser, passwd, TId>
    {
        protected override unsafe LinuxUser FromNative(passwd* pwd) => new(pwd);
    }

    private sealed class ByNameQueryHelper : UserQueryHelper<string>
    {
        protected override unsafe LinuxErrorNumber NativeGetReturn(string name, out passwd pwd, byte* buffer, nuint bufferLen, out passwd* result) => getpwnam_r(name, out pwd, buffer, bufferLen, out result);

        public static readonly ByNameQueryHelper Instance = new();
    }

    private sealed class ByUidQueryHelper : UserQueryHelper<uint>
    {
        protected override unsafe LinuxErrorNumber NativeGetReturn(uint uid, out passwd pwd, byte* buffer, nuint bufferLen, out passwd* result) => getpwuid_r(uid, out pwd, buffer, bufferLen, out result);

        public static readonly ByUidQueryHelper Instance = new();
    }
}