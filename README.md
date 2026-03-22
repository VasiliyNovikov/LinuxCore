# LinuxCore

A thin, AOT-compatible .NET wrapper around Linux libc APIs. Provides ergonomic, low-allocation C# abstractions over raw syscalls for files, sockets, events, polling, scheduling, and more.

[![LinuxCore release](https://img.shields.io/nuget/v/LinuxCore)](https://www.nuget.org/packages/LinuxCore/)
[![LinuxCore download count](https://img.shields.io/nuget/dt/LinuxCore)](https://www.nuget.org/packages/LinuxCore/)

## Features

- **File I/O** — `LinuxFile` for `open`/`read`/`write`/`fstat` with `Span<byte>` support
- **Memory Files** — `LinuxMemoryFile` for `memfd_create` and memfd seals via `fcntl`
- **Events & Semaphores** — `LinuxEvent` and `LinuxSemaphore` backed by `eventfd`
- **Polling** — `LinuxPoll` for `poll()`-based readiness notification
- **Clocks** — `LinuxClock` for nanosecond-precision monotonic timestamps
- **Scheduling** — `LinuxScheduler` for `sched_setscheduler` (FIFO, RR, etc.)
- **Resource Limits** — `LinuxResourceLimit` for `getrlimit`/`setrlimit`
- **Cancellation** — `LinuxCancellationToken` bridges `CancellationToken` to native poll
- **Sockets** — `LinuxSocketBase` for raw socket operations

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
       ├─ LinuxSocketBase
       └─ LinuxFile → LinuxMemoryFile
```

## License

[MIT](LICENSE)
