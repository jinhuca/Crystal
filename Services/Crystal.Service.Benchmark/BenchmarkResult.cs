namespace Crystal.Service.Benchmark;

/// <summary>
/// The outcome of one benchmark run. On success <see cref="Score"/> carries the headline metric in
/// <see cref="Unit"/> (interpret with <see cref="IBenchmark.HigherIsBetter"/>) and <see cref="Detail"/>
/// a short breakdown (e.g. "read 3.1 GB/s · write 2.4 GB/s"). On failure <see cref="Succeeded"/> is
/// false and <see cref="Error"/> explains why (e.g. no Direct3D device) — a failed suite never throws
/// out of the runner, so one unavailable subsystem doesn't abort the batch.
/// </summary>
public sealed record BenchmarkResult(
    string Id,
    double Score,
    string Unit,
    string Detail,
    TimeSpan Elapsed,
    bool Succeeded,
    string? Error = null) {

  public static BenchmarkResult Success(string id, double score, string unit, string detail, TimeSpan elapsed) =>
      new(id, score, unit, detail, elapsed, Succeeded: true);

  public static BenchmarkResult Failure(string id, string error, TimeSpan elapsed = default) =>
      new(id, Score: 0, Unit: string.Empty, Detail: string.Empty, elapsed, Succeeded: false, error);
}
