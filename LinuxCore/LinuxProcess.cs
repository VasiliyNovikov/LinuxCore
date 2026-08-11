using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static LinuxCore.Interop.File;
using static LinuxCore.Interop.Process;

namespace LinuxCore;

/// <summary>
/// Represents a child process and owns its Linux process file descriptor.
/// </summary>
/// <remarks>
/// The caller must successfully call <see cref="Wait()"/> before disposal to reap the child.
/// Disposal closes only the process file descriptor and does not terminate or reap the process.
/// Concurrent waits, concurrent disposal, and external reaping of the child are unsupported.
/// </remarks>
public sealed unsafe class LinuxProcess : FileObject
{
    /// <summary>
    /// Indicates whether <c>pidfd_open(2)</c> is available to the current process.
    /// </summary>
    public static readonly bool IsPidFdSupported = GetIsPidFdSupported();

    private static readonly byte*** Environ = (byte***)NativeLibrary.GetExport(NativeLibrary.Load(LinuxLibraries.LibC), "environ");

    private (int? ExitCode, int? TerminationSignal)? _exitStatus;

    private LinuxProcess(int id, FileDescriptor descriptor)
        : base(descriptor)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the native process identifier.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Starts a child process using <c>posix_spawnp(3)</c>.
    /// </summary>
    /// <param name="fileName">The executable path or file name. It is also used as <c>argv[0]</c>.</param>
    /// <param name="arguments">Arguments following <c>argv[0]</c>.</param>
    /// <param name="standardInput">A descriptor to map to standard input, or <see langword="null"/> to inherit it.</param>
    /// <param name="standardOutput">A descriptor to map to standard output, or <see langword="null"/> to inherit it.</param>
    /// <param name="standardError">A descriptor to map to standard error, or <see langword="null"/> to inherit it.</param>
    /// <param name="environmentVariables">
    /// The complete child environment, or <see langword="null"/> to inherit the native process environment.
    /// </param>
    /// <remarks>
    /// On supported glibc and musl versions, <c>posix_spawnp</c> searches the native parent
    /// <c>PATH</c>, not a replacement <paramref name="environmentVariables"/> value. Pass a path
    /// containing a slash when executable lookup must not use the parent environment.
    ///
    /// The native environment and supplied descriptors must not be changed or closed until this
    /// method returns. Managed <see cref="Environment.SetEnvironmentVariable(string, string?)"/>
    /// changes are not guaranteed to update the native environment inherited by this method.
    /// </remarks>
    public static LinuxProcess Start(string fileName,
                                     ReadOnlySpan<string> arguments = default,
                                     FileDescriptor? standardInput = null,
                                     FileDescriptor? standardOutput = null,
                                     FileDescriptor? standardError = null,
                                     IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        Validate(fileName, arguments, environmentVariables);
        if (!IsPidFdSupported)
            throw new PlatformNotSupportedException("pidfd_open is not supported by the current environment");

        using var argv = new NativeStringVector(checked(arguments.Length + 1));
        argv.Add(fileName);
        foreach (var argument in arguments)
            argv.Add(argument);

        using var nativeEnvironment = environmentVariables is null ? default : new NativeStringVector(environmentVariables.Count);
        byte** environmentPointer;
        if (environmentVariables is null)
            environmentPointer = *Environ;
        else
        {
            foreach (var (name, value) in environmentVariables)
                nativeEnvironment.Add($"{name}={value}");
            environmentPointer = nativeEnvironment.Pointer;
        }

        return Spawn(argv.Pointer[0], argv.Pointer, environmentPointer, standardInput, standardOutput, standardError);
    }

    /// <summary>
    /// Waits until the process exits and reaps it.
    /// </summary>
    /// <returns>
    /// The exit code for normal termination or the signal number for signal termination. Exactly
    /// one tuple member is non-null for values returned by this method.
    /// </returns>
    public (int? ExitCode, int? TerminationSignal) Wait() => Wait(LinuxCancellationToken.None);

    /// <summary>
    /// Waits until the process exits and reaps it, or until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">The cancellation-aware native wait token.</param>
    /// <returns>
    /// The exit code for normal termination or the signal number for signal termination. Exactly
    /// one tuple member is non-null for values returned by this method.
    /// </returns>
    /// <remarks>
    /// Cancellation stops only this wait and does not terminate or reap the child. Call this method
    /// again to reap the process. If process exit and cancellation race, either result may win; when
    /// both descriptors are ready in the same poll, cancellation wins.
    /// </remarks>
    public (int? ExitCode, int? TerminationSignal) Wait(LinuxCancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);
        ThrowIfDisposed();
        if (_exitStatus is { } completedStatus)
            return completedStatus;

        while (true)
        {
            _ = cancellationToken.Wait(this, LinuxPoll.Event.Readable);
            while (true)
            {
                int nativeStatus;
                var waitResult = waitpid(Id, &nativeStatus, 0);
                if (waitResult.IsError)
                {
                    var error = LinuxErrorNumber.Last;
                    if (error != LinuxErrorNumber.InterruptedSystemCall)
                        throw new LinuxException(error);

                    cancellationToken.ThrowIfCancellationRequested();
                    continue;
                }

                if (TryDecodeStatus(nativeStatus, out var status))
                {
                    _exitStatus = status;
                    return status;
                }

                cancellationToken.ThrowIfCancellationRequested();
                break;
            }
        }
    }

    private static LinuxProcess Spawn(byte* fileName, byte** argv, byte** environment, FileDescriptor? standardInput, FileDescriptor? standardOutput, FileDescriptor? standardError)
    {
        Unsafe.SkipInit(out posix_spawn_file_actions_t actions);
        var actionsInitialized = false;
        var childCreated = false;
        var processId = 0;
        Span<FileDescriptor> temporaryDescriptors = stackalloc FileDescriptor[3];
        var temporaryDescriptorsCount = 0;
        try
        {
            posix_spawn_file_actions_init(&actions).ThrowIfError();
            actionsInitialized = true;

            AddRedirection(&actions, standardInput, FileDescriptor.StandardInput, temporaryDescriptors, ref temporaryDescriptorsCount);
            AddRedirection(&actions, standardOutput, FileDescriptor.StandardOutput, temporaryDescriptors, ref temporaryDescriptorsCount);
            AddRedirection(&actions, standardError, FileDescriptor.StandardError, temporaryDescriptors, ref temporaryDescriptorsCount);

            Span<int> descriptorsToClose = stackalloc int[3];
            var descriptorsToCloseCount = 0;
            AddOriginalDescriptorClose(&actions, standardInput, descriptorsToClose, ref descriptorsToCloseCount);
            AddOriginalDescriptorClose(&actions, standardOutput, descriptorsToClose, ref descriptorsToCloseCount);
            AddOriginalDescriptorClose(&actions, standardError, descriptorsToClose, ref descriptorsToCloseCount);

            posix_spawnp(&processId, fileName, &actions, null, argv, environment).ThrowIfError();
            childCreated = true;
        }
        catch
        {
            if (childCreated)
                TerminateAndReap(processId);
            throw;
        }
        finally
        {
            for (var i = 0; i < temporaryDescriptorsCount; ++i)
                temporaryDescriptors[i].Close();
            if (actionsInitialized)
                posix_spawn_file_actions_destroy(&actions).ThrowIfError();
        }

        var descriptorResult = pidfd_open(processId, 0);
        if (descriptorResult.IsError)
        {
            var error = LinuxErrorNumber.Last;
            TerminateAndReap(processId);
            throw new LinuxException(error);
        }

        return new(processId, descriptorResult);
    }

    private static void AddRedirection(posix_spawn_file_actions_t* actions, FileDescriptor? source, FileDescriptor target, Span<FileDescriptor> temporaryDescriptors, ref int temporaryCount)
    {
        if (source is not { } sourceDescriptor)
            return;

        var temporaryDescriptor = new FileDescriptor(fcntl(sourceDescriptor, F_DUPFD_CLOEXEC, FileDescriptor.StandardError.Value + 1).ThrowIfError());
        temporaryDescriptors[temporaryCount++] = temporaryDescriptor;
        posix_spawn_file_actions_adddup2(actions, temporaryDescriptor, target).ThrowIfError();
    }

    private static void AddOriginalDescriptorClose(posix_spawn_file_actions_t* actions, FileDescriptor? descriptor, Span<int> descriptorsToClose, ref int descriptorToCloseCount)
    {
        if (descriptor is not { } source || source.Value <= FileDescriptor.StandardError.Value || descriptorsToClose[..descriptorToCloseCount].Contains(source.Value))
            return;

        descriptorsToClose[descriptorToCloseCount++] = source.Value;
        posix_spawn_file_actions_addclose(actions, source).ThrowIfError();
    }

    private static void TerminateAndReap(int processId)
    {
        _ = kill(processId, SIGKILL);
        while (true)
        {
            int nativeStatus;
            if (waitpid(processId, &nativeStatus, 0).IsError)
            {
                if (LinuxErrorNumber.Last == LinuxErrorNumber.InterruptedSystemCall) 
                    continue;
                return;
            }

            if (TryDecodeStatus(nativeStatus, out _))
                return;
        }
    }

    private static bool TryDecodeStatus(int nativeStatus, out (int? ExitCode, int? TerminationSignal) status)
    {
        const int SignalMask = 0x7f;
        var signal = nativeStatus & SignalMask;
        if (signal == 0)
        {
            status = ((nativeStatus >> 8) & 0xff, null);
            return true;
        }
        if (signal != SignalMask)
        {
            status = (null, signal);
            return true;
        }

        status = default;
        return false;
    }

    private static bool GetIsPidFdSupported()
    {
        var result = pidfd_open(Environment.ProcessId, 0);
        if (result.IsError)
            return false;

        result.ThrowIfError().Close();
        return true;
    }

    private static void Validate(string fileName, ReadOnlySpan<string> arguments, IReadOnlyDictionary<string, string>? environmentVariables)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        if (fileName.Length == 0)
            throw new ArgumentException("The process file name cannot be empty.", nameof(fileName));
        if (fileName.Contains('\0', StringComparison.Ordinal))
            throw new ArgumentException("The process file name cannot contain null characters.", nameof(fileName));

        foreach (var argument in arguments)
        {
            ArgumentNullException.ThrowIfNull(argument, nameof(arguments));
            if (argument.Contains('\0', StringComparison.Ordinal))
                throw new ArgumentException("Process arguments cannot contain null characters.", nameof(arguments));
        }

        if (environmentVariables is not null)
            foreach (var (name, value) in environmentVariables)
            {
                ArgumentNullException.ThrowIfNull(name, nameof(environmentVariables));
                if (name.Length == 0 || name.Contains('=', StringComparison.Ordinal) || name.Contains('\0', StringComparison.Ordinal))
                    throw new ArgumentException("Environment variable names must be nonempty and cannot contain '=' or null characters.", nameof(environmentVariables));
                ArgumentNullException.ThrowIfNull(value, nameof(environmentVariables));
                if (value.Contains('\0', StringComparison.Ordinal))
                    throw new ArgumentException("Environment variable values cannot contain null characters.", nameof(environmentVariables));
            }
    }

    private struct NativeStringVector(int capacity) : IDisposable
    {
        private readonly byte** _items = (byte**)NativeMemory.AllocZeroed(checked((nuint)(capacity + 1)), (nuint)sizeof(byte*));

        public int Count { get; private set; }

        public readonly byte** Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _items;
        }

        public void Add(string value)
        {
            if (Count == capacity)
                throw new InvalidOperationException("The string collection changed while the process was being started.");
            _items[Count++] = (byte*)Marshal.StringToCoTaskMemUTF8(value);
        }

        public readonly void Dispose()
        {
            if (_items is null)
                return;
            for (var i = 0; i < Count; ++i)
                Marshal.FreeCoTaskMem((nint)_items[i]);
            NativeMemory.Free(_items);
        }
    }
}