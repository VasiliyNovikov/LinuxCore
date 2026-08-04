using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class NativeProcessTests
{
    [TestMethod]
    public void NativeProcess_Captures_StandardOutput()
    {
        var result = NativeProcess.Run("/bin/sh", "-c", "printf 'hello\\n'");

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNull(result.TerminationSignal);
        Assert.AreEqual("hello\n", result.StandardOutput);
        Assert.AreEqual(string.Empty, result.StandardError);
    }

    [TestMethod]
    public void NativeProcess_Captures_StandardError_And_Nonzero_Exit()
    {
        var result = NativeProcess.Run("/bin/sh", "-c", "printf failure >&2; exit 23");

        Assert.AreEqual(23, result.ExitCode);
        Assert.IsNull(result.TerminationSignal);
        Assert.AreEqual(string.Empty, result.StandardOutput);
        Assert.AreEqual("failure", result.StandardError);
    }

    [TestMethod]
    public void NativeProcess_Preserves_Arguments_With_Shell_Metacharacters()
    {
        const string argument = "value with spaces; $(printf injected) ' \" *";

        var result = NativeProcess.Run("/bin/sh", "-c", "printf '%s' \"$1\"", nameof(NativeProcess), argument);

        Assert.AreEqual(0, result.ExitCode);
        Assert.AreEqual(argument, result.StandardOutput);
    }

    [TestMethod]
    public void NativeProcess_Decodes_TerminationSignal()
    {
        var result = NativeProcess.Run("/bin/sh", "-c", "kill -TERM $$");

        Assert.AreEqual(143, result.ExitCode);
        Assert.AreEqual(15, result.TerminationSignal);
    }

    [TestMethod]
    public void NativeProcess_Rejects_Embedded_Null_Argument()
    {
        _ = Assert.ThrowsExactly<ArgumentException>(() => NativeProcess.Run("/bin/true", "invalid\0argument"));
    }
}