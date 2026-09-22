namespace Crystal.Service.Benchmark;

/// <summary>
/// A single progress notification from a running benchmark. <see cref="Fraction"/> is clamped
/// 0–1; <see cref="Status"/> is a short human-readable phase label ("Warming up", "Read pass",
/// etc.). Suites report these through the <see cref="IProgress{T}"/> passed to
/// <see cref="IBenchmark.RunAsync"/>.
/// </summary>
public readonly record struct BenchmarkProgress(double Fraction, string Status) {
  /// <summary>
  /// Creates a new <see cref="BenchmarkProgress"/> with the given fraction and status. The fraction is clamped to 0–1.
  /// </summary>
  /// <param name="fraction">The progress fraction, clamped to 0–1.</param>
  /// <param name="status">The human-readable status label.</param>
  /// <returns>A new <see cref="BenchmarkProgress"/> instance.</returns>
  public static BenchmarkProgress At(double fraction, string status) =>
    new(fraction < 0 ? 0 : fraction > 1 ? 1 : fraction, status);
}
