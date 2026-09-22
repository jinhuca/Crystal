using System.Collections.Concurrent;
using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Sustained memory-bandwidth suite modelled on the STREAM "triad" kernel
/// (<c>a[i] = b[i] + q·c[i]</c>) over arrays far larger than last-level cache, so it measures DRAM
/// throughput rather than cache. Each element touched moves 24 bytes (two 8-byte reads + one 8-byte
/// write); the score is aggregate GB/s. Parallelized so it saturates the memory controllers.
/// </summary>
public sealed class MemoryBandwidthBenchmark : IBenchmark {
  /// <summary>
  /// The number of elements in each array. The default is 8,000,000, which results in a working set 
  /// of 192 MB (3 arrays of 64 MB each), comfortably exceeding typical CPU cache sizes to ensure that 
  /// the benchmark measures DRAM bandwidth.
  /// </summary>
  private readonly int _elements;

  /// <summary>
  /// The number of rounds to run the STREAM triad operation. The default is 12, which provides a good
  /// balance between measurement accuracy and runtime.
  /// </summary>
  private readonly int _rounds;

  /// <summary>
  /// 8M doubles per array => 64 MB each, 192 MB working set — comfortably past any CPU cache.
  /// </summary>
  /// <param name="elements">The number of elements in each array.</param>
  /// <param name="rounds">The number of rounds to run the STREAM triad operation.</param>
  public MemoryBandwidthBenchmark(int elements = 8_000_000, int rounds = 12) {
    _elements = elements;
    _rounds = rounds;
  }

  /// <summary>
  /// Gets the unique identifier for this benchmark suite, which is "mem.bandwidth".
  /// </summary>
  public string Id => "mem.bandwidth";

  /// <summary>
  /// Gets the display name for this benchmark suite, which is "Bandwidth (STREAM triad)".
  /// </summary>
  public string Name => "Bandwidth (STREAM triad)";

  /// <summary>
  /// Gets the category of this benchmark suite, which is Memory.
  /// </summary>
  public BenchmarkCategory Category => BenchmarkCategory.Memory;

  /// <summary>
  /// Gets a one-line description of what this benchmark suite measures, which is "STREAM triad 
  /// over a 192 MB working set — sustained DRAM throughput.".
  /// </summary>
  public string Description => "STREAM triad over a 192 MB working set — sustained DRAM throughput.";

  /// <summary>
  /// Gets the unit of the headline score for this benchmark suite, which is GB/s (gigabytes per second).
  /// </summary>
  public string Unit => "GB/s";

  /// <summary>
  /// Gets a value indicating whether a higher score is better for this benchmark suite. 
  /// For memory bandwidth, higher is better.
  /// </summary>
  public bool HigherIsBetter => true;

  /// <summary>
  /// Runs the memory bandwidth benchmark asynchronously, reporting progress and supporting
  /// cancellation. It allocates the three working arrays, runs the STREAM triad once untimed to
  /// page them in, then times repeated triad passes and reports aggregate DRAM throughput.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>A task returning the benchmark result.</returns>
  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
    Task.Run(() => {
      progress.Report(BenchmarkProgress.At(0, "Allocating arrays"));
      int n = _elements;
      double[] a = new double[n];
      double[] b = new double[n];
      double[] c = new double[n];
      Array.Fill(b, 1.0);
      Array.Fill(c, 2.0);
      const double q = 3.0;

      Triad(a, b, c, q, ct); // Warm-up + first-touch page-in, untimed.

      double bytesPerRound = 24.0 * n;
      var sw = Stopwatch.StartNew();
      for (int round = 0; round < _rounds; round++) {
        ct.ThrowIfCancellationRequested();
        Triad(a, b, c, q, ct);
        progress.Report(BenchmarkProgress.At((round + 1) / (double)_rounds, $"Round {round + 1}/{_rounds}"));
      }
      sw.Stop();

      double gbps = bytesPerRound * _rounds / sw.Elapsed.TotalSeconds / 1e9;
      string detail = $"{bytesPerRound * _rounds / 1e9:N1} GB moved in {sw.Elapsed.TotalSeconds:N2} s";
      return BenchmarkResult.Success(Id, gbps, Unit, detail, sw.Elapsed);
    }, ct);

  /// <summary>
  /// Runs one STREAM triad pass (<c>a[i] = b[i] + q·c[i]</c>) in parallel. Range-partitioned so each
  /// thread runs a tight index loop, rather than paying a delegate call per element (which would
  /// dominate the memory work at this element count).
  /// </summary>
  /// <param name="a">The destination array (written).</param>
  /// <param name="b">The first source array (read).</param>
  /// <param name="c">The second source array (read, scaled by <paramref name="q"/>).</param>
  /// <param name="q">The scalar multiplier applied to <paramref name="c"/>.</param>
  /// <param name="ct">The cancellation token.</param>
  private static void Triad(double[] a, double[] b, double[] c, double q, CancellationToken ct) {
    Parallel.ForEach(Partitioner.Create(0, a.Length), new ParallelOptions { CancellationToken = ct }, range => {
      for (int i = range.Item1; i < range.Item2; i++) a[i] = b[i] + q * c[i];
    });
  }
}
