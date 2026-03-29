using System.Diagnostics.CodeAnalysis;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
public enum LinuxSeekOrigin
{
    Set     = 0, // SEEK_SET: Seek from beginning of file
    Current = 1, // SEEK_CUR: Seek from current position
    End     = 2  // SEEK_END: Seek from end of file
}