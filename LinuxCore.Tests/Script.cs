using System;
using System.Diagnostics;

namespace LinuxCore.Tests;

internal static class Script
{
    public static string Run(params ReadOnlySpan<string> command)
    {
        var psi = new ProcessStartInfo(command[0], [.. command[1..]])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode == 0
            ? output
            : throw new ScriptException(process.ExitCode, process.StandardError.ReadToEnd().Trim());
    }
}

internal sealed class ScriptException(int exitCode, string error) : Exception($"Script exited with code {exitCode}: {error}");