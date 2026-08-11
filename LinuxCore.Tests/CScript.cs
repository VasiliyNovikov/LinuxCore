using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LinuxCore.Tests;

internal static class CScript
{
    public static int EvaluateInt32(string expression, params string[] headers) => checked((int)EvaluateInt64(expression, headers));
    public static nint EvaluateNInt(string expression, params string[] headers) => checked((nint)EvaluateInt64(expression, headers));

    public static unsafe long EvaluateInt64(string expression, params string[] headers)
    {
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"linuxcore-c-oracle-{Guid.NewGuid():N}");
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
                source.Append("#include <").Append(header).Append(">\n");
            source.Append("int64_t compute(void) { return (int64_t)(").Append(expression).Append("); }\n");

            var compiler = Environment.GetEnvironmentVariable("CC");
            if (string.IsNullOrEmpty(compiler))
                compiler = "cc";

            File.WriteAllText(sourcePath, source.ToString());
            Script.Run(compiler, "-std=c11", "-Wall", "-Wextra", "-Werror", "-shared", "-fPIC", sourcePath, "-o", libraryPath);

            var library = NativeLibrary.Load(libraryPath);
            try
            {
                var compute = (delegate* unmanaged[Cdecl]<long>)NativeLibrary.GetExport(library, "compute");
                return compute();
            }
            finally
            {
                NativeLibrary.Free(library);
            }
        }
        finally
        {
            try
            {
                File.Delete(sourcePath);
            }
            finally
            {
                File.Delete(libraryPath);
            }
        }
    }
}