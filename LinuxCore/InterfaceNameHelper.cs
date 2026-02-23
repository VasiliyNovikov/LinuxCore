using System.Runtime.InteropServices.Marshalling;

using LinuxCore.Interop;

namespace LinuxCore;

public static unsafe class InterfaceNameHelper
{
    public const int MaxLength = NetIf.IF_NAMESIZE - 1;

    public static int GetIndex(string name)
    {
        var index = NetIf.if_nametoindex(name);
        return index == 0 ? throw LinuxException.FromLastError() : (int)index;
    }

    public static string GetName(int index)
    {
        var namePtr = NetIf.if_indextoname((uint)index, stackalloc byte[NetIf.IF_NAMESIZE]);
        return namePtr is null
            ? throw LinuxException.FromLastError()
            : Utf8StringMarshaller.ConvertToManaged(namePtr)!;
    }
}