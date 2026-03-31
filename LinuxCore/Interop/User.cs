using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class User
{
    [StructLayout(LayoutKind.Sequential)]
    public struct passwd
    {
        public byte* pw_name;   // username
        public byte* pw_passwd; // user password
        public uint  pw_uid;    // user ID
        public uint  pw_gid;    // group ID
        public byte* pw_gecos;  // user information
        public byte* pw_dir;    // home directory
        public byte* pw_shell;  // shell program
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct group
    {
        public byte*  gr_name;   // group name
        public byte*  gr_passwd; // group password
        public uint   gr_gid;    // group ID
        public byte** gr_mem;    // null-terminated array of group members
    }
    
    // uid_t geteuid (void)
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "geteuid")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    public static partial uint geteuid();

    // int getpwnam_r(const char *name, struct passwd *pwd, char *buf, size_t buflen, struct passwd **result);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getpwnam_r", StringMarshalling = StringMarshalling.Utf8)]
    public static partial LinuxErrorNumber getpwnam_r(string name, out passwd pwd, byte* buf, nuint buflen, out passwd* result);

    // int getpwuid_r(uid_t uid, struct passwd *pwd, char *buf, size_t buflen, struct passwd **result);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getpwuid_r")]
    public static partial LinuxErrorNumber getpwuid_r(uint uid, out passwd pwd, byte* buf, nuint buflen, out passwd* result);

    // int getgrnam_r(const char *name, struct group *grp, char *buf, size_t buflen, struct group **result);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getgrnam_r", StringMarshalling = StringMarshalling.Utf8)]
    public static partial LinuxErrorNumber getgrnam_r(string name, out group grp, byte* buf, nuint buflen, out group* result);

    // int getgrgid_r(gid_t gid, struct group *grp, char *buf, size_t buflen, struct group **result);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "getgrgid_r")]
    public static partial LinuxErrorNumber getgrgid_r(uint gid, out group grp, byte* buf, nuint buflen, out group* result);
}