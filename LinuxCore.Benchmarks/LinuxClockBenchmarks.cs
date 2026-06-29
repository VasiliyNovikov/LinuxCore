using BenchmarkDotNet.Attributes;

namespace LinuxCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class LinuxClockBenchmarks
{
    [Benchmark]
    public long MonotonicNanoseconds() => LinuxClock.MonotonicNanoseconds;

    [Benchmark]
    public long MonotonicRawNanoseconds() => LinuxClock.MonotonicRawNanoseconds;

    [Benchmark]
    public long RealtimeNanoseconds() => LinuxClock.RealtimeNanoseconds;

    [Benchmark]
    public long BootTimeNanoseconds() => LinuxClock.BootTimeNanoseconds;

    [Benchmark]
    public long ProcessCpuTimeNanoseconds() => LinuxClock.ProcessCpuTimeNanoseconds;

    [Benchmark]
    public long ThreadCpuTimeNanoseconds() => LinuxClock.ThreadCpuTimeNanoseconds;
}
