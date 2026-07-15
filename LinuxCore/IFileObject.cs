namespace LinuxCore;

/// <summary>
/// Exposes a Linux file descriptor owned or borrowed by an object.
/// </summary>
public interface IFileObject
{
    /// <summary>
    /// Gets the object's numeric descriptor without duplicating it or retaining its lifetime.
    /// </summary>
    /// <remarks>
    /// The owner must remain strongly reachable and undisposed, and callers must prevent concurrent
    /// disposal or external closure while using this value. Use <see cref="System.GC.KeepAlive(object)"/>
    /// when necessary, or <see cref="FileDescriptor.Clone"/> when an independent lifetime is required.
    /// </remarks>
    FileDescriptor Descriptor { get; }
}