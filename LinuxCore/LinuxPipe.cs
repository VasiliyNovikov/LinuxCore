using System;
using System.Runtime.CompilerServices;

using static LinuxCore.Interop.Pipe;

namespace LinuxCore;

/// <summary>
/// Provides a <c>pipe2(2)</c>-backed unidirectional channel between two file descriptors.
/// </summary>
/// <remarks>
/// <para>A <see cref="LinuxPipe"/> wraps a pair of file descriptors: data written to <see cref="Writer"/>
/// can be read from <see cref="Reader"/>. The pipe is useful for inter-thread or inter-process
/// communication and integrates with <see cref="LinuxPoll"/> for event-driven I/O.</para>
/// <para>Both ends are created with <c>O_CLOEXEC</c> set. Dispose the pipe (or each end individually)
/// when done.</para>
/// </remarks>
public sealed class LinuxPipe : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// The read end of the pipe. Data written to <see cref="Writer"/> can be read from here.
    /// </summary>
    public LinuxFile Reader { get; }

    /// <summary>
    /// The write end of the pipe. Data written here can be read from <see cref="Reader"/>.
    /// </summary>
    public LinuxFile Writer { get; }

    private LinuxPipe(FileDescriptor readFd, FileDescriptor writeFd)
    {
        Reader = new LinuxFile(readFd);
        Writer = new LinuxFile(writeFd);
    }

    /// <summary>
    /// Creates a new pipe with both ends open.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe LinuxPipe Create()
    {
        FileDescriptor* fds = stackalloc FileDescriptor[2];
        pipe2(fds, LinuxFileFlags.CloseOnExec).ThrowIfError();
        return new LinuxPipe(fds[0], fds[1]);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Reader.Dispose();
        Writer.Dispose();
    }
}
