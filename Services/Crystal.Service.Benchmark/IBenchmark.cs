namespace Crystal.Service.Benchmark;

/// <summary>
/// One selectable benchmark suite. Implementations are stateless with respect to a run — all
/// per-run state lives on the stack of <see cref="RunAsync"/> — so a single instance can be held in
/// the <see cref="IBenchmarkCatalog"/> and run repeatedly. A run reports progress through
/// <paramref name="progress"/> and must observe <paramref name="ct"/> promptly (checked between
/// work chunks) so the UI's Cancel is responsive.
/// </summary>
public interface IBenchmark {
  /// <summary>
  /// Stable identifier, unique across the catalog (used to key results back to rows).
  /// </summary>
  string Id { get; }

  /// <summary>
  /// Display name shown on the dashboard row.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Category of the suite, used to group rows on the dashboard.
  /// </summary>
  BenchmarkCategory Category { get; }

  /// <summary>
  /// One-line description of what the suite measures.
  /// </summary>
  string Description { get; }

  /// <summary>
  /// Unit of the headline <see cref="BenchmarkResult.Score"/> (e.g. "GB/s", "GFLOPS").
  /// </summary>
  string Unit { get; }

  /// <summary>
  /// True when a larger score is better (throughput); false for latency-style metrics.
  /// </summary>
  bool HigherIsBetter { get; }

  /// <summary>
  /// Runs the suite to completion (or until cancelled) and returns its result. Never throws for an
  /// expected "can't run here" condition (missing device, denied path) — returns
  /// <see cref="BenchmarkResult.Failure"/> instead; only <see cref="OperationCanceledException"/>
  /// propagates when <paramref name="ct"/> fires.
  /// </summary>
  Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct);
}
