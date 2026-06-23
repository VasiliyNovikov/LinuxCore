using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(LinuxCore.Benchmarks.LinuxEventBenchmarks).Assembly).Run(args);