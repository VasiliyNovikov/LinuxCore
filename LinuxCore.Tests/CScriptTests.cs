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

    [TestMethod]
    public void TryEvaluateInt32_ReturnsWhetherExpressionCompiles()
    {
        Assert.IsTrue(CScript.TryEvaluateInt32("1 + 1", out var value));
        Assert.AreEqual(2, value);
        Assert.IsFalse(CScript.TryEvaluateInt32("undefined_symbol", out _));
    }

    [TestMethod]
    public void IsDefined_ReturnsWhetherMacroIsDefined()
    {
        Assert.IsTrue(CScript.IsDefined("NULL", "stddef.h"));
        Assert.IsFalse(CScript.IsDefined("LINUXCORE_UNDEFINED_MACRO", "stddef.h"));
    }
}