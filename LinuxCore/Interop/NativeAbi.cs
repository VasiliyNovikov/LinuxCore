using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LinuxCore.Interop;

internal static class NativeAbi
{
    private const string GlibcMarker = "gnu_get_libc_version";
    private static readonly string[] MuslMarkers = ["__freadahead", "__freadptr", "__freadptrinc", "__fseterr"];

    public static readonly bool Is32Bit = IntPtr.Size == 4;
    public static readonly bool Is64Bit = IntPtr.Size == 8;
    public static readonly LibCImplementation Implementation = GetLibCImplementation();
    public static readonly bool IsGlibc = Implementation == LibCImplementation.Glibc;
    public static readonly bool IsMusl = Implementation == LibCImplementation.Musl;
    public static readonly bool IsLikelyQemuLinuxUser = GetIsLikelyQemuLinuxUser();

    private static LibCImplementation GetLibCImplementation()
    {
        var handle = NativeLibrary.Load(LinuxLibraries.LibC);
        try
        {
            if (NativeLibrary.TryGetExport(handle, GlibcMarker, out _))
                return LibCImplementation.Glibc;

            foreach (var marker in MuslMarkers)
                if (!NativeLibrary.TryGetExport(handle, marker, out _))
                    return LibCImplementation.Unknown;

            return LibCImplementation.Musl;
        }
        finally
        {
            NativeLibrary.Free(handle);
        }
    }

    private static bool GetIsLikelyQemuLinuxUser()
    {
        Span<byte> header = stackalloc byte[20];
        try
        {
            using var executable = new LinuxFile("/proc/thread-self/exe", LinuxFileFlags.ReadOnly);
            executable.ReadExactly(header);
        }
        catch (Exception exception) when (exception is IOException or LinuxException)
        {
            return true;
        }

        if (!header[..4].SequenceEqual("\u007fELF"u8) || header[6] != 1)
            return true;

        byte expectedClass;
        ushort expectedMachine;
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
                expectedClass = 1;
                expectedMachine = 3;
                break;
            case Architecture.X64:
                expectedClass = 2;
                expectedMachine = 62;
                break;
            case Architecture.Arm:
            case Architecture.Armv6:
                expectedClass = 1;
                expectedMachine = 40;
                break;
            case Architecture.Arm64:
                expectedClass = 2;
                expectedMachine = 183;
                break;
            case Architecture.S390x:
                expectedClass = 2;
                expectedMachine = 22;
                break;
            case Architecture.LoongArch64:
                expectedClass = 2;
                expectedMachine = 258;
                break;
            case Architecture.Ppc64le:
                expectedClass = 2;
                expectedMachine = 21;
                break;
            case Architecture.RiscV64:
                expectedClass = 2;
                expectedMachine = 243;
                break;
            default:
                return true;
        }

        var expectedData = BitConverter.IsLittleEndian ? (byte)1 : (byte)2;
        var machine = BitConverter.ToUInt16(header[18..]);
        return header[4] != expectedClass || header[5] != expectedData || machine != expectedMachine;
    }
}