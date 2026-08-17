using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NativeProcess = LinuxCore.Interop.Process;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxProcessTests
{
    [TestMethod]
    public void LinuxProcess_Constants_Match_Current_Platform_Headers()
    {
        Assert.AreEqual(NativeProcess.SIGKILL, CScript.EvaluateInt32("SIGKILL", "signal.h"));
    }

    [TestMethod]
    public void IsPidFdSupported_Is_True_On_Supported_Kernel() => Assert.IsTrue(LinuxProcess.IsPidFdSupported);

    [TestMethod]
    public void Start_Preserves_Arguments_And_Returns_ExitCode()
    {
        const string argument = "value with spaces; $(printf injected) ' \" *";
        var (exitCode, terminationSignal, standardOutput) = RunWithOutput("/bin/sh", ["-c", "printf '%s' \"$1\"; exit 23", nameof(LinuxProcessTests), argument]);

        Assert.AreEqual(23, exitCode);
        Assert.IsNull(terminationSignal);
        Assert.AreEqual(argument, standardOutput);
    }

    [TestMethod]
    public void Wait_Returns_TerminationSignal()
    {
        using var process = LinuxProcess.Start("/bin/sh", ["-c", "kill -TERM $$"]);
        var (exitCode, terminationSignal) = process.Wait();

        Assert.IsNull(exitCode);
        Assert.AreEqual(15, terminationSignal);
    }

    [TestMethod]
    public void Start_Inherits_Native_Environment()
    {
        var (exitCode, _, standardOutput) = RunWithOutput("/usr/bin/env", []);

        Assert.AreEqual(0, exitCode);
        Assert.Contains("PATH=", standardOutput);
    }

    [TestMethod]
    public void Start_Replaces_Environment()
    {
        var environment = new Dictionary<string, string> { ["LINUXCORE_PROCESS_TEST"] = "expected" };
        var (_, _, standardOutput) = RunWithOutput("/usr/bin/env", [], environment);

        Assert.AreEqual("LINUXCORE_PROCESS_TEST=expected\n", standardOutput);
    }

    [TestMethod]
    public void Start_Searches_Parent_Path_Not_Replacement_Path()
    {
        var environment = new Dictionary<string, string> { ["PATH"] = "/path/that/does/not/exist" };
        using var process = LinuxProcess.Start("true", environmentVariables: environment);

        Assert.AreEqual(0, process.Wait().ExitCode);
    }

    [TestMethod]
    public void Start_Redirects_All_Standard_Streams()
    {
        using var standardInput = new LinuxMemoryFile();
        using var standardOutput = new LinuxMemoryFile();
        using var standardError = new LinuxMemoryFile();
        standardInput.WriteAllText("input\n");
        standardInput.Position = 0;
        using var process = LinuxProcess.Start("/bin/sh",
                                               ["-c", "read value; printf 'out:%s' \"$value\"; printf 'err:%s' \"$value\" >&2"],
                                               standardInput: standardInput.Descriptor,
                                               standardOutput: standardOutput.Descriptor,
                                               standardError: standardError.Descriptor);

        Assert.AreEqual(0, process.Wait().ExitCode);
        standardOutput.Position = 0;
        standardError.Position = 0;
        Assert.AreEqual("out:input", standardOutput.ReadAllText());
        Assert.AreEqual("err:input", standardError.ReadAllText());
    }

    [TestMethod]
    public void Start_Maps_One_Descriptor_To_Multiple_Streams_Without_Leaking_Original()
    {
        using var output = new LinuxMemoryFile();
        var originalDescriptor = output.Descriptor.ToString();
        using var process = LinuxProcess.Start("/bin/sh",
                                               ["-c", "test ! -e \"/proc/self/fd/$1\" || exit 91; printf out; printf err >&2", nameof(LinuxProcessTests), originalDescriptor],
                                               standardOutput: output.Descriptor,
                                               standardError: output.Descriptor);

        Assert.AreEqual(0, process.Wait().ExitCode);
        output.Position = 0;
        Assert.AreEqual("outerr", output.ReadAllText());
    }

    [TestMethod]
    public void Wait_Cancellation_Is_Retryable()
    {
        using var process = LinuxProcess.Start("/bin/sh", ["-c", "sleep 0.2"]);
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
        using var cancellationToken = new LinuxCancellationToken(cancellationSource.Token);

        Assert.ThrowsExactly<OperationCanceledException>(() => process.Wait(cancellationToken));
        Assert.AreEqual(0, process.Wait().ExitCode);
    }

    [TestMethod]
    public void Wait_Returns_Cached_Status_Before_Checking_Cancellation()
    {
        using var process = LinuxProcess.Start("/bin/true");
        var expected = process.Wait();
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        using var cancellationToken = new LinuxCancellationToken(cancellationSource.Token);

        Assert.AreEqual(expected, process.Wait(cancellationToken));
    }

    [TestMethod]
    public void Wait_Throws_After_Disposal()
    {
        var process = LinuxProcess.Start("/bin/true");
        _ = process.Wait();
        process.Dispose();

        _ = Assert.ThrowsExactly<ObjectDisposedException>(() => process.Wait());
    }

    [TestMethod]
    public void Start_Rejects_Invalid_Inputs()
    {
        _ = Assert.ThrowsExactly<ArgumentException>(() => LinuxProcess.Start(string.Empty));
        _ = Assert.ThrowsExactly<ArgumentException>(() => LinuxProcess.Start("invalid\0file"));
        _ = Assert.ThrowsExactly<ArgumentException>(() => LinuxProcess.Start("/bin/true", ["invalid\0argument"]));
        _ = Assert.ThrowsExactly<ArgumentException>(() => LinuxProcess.Start("/bin/true", environmentVariables: new Dictionary<string, string> { ["INVALID=NAME"] = "value" }));
        _ = Assert.ThrowsExactly<ArgumentException>(() => LinuxProcess.Start("/bin/true", environmentVariables: new Dictionary<string, string> { ["VALID"] = "invalid\0value" }));
        _ = Assert.ThrowsExactly<LinuxException>(() => LinuxProcess.Start("/bin/true", standardOutput: new FileDescriptor(-1)));
    }

    [TestMethod]
    public void Wait_Rejects_Null_CancellationToken()
    {
        using var process = LinuxProcess.Start("/bin/true");

        _ = Assert.ThrowsExactly<ArgumentNullException>(() => process.Wait(null!));
        Assert.AreEqual(0, process.Wait().ExitCode);
    }

    [TestMethod]
    public void Start_Does_Not_Leak_Parent_Descriptors()
    {
        var descriptorCount = Directory.GetFiles("/proc/self/fd").Length;

        for (var i = 0; i < 10; ++i)
        {
            using var process = LinuxProcess.Start("/bin/true");
            _ = process.Wait();
        }

        Assert.HasCount(descriptorCount, Directory.GetFiles("/proc/self/fd"));
    }

    private static (int? ExitCode, int? TerminationSignal, string StandardOutput) RunWithOutput(string fileName, ReadOnlySpan<string> arguments, IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        using var output = new LinuxMemoryFile();
        using var process = LinuxProcess.Start(fileName, arguments, standardOutput: output.Descriptor, environmentVariables: environmentVariables);
        var (exitCode, terminationSignal) = process.Wait();
        output.Position = 0;
        return (exitCode, terminationSignal, output.ReadAllText());
    }
}