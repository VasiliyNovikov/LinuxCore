# LinuxCore

A thin, AOT-compatible .NET wrapper around Linux libc APIs. Provides ergonomic, low-allocation C# abstractions over raw syscalls for files, sockets, events, polling, scheduling, and more.

[![LinuxCore release](https://img.shields.io/nuget/v/LinuxCore)](https://www.nuget.org/packages/LinuxCore/)
[![LinuxCore download count](https://img.shields.io/nuget/dt/LinuxCore)](https://www.nuget.org/packages/LinuxCore/)

## Features

- **File I/O** — `LinuxFile` for `open`/`read`/`write`/`seek`/`statx` with `Span<byte>` support
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
- **Processes** — `LinuxProcess` for `posix_spawnp`, environment and standard-stream redirection, and cancellable pidfd waiting

## Requirements

- Linux (the library is annotated with `[SupportedOSPlatform("linux")]`)
- .NET 10+
- glibc 2.28+ (2.34+ on 32-bit architectures) or musl 1.2.5+ for `statx` and time64 entry points
- Linux 5.3+ for `LinuxProcess` pidfd waiting

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
- `FileDescriptor` is an allocation-free, non-owning value. Copying it or reading `FileObject.Descriptor` does not duplicate the descriptor or retain its lifetime; use `Clone()` for an independent descriptor. `FileObject` intentionally avoids per-operation lifetime leasing, so callers must keep the owner strongly reachable and prevent concurrent disposal or external closure while operations or raw descriptors are in use. Closed or stale descriptor values may refer to unrelated resources after Linux recycles the number.
- Generic `ReceiveMessage` methods return only the requested control-message type and do not close resources from nonmatching messages. On Unix sockets, use `ReceiveFileDescriptors` when an `SCM_RIGHTS` message may be present.
- On QEMU linux-user, `ReceiveFileDescriptors` requires `SO_PASSPIDFD` and `SO_PASSSEC` to be disabled. Either option can cause host-side descriptors to be omitted during ancillary conversion and remain open. LinuxCore does not enable either option.
- `LinuxFileFlags` and `LinuxMemoryMapFlags` values are stable managed tokens. LinuxCore translates architecture-dependent file flags on Arm and PowerPC, and mapping flags on PowerPC, before calling libc.
- CI runs the full test suite on every target. Alpine x64 and arm64 plus Arm32 glibc and musl build and test inside native SDK containers; ppc64le, s390x, RISC-V64, and LoongArch64 run host-built portable test output under QEMU. All targets gate publishing. The RISC-V64 and LoongArch64 matrix entries use community runtimes because Microsoft does not publish supported .NET 10 runtimes for those architectures or support QEMU execution.
- NativeAOT compatibility is exercised in CI on both glibc (Ubuntu) and musl (Alpine) runners via a small smoke app.
- `LinuxProcess` cancellation stops only the wait; it does not terminate the child. Callers must successfully wait and reap the child before disposal. Process waits, disposal, supplied-descriptor mutation, native-environment mutation, and external child reaping must not race. A replacement child environment does not control `posix_spawnp` executable lookup on supported glibc and musl versions; pass a path containing `/` when lookup must not use the native parent `PATH`.

## License

[MIT](LICENSE)
