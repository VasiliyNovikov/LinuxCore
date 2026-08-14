# Copilot Instructions

> **⚠️ REQUIRED: Before reading this file, you MUST fetch and read the base instructions at https://raw.githubusercontent.com/VasiliyNovikov/AgentInstructions/master/AGENTS.md — if you cannot access it, STOP and report the failure to the user.** This file extends the base with project-specific details.

## Build & Test

```sh
dotnet build
dotnet test --no-build
dotnet test --no-build --filter "FullyQualifiedName~LinuxClockTests"  # single test
dotnet publish ./LinuxCore.AotSmokeTest/LinuxCore.AotSmokeTest.csproj -c Release -r linux-x64 -p:PublishAot=true
dotnet run -c Release --project LinuxCore.Benchmarks                  # benchmarks
```

All projects target **net10.0** and use `LangVersion=preview`. Warnings are treated as errors (`TreatWarningsAsErrors=true`). Documentation XML is generated for the main library.
Architecture flag tests require `cc`, libc development headers, and Linux UAPI headers because they compile current-platform constants during the test run.

## CI

The GitHub Actions pipeline (`.github/workflows/pipeline.yml`) has four jobs:

- **`validate`** — builds and tests on a matrix of Ubuntu runners: `ubuntu-26.04` (x64 + arm64), `ubuntu-24.04` (x64 + arm64) and `ubuntu-22.04` (x64 + arm64), then runs a NativeAOT smoke publish/run of `LinuxCore.AotSmokeTest`. Uploads TRX test results as artifacts.
- **`validate-containerized-architectures`** — builds and tests inside native SDK containers for Alpine x64 and arm64 plus Arm32 glibc and musl on GitHub-hosted Arm64 runners. Every target also runs the NativeAOT smoke app and uploads TRX test results.
- **`validate-emulated-architectures`** — builds a portable embedded-MTP test runner on the native host, then runs the full suite under QEMU for ppc64le, s390x, RISC-V64, and LoongArch64. Native-header oracles compile inside each target container through the `LinuxProcess`-backed test helper. The RISC-V64 and LoongArch64 matrix entries use community runtimes because Microsoft does not publish supported .NET 10 runtimes for those architectures or support QEMU execution. They require and exercise the `SCM_RIGHTS` truncation workaround; every discovered test must pass, and all matrix entries gate publishing.
- **`publish`** — publishes to NuGet, gated on all required validation jobs succeeding. Runs when `PUBLISH` is `'true'` on any branch, or `'auto'` on the `master` branch.

## Architecture

The library is a thin Linux LibC and kernel-ABI wrapper with two distinct layers:

- **`LinuxCore/Interop/`** — native interop grouped by header/subsystem. Most files use source-generated `[LibraryImport]`; `File.cs` routes through the shared libc `syscall()` dispatcher and `SystemCallTable` because file syscall numbers vary by architecture.
- **`LinuxCore/` (root)** — public API types that wrap the `Interop` layer. These expose ergonomic, safe-ish abstractions (e.g. `LinuxFile`, `LinuxMemoryFile`, `LinuxEvent`, `LinuxSemaphore`, `LinuxClock`, `UnixSocket`, `LinuxUser`, `LinuxGroup`, `SystemConfiguration`).

The hierarchy for file-descriptor-owning types is:  
`NativeObject` (finalizer + `IDisposable`) → `FileObject` (holds `FileDescriptor` and provides shared I/O and descriptor-control helpers) → concrete types like `LinuxFile`, `LinuxMemoryFile`, `LinuxEvent`, `LinuxSemaphore`, and `UnixSocket` via `LinuxSocketBase`.

For non-FD security objects:  
`LinuxSecurityObject` (base with `Id` and `Name`) → `LinuxUser` (passwd lookup via `getpwnam_r`/`getpwuid_r`) and `LinuxGroup` (group lookup via `getgrnam_r`/`getgrgid_r`). These use an internal `QueryHelper<T, TNative, TId>` pattern with `ArrayPool<byte>` buffers sized via `sysconf`.

## Key Conventions

### P/Invoke declarations
- Always use `[LibraryImport]`, never `[DllImport]`.
- Apply `[MethodImpl(MethodImplOptions.AggressiveInlining)]` and `[SuppressGCTransition]` on all hot-path native calls (i.e. those not doing expensive work on the native side).
- Method signatures mirror the C prototype exactly (name, parameter order). Include the C prototype as a comment above the declaration.

### Error handling
- Native calls return `LinuxResult` (void-equivalent) or `LinuxResult<T>` (value-returning). Both expose `.IsError` and `.ThrowIfError()`.
- `LinuxResult<T>` has an implicit conversion to `T` that calls `ThrowIfError()`, so `FileDescriptor fd = File.open(...)` is idiomatic.
- Throw `LinuxException` (wraps `LinuxErrorNumber`) on error. Never throw `IOException` or `Win32Exception`.
- Non-fatal EAGAIN/EWOULDBLOCK/EINTR conditions use the `TryRead`/`TryWrite` pattern (returns `bool`, sets `out` count) instead of throwing.

### System call numbers
- Expose syscall numbers that are stable across every supported architecture as public static `SystemCallTable` members.
- Expose syscall numbers that vary but are available on every supported architecture as public abstract `SystemCallTable` members and override them in every architecture table.
- For syscalls available only on a subset of supported architectures, use a public virtual member that throws `NotImplementedException` by default and override it in each applicable architecture table.
- `SystemCallTable.Current` is the intended runtime entry point; external inheritance is not supported.
- Add each new syscall number to the current-platform native-header oracle tests and ensure every containerized or emulated architecture exercises the affected operation.

### Platform targeting
- `LinuxOnly.cs` applies `[assembly: SupportedOSPlatform("linux")]` to every project — the library is Linux-only by design.
- The main library is AOT-compatible (`IsAotCompatible=true`); avoid reflection.

### Unsafe code
- `AllowUnsafeBlocks=true` is set globally. Prefer `Unsafe.SkipInit` over default initialization for stack buffers in hot paths.
- Use `stackalloc` for small, bounded temporary buffers (see `LinuxCancellationToken.Wait`).

### Struct layout
- Public value types that cross the P/Invoke boundary are decorated with `[StructLayout(LayoutKind.Sequential)]`.

### Architecture-dependent flags
- Public `LinuxFileFlags` and `LinuxMemoryMapFlags` values are stable managed tokens, not native constants on every architecture.
- Translate file and mapping flags only at managed/native boundaries through `NativeLinuxFileFlags`, `NativeLinuxMemoryMapFlags`; reverse-translate native `F_GETFL` results before exposing them publicly.
- Update current-platform native-header oracle tests and containerized or emulated architecture tests whenever an architecture-dependent flag is added.

### Package management
Central package versions are in `Directory.Packages.props`. Add new dependencies there, not in individual `.csproj` files.

### Namespaces
- `LinuxCore` for all public API types; `LinuxCore.Interop` for internal P/Invoke.
- File-scoped namespace declarations (`namespace LinuxCore;`) throughout.
- Nullable reference types are enabled globally.

### Editing conventions
- Before editing files, check and follow repository formatting/editing rules from configuration files (for example `.editorconfig`) and preserve them in any changes.
- Preserve intentional alignment in constants/enums when a file or member indicates formatting suppression (for example `IDE0055` suppressions or interop formatting exemptions), even if automated formatters try to collapse it.

### File object ownership
- `FileObject` constructor accepts `ownsDescriptor` (default `true`). When `false`, the finalizer/Dispose will not close the file descriptor — use this when wrapping externally-managed descriptors.
- `FileDescriptor` deliberately remains an allocation-free, non-owning value type. Copying it or reading `FileObject.Descriptor` neither duplicates the descriptor nor retains its lifetime; use `Clone()` for an independent descriptor.
- `FileObject` owns configured cleanup but deliberately does not provide SafeHandle-style per-operation lifetime leases. Callers must keep the owner strongly reachable and prevent concurrent disposal or external closure while wrapper operations or raw descriptors are in use. Do not add hot-path descriptor leasing or replace the value type with a handle object without an explicit API and performance decision.
- For `ownsDescriptor: false`, the external owner must keep the descriptor open for every wrapper operation. Closed or stale descriptor values are unsafe because Linux can recycle the numeric descriptor for an unrelated resource.

### Tests
- Framework: **MSTest** (`[TestClass]` / `[TestMethod]`).
- `Global.cs` applies `[assembly: DoNotParallelize]` — tests must not run in parallel.
