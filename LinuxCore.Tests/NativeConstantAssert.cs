using System;
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

internal static class NativeConstantAssert
{
    public static void EnumValuesMatch<TEnum>((string Name, string Native)[] constants, params string[] headers)
        where TEnum : struct, Enum
    {
        Assert.AreSequenceEqual(Enum.GetNames<TEnum>(), constants.Select(static constant => constant.Name), SequenceOrder.InAnyOrder);
        var nativeValues = CScript.EvaluateInt64s([.. constants.Select(static constant => constant.Native)], headers);
        for (var i = 0; i < constants.Length; ++i)
            Assert.AreEqual(Convert.ToInt64(Enum.Parse<TEnum>(constants[i].Name), CultureInfo.InvariantCulture), nativeValues[i], constants[i].Name);
    }

    public static void ValuesMatch((string Name, long Managed, string Native)[] constants, params string[] headers)
    {
        var nativeValues = CScript.EvaluateInt64s([.. constants.Select(static constant => constant.Native)], headers);
        for (var i = 0; i < constants.Length; ++i)
            Assert.AreEqual(constants[i].Managed, nativeValues[i], constants[i].Name);
    }
}