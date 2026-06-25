# LinuxCore

A thin, AOT-compatible .NET wrapper around Linux libc APIs. Provides ergonomic, low-allocation C# abstractions over raw syscalls for files, sockets, events, polling, scheduling, and more.

[![LinuxCore release](https://img.shields.io/nuget/v/LinuxCore)](https://www.nuget.org/packages/LinuxCore/)
[![LinuxCore download count](https://img.shields.io/nuget/dt/LinuxCore)](https://www.nuget.org/packages/LinuxCore/)

## Features

- **File I/O** — `LinuxFile` for `open`/`read`/`write`/`seek`/`fstat` with `Span<byte>` support
- **Memory Files** — `LinuxMemoryFile` for `memfd_create` and memfd seals via `fcntl`
- **Memory Maps** — `LinuxMemoryMap`/`LinuxReadOnlyMemoryMap` for `mmap`/`munmap` with `Span<byte>`/`Memory<byte>` access
- **Events & Semaphores** — `LinuxEvent` and `LinuxSemaphore` backed by `eventfd`
- **Polling** — `LinuxPoll` for `poll()`-based readiness notification
- **Clocks** — `LinuxClock` for nanosecond-precision monotonic, boottime, CPU-time, and wall-clock (`CLOCK_REALTIME`) timestamps and high-precision sleeps
- **Scheduling** — `LinuxScheduler` for `sched_setscheduler`/`sched_getscheduler` (FIFO, RR, etc.)
- **Resource Limits** — `LinuxResourceLimit` for `getrlimit`/`setrlimit`
- **Cancellation** — `LinuxCancellationToken` bridges `CancellationToken` to native poll
- **Sockets** — `UnixSocket` for AF_UNIX sockets and `LinuxSocketBase` for shared raw socket operations
- **System Configuration** — `SystemConfiguration` for `sysconf()` queries (page size, max open files, etc.)
- **Users & Groups** — `LinuxUser` and `LinuxGroup` for passwd/group lookups by name or ID

## Requirements

- Linux (the library is annotated with `[SupportedOSPlatform("linux")]`)
- .NET 10+

## Architecture

The library has two layers:

- **`LinuxCore.Interop`** — internal P/Invoke declarations using source-generated `[LibraryImport]`. Each file maps to a libc subsystem.
- **`LinuxCore`** — public API types wrapping the interop layer with safe-ish, idiomatic C# APIs.

File-descriptor-owning types follow this hierarchy:

```
NativeObject (IDisposable + Finalizer)
  └─ FileObject (FileDescriptor, shared I/O and descriptor-control helpers)
       ├─ LinuxEventBase → LinuxEvent, LinuxSemaphore
       ├─ LinuxSocketBase → UnixSocket
       └─ LinuxFile → LinuxMemoryFile

LinuxSecurityObject (Id + Name)
  ├─ LinuxUser
  └─ LinuxGroup
```

## Operational notes

- `LinuxScheduler.Set(...)` and some `LinuxResourceLimit.Set(...)` calls may require root privileges or Linux capabilities such as `CAP_SYS_NICE` / `CAP_SYS_RESOURCE`.
- AF_UNIX pathname sockets are subject to the kernel `sockaddr_un.sun_path` limit (108 bytes on Linux).
- NativeAOT compatibility is exercised in CI on both glibc (Ubuntu) and musl (Alpine) runners via a small smoke app.

## License

[MIT](LICENSE)
