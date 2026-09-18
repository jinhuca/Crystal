namespace Crystal.Service.Benchmark;

/// <summary>The subsystem a benchmark suite stresses. Drives the dashboard's grouping.</summary>
public enum BenchmarkCategory {
  Cpu,
  Gpu,
  Memory,
  Storage,
}
