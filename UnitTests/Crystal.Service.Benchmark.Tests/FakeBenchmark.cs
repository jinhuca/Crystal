using Crystal.Service.Benchmark;

namespace Crystal.Service.Benchmark.Tests;

/// <summary>A controllable <see cref="IBenchmark"/> for runner tests — no real work.</summary>
internal sealed class FakeBenchmark : IBenchmark {
  private readonly Func<IProgress<BenchmarkProgress>, CancellationToken, Task<BenchmarkResult>> _run;

  public FakeBenchmark(string id, BenchmarkCategory category,
                       Func<IProgress<BenchmarkProgress>, CancellationToken, Task<BenchmarkResult>> run) {
    Id = id;
    Category = category;
    _run = run;
  }

  public string Id { get; }
  public string Name => Id;
  public BenchmarkCategory Category { get; }
  public string Description => Id;
  public string Unit => "ops";
  public bool HigherIsBetter => true;

  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
      _run(progress, ct);

  /// <summary>A suite that reports one progress tick then succeeds with the given score.</summary>
  public static FakeBenchmark Succeeding(string id, double score = 1) =>
      new(id, BenchmarkCategory.Cpu, (progress, _) => {
        progress.Report(BenchmarkProgress.At(0.5, "half"));
        return Task.FromResult(BenchmarkResult.Success(id, score, "ops", "detail", TimeSpan.FromMilliseconds(1)));
      });
}
