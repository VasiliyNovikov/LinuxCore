using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class CScriptTests
{
    [TestMethod]
    public void EvaluateInt32_Does_Not_Shadow_Expression_Identifiers()
    {
        Assert.AreEqual(1, CScript.EvaluateInt32("index(\"abc\", 'b') != NULL", "strings.h"));
    }
}