using BenchmarkDotNet.Running;

using LinuxCore.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(LinuxEventBenchmarks).Assembly).Run(args);