using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            EnumValueMatches<TEnum>(constants[i].Name, nativeValues[i]);
    }

    public static void EnumValuesMatch<TEnum>((string Name, string Native)[] requiredConstants, (string Name, string Symbol)[] optionalConstants, params string[] headers)
        where TEnum : struct, Enum
    {
        Assert.AreSequenceEqual(Enum.GetNames<TEnum>(), requiredConstants.Select(static constant => constant.Name).Concat(optionalConstants.Select(static constant => constant.Name)), SequenceOrder.InAnyOrder);
        if (requiredConstants.Length > 0)
        {
            var requiredValues = CScript.EvaluateInt64s([.. requiredConstants.Select(static constant => constant.Native)], headers);
            for (var i = 0; i < requiredConstants.Length; ++i)
                EnumValueMatches<TEnum>(requiredConstants[i].Name, requiredValues[i]);
        }

        if (optionalConstants.Length > 0)
        {
            var definedValues = CScript.EvaluateDefinedInt32s([.. optionalConstants.Select(static constant => (constant.Symbol, "0"))], headers);
            var definedConstants = optionalConstants.Where((_, index) => definedValues[index].HasValue).ToArray();
            if (definedConstants.Length > 0)
            {
                var optionalValues = CScript.EvaluateInt64s([.. definedConstants.Select(static constant => constant.Symbol)], headers);
                for (var i = 0; i < definedConstants.Length; ++i)
                    EnumValueMatches<TEnum>(definedConstants[i].Name, optionalValues[i]);
            }
        }
    }

    public static void ValuesMatch((string Name, long Managed, string Native)[] constants, params string[] headers)
    {
        var nativeValues = CScript.EvaluateInt64s([.. constants.Select(static constant => constant.Native)], headers);
        for (var i = 0; i < constants.Length; ++i)
            Assert.AreEqual(constants[i].Managed, nativeValues[i], constants[i].Name);
    }

    public static void SizeMatches<T>(params string[] headers) where T : unmanaged
    {
        Assert.AreEqual(CScript.EvaluateInt32($"sizeof(struct {typeof(T).Name})", headers), Unsafe.SizeOf<T>());
    }

    public static void OffsetMatches<T>(string fieldName, params string[] headers) where T : unmanaged
    {
        Assert.AreEqual(CScript.EvaluateNInt($"offsetof(struct {typeof(T).Name}, {fieldName})", headers), Marshal.OffsetOf<T>(fieldName));
    }

    private static void EnumValueMatches<TEnum>(string name, long nativeValue)
        where TEnum : struct, Enum
    {
        var managedValue = Enum.Parse<TEnum>(name);
        var managedInt64 = Enum.GetUnderlyingType(typeof(TEnum)) == typeof(ulong)
            ? unchecked((long)Convert.ToUInt64(managedValue, CultureInfo.InvariantCulture))
            : Convert.ToInt64(managedValue, CultureInfo.InvariantCulture);
        Assert.AreEqual(managedInt64, nativeValue, name);
    }
}