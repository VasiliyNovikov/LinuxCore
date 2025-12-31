using System;
using System.Diagnostics.CodeAnalysis;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[Flags]
public enum LinuxFileMode
{
    None         = 0x0000, // No permissions
    OtherExecute = 0x0001,  // S_IXOTH: Execute/Search by others
    OtherWrite   = 0x0002, // S_IWOTH: Write by others
    OtherRead    = 0x0004, // S_IROTH: Read by others
    GroupExecute = 0x0008, // S_IXGRP: Execute/Search by group
    GroupWrite   = 0x0010, // S_IWGRP: Write by group
    GroupRead    = 0x0020, // S_IRGRP: Read by group
    UserExecute  = 0x0040, // S_IXUSR: Execute/Search by owner
    UserWrite    = 0x0080, // S_IWUSR: Write by owner
    UserRead     = 0x0100, // S_IRUSR: Read by owner
    StickyBit    = 0x0200, // S_ISVTX: Sticky bit
    SetGroup     = 0x0400, // S_ISGID: Set group ID on execution
    SetUser      = 0x0800  // S_ISUID: Set user ID on execution
}