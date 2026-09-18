using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Floating-point-throughput suite: repeated dense single-precision matrix multiplication
/// (<c>C = A · B</c>, N×N), parallelized over output rows. Each multiply is 2·N³ floating-point
/// operations (one multiply + one add per inner step), so the score is sustained GFLOPS across all
/// cores — the classic FP compute proxy.
/// </summary>
public sealed class CpuFloatingPointBenchmark : IBenchmark {
  private readonly int _n;
  private readonly int _rounds;

  public CpuFloatingPointBenchmark(int n = 640, int rounds = 4) {
    _n = n;
    _rounds = rounds;
  }

  public string Id => "cpu.float";
  public string Name => "Floating point (matrix multiply)";
  public BenchmarkCategory Category => BenchmarkCategory.Cpu;
  public string Description => "Dense N×N single-precision matrix multiplication across all cores.";
  public string Unit => "GFLOPS";
  public bool HigherIsBetter => true;

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

  private static float[] MakeMatrix(int n, int seed) {
    var rng = new Random(seed);
    var m = new float[n * n];
    for (int i = 0; i < m.Length; i++) m[i] = (float)rng.NextDouble();
    return m;
  }
}
