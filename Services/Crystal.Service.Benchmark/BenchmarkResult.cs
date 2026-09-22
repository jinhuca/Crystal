namespace Crystal.Service.Benchmark;

/// <summary>
/// The outcome of one benchmark run. On success <see cref="Score"/> carries the headline metric in
/// <see cref="Unit"/> (interpret with <see cref="IBenchmark.HigherIsBetter"/>) and <see cref="Detail"/>
/// a short breakdown (e.g. "read 3.1 GB/s · write 2.4 GB/s"). On failure <see cref="Succeeded"/> is
/// false and <see cref="Error"/> explains why (e.g. no Direct3D device) — a failed suite never throws
/// out of the runner, so one unavailable subsystem doesn't abort the batch.
/// </summary>
public sealed record BenchmarkResult(string Id,
                                     double Score,
                                     string Unit,
                                     string Detail,
                                     TimeSpan Elapsed,
                                     bool Succeeded,
                                     string? Error = null) {
  /// <summary>
  /// Creates a successful benchmark result.
  /// </summary>
  /// <param name="id">The unique identifier for the benchmark.</param>
  /// <param name="score">The headline metric value.</param>
  /// <param name="unit">The unit of the headline metric.</param>
  /// <param name="detail">A short breakdown of the benchmark results.</param>
  /// <param name="elapsed">The time taken to complete the benchmark.</param>
  /// <returns>A successful benchmark result.</returns>
  public static BenchmarkResult Success(string id, double score, string unit, string detail, TimeSpan elapsed) =>
    new(id, score, unit, detail, elapsed, Succeeded: true);

  /// <summary>
  /// Creates a failed benchmark result.
  /// </summary>
  /// <param name="id">The unique identifier for the benchmark.</param>
  /// <param name="error">An explanation of why the benchmark failed.</param>
  /// <param name="elapsed">The time taken to complete the benchmark.</param>
  /// <returns>A failed benchmark result.</returns>
  public static BenchmarkResult Failure(string id, string error, TimeSpan elapsed = default) =>
    new(id, Score: 0, Unit: string.Empty, Detail: string.Empty, elapsed, Succeeded: false, error);
}
