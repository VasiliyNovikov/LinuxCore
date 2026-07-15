using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LinuxCore.Tests;

internal static class CScript
{
    public static int EvaluateInt32(string expression, params string[] headers) => checked((int)EvaluateInt64(expression, headers));

    public static unsafe long EvaluateInt64(string expression, params string[] headers)
    {
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"linuxcore-c-oracle-{Guid.NewGuid():N}");
        var sourcePath = $"{temporaryPath}.c";
        var libraryPath = $"{temporaryPath}.so";
        try
        {
            var source = new StringBuilder("#define _GNU_SOURCE\n#define _FILE_OFFSET_BITS 64\n#define _TIME_BITS 64\n#include <stdint.h>\n");
            foreach (var header in headers)
                source.Append("#include <").Append(header).Append(">\n");
            source.Append("int64_t compute(void) { return (int64_t)(").Append(expression).Append("); }\n");

            var compiler = Environment.GetEnvironmentVariable("CC");
            if (string.IsNullOrEmpty(compiler))
                compiler = "cc";

            File.WriteAllText(sourcePath, source.ToString());
            var result = NativeProcess.Run(compiler, "-std=c11", "-Wall", "-Wextra", "-Werror", "-shared", "-fPIC", sourcePath, "-o", libraryPath);
            if (result.ExitCode != 0)
                throw new InvalidOperationException($"{compiler} exited with code {result.ExitCode}: {result.StandardError}\n{result.StandardOutput}");

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