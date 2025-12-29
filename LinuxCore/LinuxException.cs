using System;

namespace LinuxCore;

public class LinuxException(LinuxErrorNumber errorNumber)
    : Exception($"{(int)errorNumber} - {errorNumber.Message}")
{
    public LinuxErrorNumber ErrorNumber => errorNumber;

    public static LinuxException FromLastError() => new(LinuxErrorNumber.Last);
}