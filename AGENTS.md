# Copilot Instructions

## Build & Test

```sh
dotnet build
dotnet test --no-build
dotnet test --no-build --filter "FullyQualifiedName~LinuxClockTests"  # single test
dotnet run -c Release --project LinuxCore.Benchmarks                  # benchmarks
```

All projects target **net10.0** and use `LangVersion=preview`. Warnings are treated as errors (`TreatWarningsAsErrors=true`). Documentation XML is generated for the main library.

## Architecture

The library is a thin Linux LibC wrapper with two distinct layers:

- **`LinuxCore/Interop/`** — raw P/Invoke declarations only. Each file maps to one libc header/subsystem (e.g. `File.cs`, `Socket.cs`, `Time.cs`). These are `internal static unsafe partial` classes using `[LibraryImport]` (source-generated P/Invoke, AOT-compatible).
- **`LinuxCore/` (root)** — public API types that wrap the `Interop` layer. These expose ergonomic, safe-ish abstractions (e.g. `LinuxFile`, `LinuxEvent`, `LinuxSemaphore`, `LinuxClock`).

The hierarchy for file-descriptor-owning types is:  
`NativeObject` (finalizer + `IDisposable`) → `FileObject` (holds `FileDescriptor`, exposes `Read`/`Write`/`IOCctl`) → concrete types like `LinuxEventBase`, `LinuxSocketBase`.

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

### Tests
- Framework: **MSTest** (`[TestClass]` / `[TestMethod]`).
- `Global.cs` applies `[assembly: DoNotParallelize]` — tests must not run in parallel.
