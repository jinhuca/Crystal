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
  private readonly int _elements;
  private readonly int _rounds;

  // 8M doubles per array => 64 MB each, 192 MB working set — comfortably past any CPU cache.
  public MemoryBandwidthBenchmark(int elements = 8_000_000, int rounds = 12) {
    _elements = elements;
    _rounds = rounds;
  }

  public string Id => "mem.bandwidth";
  public string Name => "Bandwidth (STREAM triad)";
  public BenchmarkCategory Category => BenchmarkCategory.Memory;
  public string Description => "STREAM triad over a 192 MB working set — sustained DRAM throughput.";
  public string Unit => "GB/s";
  public bool HigherIsBetter => true;

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

  // Range-partitioned so each thread runs a tight index loop, rather than paying a delegate call
  // per element (which would dominate the memory work at this element count).
  private static void Triad(double[] a, double[] b, double[] c, double q, CancellationToken ct) {
    Parallel.ForEach(Partitioner.Create(0, a.Length), new ParallelOptions { CancellationToken = ct }, range => {
      for (int i = range.Item1; i < range.Item2; i++) a[i] = b[i] + q * c[i];
    });
  }
}
