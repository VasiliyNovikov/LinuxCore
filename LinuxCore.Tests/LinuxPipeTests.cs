using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxPipeTests
{
    [TestMethod]
    public void LinuxPipe_Create_Returns_Readable_And_Writable_Descriptors()
    {
        using var pipe = LinuxPipe.Create();

        Assert.IsNotNull(pipe.Reader);
        Assert.IsNotNull(pipe.Writer);

        // Write end should be writable; read end should not be readable yet.
        Assert.IsNull(LinuxPoll.Wait(pipe.Reader.Descriptor, LinuxPoll.Event.Readable, 0));
        Assert.AreEqual(LinuxPoll.Event.Writable, LinuxPoll.Wait(pipe.Writer.Descriptor, LinuxPoll.Event.Writable, 0));
    }

    [TestMethod]
    public void LinuxPipe_Write_Makes_Reader_Readable()
    {
        using var pipe = LinuxPipe.Create();

        var data = "hello pipe"u8.ToArray();
        pipe.Writer.Write(data);

        Assert.AreEqual(LinuxPoll.Event.Readable, LinuxPoll.Wait(pipe.Reader.Descriptor, LinuxPoll.Event.Readable, 0));
    }

    [TestMethod]
    public void LinuxPipe_Write_Read_RoundTrip()
    {
        using var pipe = LinuxPipe.Create();

        const string message = "LinuxPipe round-trip test";
        var written = Encoding.UTF8.GetBytes(message);
        pipe.Writer.Write(written);

        Span<byte> buffer = stackalloc byte[written.Length];
        var bytesRead = pipe.Reader.Read(buffer);

        Assert.AreEqual(written.Length, bytesRead);
        Assert.AreEqual(message, Encoding.UTF8.GetString(buffer));
    }

    [TestMethod]
    public void LinuxPipe_Writer_Close_Makes_Reader_HangUp()
    {
        using var pipe = LinuxPipe.Create();

        pipe.Writer.Dispose();

        // After the write end is closed, polling the read end should return HangUp.
        var events = LinuxPoll.Wait(pipe.Reader.Descriptor, LinuxPoll.Event.Readable | LinuxPoll.Event.HangUp, 0);
        Assert.IsNotNull(events);
        Assert.IsTrue((events!.Value & LinuxPoll.Event.HangUp) != 0);
    }
}
