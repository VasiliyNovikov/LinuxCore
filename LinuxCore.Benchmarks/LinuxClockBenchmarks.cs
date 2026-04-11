using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace LinuxCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class LinuxClockBenchmarks
{
    [Benchmark]
    public long MonotonicNanoseconds() => LinuxClock.MonotonicNanoseconds;

    [Benchmark]
    public System.TimeSpan Monotonic() => LinuxClock.Monotonic;

    [Benchmark]
    public long RealtimeNanoseconds() => LinuxClock.RealtimeNanoseconds;

    [Benchmark]
    public System.DateTimeOffset Realtime() => LinuxClock.Realtime;
}
