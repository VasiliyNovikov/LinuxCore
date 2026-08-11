using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Process
{
    public const int SIGKILL = 9;

    [StructLayout(LayoutKind.Sequential)]
    public struct posix_spawn_file_actions_t
    {
        private nint __alignment;
        private fixed byte __storage[72];
    }

    // int posix_spawnp(pid_t *pid, const char *file, const posix_spawn_file_actions_t *file_actions, const posix_spawnattr_t *attrp, char *const argv[], char *const envp[]);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "posix_spawnp")]
    public static partial LinuxErrorNumber posix_spawnp(int* pid, byte* file, posix_spawn_file_actions_t* file_actions, void* attrp, byte** argv, byte** envp);

    // int posix_spawn_file_actions_init(posix_spawn_file_actions_t *file_actions);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "posix_spawn_file_actions_init")]
    public static partial LinuxErrorNumber posix_spawn_file_actions_init(posix_spawn_file_actions_t* file_actions);

    // int posix_spawn_file_actions_destroy(posix_spawn_file_actions_t *file_actions);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "posix_spawn_file_actions_destroy")]
    public static partial LinuxErrorNumber posix_spawn_file_actions_destroy(posix_spawn_file_actions_t* file_actions);

    // int posix_spawn_file_actions_adddup2(posix_spawn_file_actions_t *file_actions, int fd, int newfd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "posix_spawn_file_actions_adddup2")]
    private static partial LinuxErrorNumber posix_spawn_file_actions_adddup2_raw(posix_spawn_file_actions_t* file_actions, int fd, int newfd);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxErrorNumber posix_spawn_file_actions_adddup2(posix_spawn_file_actions_t* file_actions, FileDescriptor fd, FileDescriptor newfd)
    {
        return posix_spawn_file_actions_adddup2_raw(file_actions, fd.Value, newfd.Value);
    }

    // int posix_spawn_file_actions_addclose(posix_spawn_file_actions_t *file_actions, int fd);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "posix_spawn_file_actions_addclose")]
    private static partial LinuxErrorNumber posix_spawn_file_actions_addclose_raw(posix_spawn_file_actions_t* file_actions, int fd);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxErrorNumber posix_spawn_file_actions_addclose(posix_spawn_file_actions_t* file_actions, FileDescriptor fd)
    {
        return posix_spawn_file_actions_addclose_raw(file_actions, fd.Value);
    }

    // pid_t waitpid(pid_t pid, int *wstatus, int options);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "waitpid")]
    private static partial int waitpid_raw(int pid, int* wstatus, int options);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<int> waitpid(int pid, int* wstatus, int options) => new(waitpid_raw(pid, wstatus, options));

    // int kill(pid_t pid, int sig);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "kill")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int kill_raw(int pid, int sig);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult kill(int pid, int sig) => new(kill_raw(pid, sig));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LinuxResult<FileDescriptor> pidfd_open(int pid, uint flags)
    {
        return SystemCall.Invoke<int, uint, FileDescriptor>(SystemCallTable.PidFdOpen, pid, flags);
    }
}