using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LinuxCore.Tests;

internal static unsafe partial class NativeProcess
{
    private const string ShellPath = "/bin/sh";
    private const string ShellCommand = "stdout=$1; stderr=$2; shift 2; exec \"$@\" >\"$stdout\" 2>\"$stderr\"";

    public static NativeProcessResult Run(params ReadOnlySpan<string> commandLine)
    {
        var standardOutputPath = Path.GetTempFileName();
        try
        {
            var standardErrorPath = Path.GetTempFileName();
            try
            {
                var status = SpawnAndWait([ShellPath, "-c", ShellCommand, nameof(NativeProcess), standardOutputPath, standardErrorPath, .. commandLine]);
                var standardOutput = File.ReadAllText(standardOutputPath);
                var standardError = File.ReadAllText(standardErrorPath);
                return DecodeResult(status, standardOutput, standardError);
            }
            finally
            {
                File.Delete(standardErrorPath);
            }
        }
        finally
        {
            File.Delete(standardOutputPath);
        }
    }

    private static int SpawnAndWait(ReadOnlySpan<string> arguments)
    {
        byte** argv = null;
        Span<nint> allocations = stackalloc nint[arguments.Length];
        try
        {
            argv = (byte**)NativeMemory.AllocZeroed((nuint)(arguments.Length + 1), (nuint)sizeof(nint));
            for (var i = 0; i < arguments.Length; i++)
            {
                ArgumentNullException.ThrowIfNull(arguments[i]);
                if (arguments[i].Contains('\0', StringComparison.Ordinal))
                    throw new ArgumentException("Process arguments cannot contain null characters.", nameof(arguments));

                allocations[i] = Marshal.StringToCoTaskMemUTF8(arguments[i]);
                argv[i] = (byte*)allocations[i];
            }

            int processId;
            int spawnResult;
            var libCHandle = NativeLibrary.Load(LinuxLibraries.LibC);
            try
            {
                var environSymbol = NativeLibrary.GetExport(libCHandle, "environ");
                spawnResult = posix_spawnp(&processId, argv[0], null, null, argv, *(byte***)environSymbol);
            }
            finally
            {
                NativeLibrary.Free(libCHandle);
            }
            if (spawnResult != 0)
                throw new LinuxException((LinuxErrorNumber)spawnResult);

            int status;
            int waitResult;
            do
            {
                waitResult = waitpid(processId, &status, 0);
            }
            while (waitResult == -1 && LinuxErrorNumber.Last == LinuxErrorNumber.InterruptedSystemCall);
            return waitResult == -1
                ? throw LinuxException.FromLastError()
                : waitResult == processId
                    ? status
                    : throw new InvalidOperationException($"waitpid returned process ID {waitResult} instead of {processId}");
        }
        finally
        {
            foreach (var allocation in allocations)
                if (allocation != 0)
                    Marshal.FreeCoTaskMem(allocation);
            NativeMemory.Free(argv);
        }
    }

    private static NativeProcessResult DecodeResult(int status, string standardOutput, string standardError)
    {
        const int signalMask = 0x7f;
        var signal = status & signalMask;
        if (signal == 0)
            return new NativeProcessResult((status >> 8) & 0xff, null, standardOutput, standardError);
        if (signal != signalMask)
            return new NativeProcessResult(128 + signal, signal, standardOutput, standardError);
        throw new InvalidOperationException($"Process stopped with wait status 0x{status:x}.");
    }

    // int posix_spawnp(pid_t *pid, const char *file, const posix_spawn_file_actions_t *file_actions, const posix_spawnattr_t *attrp, char *const argv[], char *const envp[]);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "posix_spawnp")]
    private static partial int posix_spawnp(int* pid, byte* file, void* fileActions, void* attributes, byte** argv, byte** envp);

    // pid_t waitpid(pid_t pid, int *wstatus, int options);
    [LibraryImport(LinuxLibraries.LibC, EntryPoint = "waitpid")]
    private static partial int waitpid(int pid, int* status, int options);
}

internal readonly record struct NativeProcessResult(int ExitCode, int? TerminationSignal, string StandardOutput, string StandardError);