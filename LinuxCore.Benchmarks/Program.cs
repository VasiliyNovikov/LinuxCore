using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(global::LinuxCore.Benchmarks.LinuxEventBenchmarks).Assembly).Run(args);