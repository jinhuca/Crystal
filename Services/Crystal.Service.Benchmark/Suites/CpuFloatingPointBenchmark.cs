using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Floating-point-throughput suite: repeated dense single-precision matrix multiplication
/// (<c>C = A · B</c>, N×N), parallelized over output rows. Each multiply is 2·N³ floating-point
/// operations (one multiply + one add per inner step), so the score is sustained GFLOPS across all
/// cores — the classic FP compute proxy.
/// </summary>
public sealed class CpuFloatingPointBenchmark : IBenchmark {
  /// <summary>
  /// The matrix size N (N×N) and the number of rounds to run. The default is 640, which is large 
  /// enough to saturate all cores on a modern CPU but small enough to fit in L3 cache and complete 
  /// in a few seconds.
  /// </summary>
  private readonly int _n;

  /// <summary>
  /// The number of rounds to run the matrix multiplication. Each round performs one complete multiplication 
  /// of the matrices. The default is 4, which provides a good balance between accuracy and runtime.
  /// </summary>
  private readonly int _rounds;

  /// <summary>
  /// Creates a new instance of the <see cref="CpuFloatingPointBenchmark"/> class with the specified matrix 
  /// size and number of rounds.
  /// </summary>
  /// <param name="n">The matrix size N (N×N)</param>
  /// <param name="rounds">The number of rounds to run</param>
  public CpuFloatingPointBenchmark(int n = 640, int rounds = 4) {
    _n = n;
    _rounds = rounds;
  }

  /// <summary>
  /// Gets the unique identifier for this benchmark suite.
  /// </summary>
  public string Id => "cpu.float";

  /// <summary>
  /// Gets the display name for this benchmark suite.
  /// </summary>
  public string Name => "Floating point (matrix multiply)";

  /// <summary>
  /// Gets the category of this benchmark suite, which is CPU.
  /// </summary>
  public BenchmarkCategory Category => BenchmarkCategory.Cpu;

  /// <summary>
  /// Gets a one-line description of what this benchmark suite measures.
  /// </summary>
  public string Description => "Dense N×N single-precision matrix multiplication across all cores.";

  /// <summary>
  /// Gets the unit of the headline score for this benchmark suite, which is GFLOPS 
  /// (giga floating-point operations per second).
  /// </summary>
  public string Unit => "GFLOPS";

  /// <summary>
  /// Gets a value indicating whether a higher score is better for this benchmark suite. 
  /// For this suite, a higher score indicates better performance, so this property returns true.
  /// </summary>
  public bool HigherIsBetter => true;

  /// <summary>
  /// Runs the benchmark asynchronously, reporting progress and supporting cancellation. 
  /// The benchmark performs repeated dense single-precision matrix multiplication and measures 
  /// the sustained GFLOPS across all cores.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>The benchmark result.</returns>
  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
    Task.Run(() => {
      progress.Report(BenchmarkProgress.At(0, "Allocating matrices"));
      int n = _n;
      float[] a = MakeMatrix(n, seed: 1);
      float[] b = MakeMatrix(n, seed: 2);
      float[] c = new float[n * n];

      Multiply(a, b, c, n, ct); // Warm-up, untimed.

      double flopsPerMultiply = 2.0 * n * n * n;
      var sw = Stopwatch.StartNew();
      for (int round = 0; round < _rounds; round++) {
        ct.ThrowIfCancellationRequested();
        Multiply(a, b, c, n, ct);
        progress.Report(BenchmarkProgress.At((round + 1) / (double)_rounds, $"Round {round + 1}/{_rounds}"));
      }
      sw.Stop();

      double gflops = flopsPerMultiply * _rounds / sw.Elapsed.TotalSeconds / 1e9;
      string detail = $"{n}×{n} · {_rounds} rounds in {sw.Elapsed.TotalSeconds:N2} s";
      return BenchmarkResult.Success(Id, gflops, Unit, detail, sw.Elapsed);
    }, ct);

  /// <summary>
  /// Performs matrix multiplication C = A · B for N×N matrices, parallelized over output rows.
  /// </summary>
  /// <param name="a">The first matrix.</param>
  /// <param name="b">The second matrix.</param>
  /// <param name="c">The output matrix.</param>
  /// <param name="n">The dimension of the matrices.</param>
  /// <param name="ct">The cancellation token.</param>
  private static void Multiply(float[] a, float[] b, float[] c, int n, CancellationToken ct) {
    Parallel.For(0, n, new ParallelOptions { CancellationToken = ct }, i => {
      int iRow = i * n;
      for (int k = 0; k < n; k++) {
        float aik = a[iRow + k];
        int kRow = k * n;
        for (int j = 0; j < n; j++)
          c[iRow + j] += aik * b[kRow + j];
      }
    });
  }

  /// <summary>
  /// Generates a random N×N matrix with values in the range [0, 1) using the specified seed for reproducibility.
  /// </summary>
  /// <param name="n">The dimension of the matrix.</param>
  /// <param name="seed">The seed for the random number generator.</param>
  /// <returns>The generated matrix.</returns>
  private static float[] MakeMatrix(int n, int seed) {
    var rng = new Random(seed);
    var m = new float[n * n];
    for (int i = 0; i < m.Length; i++) m[i] = (float)rng.NextDouble();
    return m;
  }
}
