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

## CI

The GitHub Actions pipeline (`.github/workflows/pipeline.yml`) has three jobs:

- **`validate`** — builds and tests on a matrix of Ubuntu runners: `ubuntu-26.04` (x64 + arm64), `ubuntu-24.04` (x64 + arm64), `ubuntu-22.04` (x64 + arm64) and ubuntu-slim, then runs a NativeAOT smoke publish/run of `LinuxCore.AotSmokeTest`. Uploads TRX test results as artifacts.
- **`validate-alpine`** — builds and tests under musl/Alpine Linux via Docker (`mcr.microsoft.com/dotnet/sdk:10.0-alpine`) on x64 and arm64 runners, and also runs the NativeAOT smoke app there. Uploads TRX test results as artifacts.
- **`publish`** — publishes to NuGet, gated on both `validate` and `validate-alpine` succeeding. Runs when `PUBLISH` is `'true'` on any branch, or `'auto'` on the `master` branch.

## Architecture

The library is a thin Linux LibC wrapper with two distinct layers:

- **`LinuxCore/Interop/`** — raw P/Invoke declarations only. Each file maps to one libc header/subsystem (e.g. `File.cs`, `Socket.cs`, `Time.cs`, `SysConf.cs`, `User.cs`). These are `internal static unsafe partial` classes using `[LibraryImport]` (source-generated P/Invoke, AOT-compatible).
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

### Platform targeting
- `LinuxOnly.cs` applies `[assembly: SupportedOSPlatform("linux")]` to every project — the library is Linux-only by design.
- The main library is AOT-compatible (`IsAotCompatible=true`); avoid reflection.

### Unsafe code
- `AllowUnsafeBlocks=true` is set globally. Prefer `Unsafe.SkipInit` over default initialization for stack buffers in hot paths.
- Use `stackalloc` for small, bounded temporary buffers (see `LinuxCancellationToken.Wait`).

### Struct layout
- Public value types that cross the P/Invoke boundary are decorated with `[StructLayout(LayoutKind.Sequential)]`.

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

### Tests
- Framework: **MSTest** (`[TestClass]` / `[TestMethod]`).
- `Global.cs` applies `[assembly: DoNotParallelize]` — tests must not run in parallel.
