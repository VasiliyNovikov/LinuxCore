namespace LinuxCore;

public interface IFileObject
{
    FileDescriptor Descriptor { get; }
}