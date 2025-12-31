using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static unsafe partial class Poll
{
    public const short POLLIN   = 0b000001; // There is data to read
    public const short POLLPRI  = 0b000010; // There is urgent data to read
    public const short POLLOUT  = 0b000100; // Writing now not block
    public const short POLLERR  = 0b001000; // Error condition
    public const short POLLHUP  = 0b010000; // Hung up
    public const short POLLNVAL = 0b100000; // Invalid request: fd not open

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct pollfd
    {
        public readonly FileDescriptor fd; // File descriptor to poll
        public readonly short events;      // Types of events poller cares about
        public readonly short revents;     // Types of events that actually occurred
    }

    // int poll(struct pollfd *fds, nfds_t nfds, int timeout);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "poll")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LinuxResult<int> poll(pollfd* fds, uint nfds, int timeout);
}