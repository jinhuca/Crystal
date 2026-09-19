namespace Crystal.Service.Benchmark;

/// <summary>
/// The set of benchmark suites the app offers. A singleton built once at composition; the dashboard
/// reads <see cref="All"/> to populate its selectable rows.
/// </summary>
public interface IBenchmarkCatalog {
  /// <summary>Every suite, in a stable order grouped by <see cref="BenchmarkCategory"/>.</summary>
  IReadOnlyList<IBenchmark> All { get; }
}
