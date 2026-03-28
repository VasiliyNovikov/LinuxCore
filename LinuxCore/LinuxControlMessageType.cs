using System.Diagnostics.CodeAnalysis;

using LinuxCore.Interop;

namespace LinuxCore;

[SuppressMessage("Style", "IDE0055:Fix formatting")]
public enum LinuxControlMessageType
{
    ScmRights       = Socket.SCM_RIGHTS,       // SOL_SOCKET: Pass file descriptors
    ScmCredentials  = Socket.SCM_CREDENTIALS   // SOL_SOCKET: Pass process credentials
}