using System;

namespace LinuxCore.Tests;

internal static class Script
{
    public static string Run(string fileName, params ReadOnlySpan<string> arguments)
    {
        using var standardOutputFile = new LinuxMemoryFile();
        using var standardErrorFile = new LinuxMemoryFile();
        using var process = LinuxProcess.Start(fileName, arguments, standardOutput: standardOutputFile.Descriptor, standardError: standardErrorFile.Descriptor);
        var status = process.Wait();

        if (status is not { ExitCode: { } exitCode, TerminationSignal: null })
            if (status.TerminationSignal is { } signal)
                exitCode = 128 + signal;
            else
                exitCode = 0;

        if (exitCode == 0)
        {
            standardOutputFile.Position = 0;
            return standardOutputFile.ReadAllText().Trim();
        }

        standardErrorFile.Position = 0;
        throw new ScriptException(exitCode, standardErrorFile.ReadAllText().Trim());
    }
}

internal sealed class ScriptException(int exitCode, string error) : Exception($"Script exited with code {exitCode}: {error}");