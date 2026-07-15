using System;

namespace LinuxCore.Tests;

internal static class Script
{
    public static string Run(params ReadOnlySpan<string> command)
    {
        var result = NativeProcess.Run(command);
        return result.ExitCode == 0
            ? result.StandardOutput.Trim()
            : throw new ScriptException(result.ExitCode, result.StandardError.Trim());
    }
}

internal sealed class ScriptException(int exitCode, string error) : Exception($"Script exited with code {exitCode}: {error}");