using Crystal.Service.Benchmark.Suites;

namespace Crystal.Service.Benchmark;

/// <summary>
/// The default catalog: one instance of every built-in suite, ordered CPU → GPU → Memory → Storage
/// so the dashboard's category sections come out in a natural order. Suites are constructed with
/// their production sizing; tests construct suites directly with small sizes instead of going
/// through the catalog.
/// </summary>
public sealed class BenchmarkCatalog : IBenchmarkCatalog {
  public BenchmarkCatalog() {
    All = [
      new CpuIntegerBenchmark(),
      new CpuFloatingPointBenchmark(),
      new GpuComputeBenchmark(),
      new MemoryBandwidthBenchmark(),
      new MemoryLatencyBenchmark(),
      new StorageSequentialBenchmark(),
      new StorageRandomBenchmark(),
    ];
  }

  public IReadOnlyList<IBenchmark> All { get; }
}
