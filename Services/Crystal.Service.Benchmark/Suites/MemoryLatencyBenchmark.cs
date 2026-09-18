using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Memory-latency suite: a dependent pointer-chase through a single randomly-permuted cycle that
/// spans a working set far larger than cache, so almost every hop is a cache miss served from DRAM.
/// Each hop depends on the previous one, defeating prefetch and memory-level parallelism, so the
/// score is the average load-to-use latency in nanoseconds (lower is better).
/// </summary>
public sealed class MemoryLatencyBenchmark : IBenchmark {
  private readonly int _elements;
  private readonly int _hops;

  // 16M ints => 64 MB, well past LLC. 128M dependent hops keeps the timed section ~a second.
  public MemoryLatencyBenchmark(int elements = 16_000_000, int hops = 128_000_000) {
    _elements = elements;
    _hops = hops;
  }

  public string Id => "mem.latency";
  public string Name => "Latency (pointer chase)";
  public BenchmarkCategory Category => BenchmarkCategory.Memory;
  public string Description => "Dependent pointer-chase over a 64 MB buffer — DRAM load-to-use latency.";
  public string Unit => "ns";
  public bool HigherIsBetter => false;

  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
      Task.Run(() => {
        progress.Report(BenchmarkProgress.At(0, "Building chase cycle"));
        int[] next = BuildCycle(_elements, ct);

        // Warm-up: a short chase to page in and JIT, untimed.
        Chase(next, 1_000_000, 0);

        var sw = Stopwatch.StartNew();
        int idx = 0;
        const int segments = 20;
        long perSegment = _hops / segments;
        for (int s = 0; s < segments; s++) {
          ct.ThrowIfCancellationRequested();
          idx = Chase(next, perSegment, idx);
          progress.Report(BenchmarkProgress.At((s + 1) / (double)segments, $"Chasing {(s + 1) * perSegment / 1e6:N0}M hops"));
        }
        sw.Stop();
        GC.KeepAlive(idx); // Prevent the chase from being optimized away.

        long totalHops = perSegment * segments;
        double nsPerHop = sw.Elapsed.TotalMilliseconds * 1e6 / totalHops;
        string detail = $"{totalHops / 1e6:N0}M hops over {_elements / (1024 * 1024 / 4)} MB in {sw.Elapsed.TotalSeconds:N2} s";
        return BenchmarkResult.Success(Id, nsPerHop, Unit, detail, sw.Elapsed);
      }, ct);

  // Fisher–Yates over [0, n) then link into one Hamiltonian cycle: next[perm[i]] = perm[i+1].
  // A single cycle guarantees the chase visits the whole buffer without early repeats.
  private static int[] BuildCycle(int n, CancellationToken ct) {
    var perm = new int[n];
    for (int i = 0; i < n; i++) perm[i] = i;
    var rng = new Random(12345);
    for (int i = n - 1; i > 0; i--) {
      int j = rng.Next(i + 1);
      (perm[i], perm[j]) = (perm[j], perm[i]);
    }
    ct.ThrowIfCancellationRequested();
    var next = new int[n];
    for (int i = 0; i < n; i++) next[perm[i]] = perm[(i + 1) % n];
    return next;
  }

  private static int Chase(int[] next, long hops, int start) {
    int idx = start;
    for (long h = 0; h < hops; h++) idx = next[idx];
    return idx;
  }
}
