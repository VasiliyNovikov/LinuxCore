using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LinuxCore.Tests;

internal static class CScript
{
    public static int EvaluateInt32(string expression, params string[] headers) => checked((int)EvaluateInt64(expression, headers));
    public static nint EvaluateNInt(string expression, params string[] headers) => checked((nint)EvaluateInt64(expression, headers));
    public static long EvaluateInt64(string expression, params string[] headers) => EvaluateInt64s([expression], headers)[0];

    public static int[] EvaluateInt32s(IReadOnlyList<string> expressions, params string[] headers)
    {
        var values = EvaluateInt64s(expressions, headers);
        var results = new int[values.Length];
        for (var i = 0; i < values.Length; ++i)
            results[i] = checked((int)values[i]);
        return results;
    }

    public static long[] EvaluateInt64s(IReadOnlyList<string> expressions, params string[] headers)
    {
        var constants = new (string? Symbol, string Expression)[expressions.Count];
        for (var i = 0; i < expressions.Count; ++i)
            constants[i] = (null, expressions[i]);

        var values = EvaluateInt64s(constants, headers);
        var results = new long[values.Length];
        for (var i = 0; i < values.Length; ++i)
            results[i] = values[i] ?? throw new InvalidOperationException("A required C expression was not evaluated.");
        return results;
    }

    public static int?[] EvaluateDefinedInt32s(IReadOnlyList<string> names, params string[] headers)
    {
        var constants = new (string Symbol, string Expression)[names.Count];
        for (var i = 0; i < names.Count; ++i)
            constants[i] = (names[i], names[i]);
        return EvaluateDefinedInt32s(constants, headers);
    }

    public static int?[] EvaluateDefinedInt32s(IReadOnlyList<(string Symbol, string Expression)> constants, params string[] headers)
    {
        var optionalConstants = new (string? Symbol, string Expression)[constants.Count];
        for (var i = 0; i < constants.Count; ++i)
            optionalConstants[i] = constants[i];

        var values = EvaluateInt64s(optionalConstants, headers);
        var results = new int?[values.Length];
        for (var i = 0; i < values.Length; ++i)
            if (values[i] is { } value)
                results[i] = checked((int)value);
        return results;
    }

    private static unsafe long?[] EvaluateInt64s((string? Symbol, string Expression)[] constants, params string[] headers)
    {
        ArgumentOutOfRangeException.ThrowIfZero(constants.Length);

        var identifier = Guid.NewGuid().ToString("N");
        var functionName = $"linuxcore_oracle_{identifier}";
        var indexName = $"{functionName}_index";
        var definedName = $"{functionName}_defined";
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"linuxcore-c-oracle-{identifier}");
        var sourcePath = $"{temporaryPath}.c";
        var libraryPath = $"{temporaryPath}.so";
        try
        {
            var source = new StringBuilder("""
                                           #define _GNU_SOURCE
                                           #define _FILE_OFFSET_BITS 64
                                           #define _TIME_BITS 64
                                           #include <stdint.h>
                                           #include <stddef.h>

                                           """);
            foreach (var header in headers)
                source.Append(CultureInfo.InvariantCulture,
                              $"""
                               #include <{header}>

                               """);
            source.Append(CultureInfo.InvariantCulture,
                          $$"""
                           int64_t {{functionName}}(int32_t {{indexName}}, int32_t *{{definedName}}) {
                               *{{definedName}} = 0;
                               switch ({{indexName}}) {

                           """);
            for (var i = 0; i < constants.Length; ++i)
            {
                var symbol = constants[i].Symbol;
                source.Append(CultureInfo.InvariantCulture,
                              $"""
                                   case {i}:

                               """);
                if (symbol is not null)
                    source.Append(CultureInfo.InvariantCulture,
                                  $"""
                                   #ifdef {symbol}

                                   """);
                source.Append(CultureInfo.InvariantCulture,
                              $"""
                                       *{definedName} = 1;
                                       return (int64_t)({constants[i].Expression});

                               """);
                if (symbol is not null)
                    source.Append("""
                                  #else
                                              return 0;
                                  #endif

                                  """);
            }
            source.Append("""
                                  default:
                                      return 0;
                              }
                          }

                          """);

            var compiler = Environment.GetEnvironmentVariable("CC") ?? "cc";
            File.WriteAllText(sourcePath, source.ToString());
            try
            {
                Script.Run(compiler, "-std=c11", "-Wall", "-Wextra", "-Werror", "-shared", "-fPIC", sourcePath, "-o", libraryPath);
            }
            finally
            {
                File.Delete(sourcePath);
            }

            var library = NativeLibrary.Load(libraryPath);
            try
            {
                var compute = (delegate* unmanaged[Cdecl]<int, int*, long>)NativeLibrary.GetExport(library, functionName);
                var results = new long?[constants.Length];
                for (var i = 0; i < results.Length; ++i)
                {
                    var isDefined = 0;
                    var value = compute(i, &isDefined);
                    if (isDefined != 0)
                        results[i] = value;
                }
                return results;
            }
            finally
            {
                NativeLibrary.Free(library);
            }
        }
        finally
        {
            File.Delete(libraryPath);
        }
    }
}