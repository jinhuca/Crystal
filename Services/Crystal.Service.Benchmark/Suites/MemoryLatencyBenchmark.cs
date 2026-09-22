using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Memory-latency suite: a dependent pointer-chase through a single randomly-permuted cycle that
/// spans a working set far larger than cache, so almost every hop is a cache miss served from DRAM.
/// Each hop depends on the previous one, defeating prefetch and memory-level parallelism, so the
/// score is the average load-to-use latency in nanoseconds (lower is better).
/// </summary>
public sealed class MemoryLatencyBenchmark : IBenchmark {
  /// <summary>
  /// The number of elements in the chase cycle. The default is 16,000,000, which results in a working set
  /// of 64 MB, well past the last-level cache.
  /// </summary>
  private readonly int _elements;

  /// <summary>
  /// The number of hops to perform in the pointer chase. The default is 128,000,000, which keeps the timed 
  /// section around one second for a meaningful measurement of latency.
  /// </summary>
  private readonly int _hops;

  /// <summary>
  /// 16M ints => 64 MB, well past LLC. 128M dependent hops keeps the timed section ~a second.
  /// </summary>
  /// <param name="elements">The number of elements in the chase cycle.</param>
  /// <param name="hops">The number of hops to perform in the pointer chase.</param>
  public MemoryLatencyBenchmark(int elements = 16_000_000, int hops = 128_000_000) {
    _elements = elements;
    _hops = hops;
  }

  /// <summary>
  /// Gets the unique identifier for this benchmark suite, which is "mem.latency".
  /// </summary>
  public string Id => "mem.latency";

  /// <summary>
  /// Gets the display name for this benchmark suite, which is "Latency (pointer chase)".
  /// </summary>
  public string Name => "Latency (pointer chase)";

  /// <summary>
  /// Gets the category of this benchmark suite, which is Memory.
  /// </summary>
  public BenchmarkCategory Category => BenchmarkCategory.Memory;

  /// <summary>
  /// Gets a one-line description of what this benchmark suite measures.
  /// </summary>
  public string Description => "Dependent pointer-chase over a 64 MB buffer — DRAM load-to-use latency.";

  /// <summary>
  /// Gets the unit of the headline score for this benchmark suite, which is ns (nanoseconds per hop).
  /// </summary>
  public string Unit => "ns";

  /// <summary>
  /// Gets a value indicating whether a higher score is better. Latency is a lower-is-better metric,
  /// so this returns false.
  /// </summary>
  public bool HigherIsBetter => false;

  /// <summary>
  /// Runs the benchmark asynchronously, reporting progress and supporting cancellation. It builds a
  /// randomly-permuted chase cycle over the working set, then times a long dependent pointer-chase
  /// (split into segments for progress reporting) and reports the average per-hop latency.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>A task returning the benchmark result.</returns>
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

  /// <summary>
  /// Builds the pointer-chase table: a Fisher–Yates shuffle of <c>[0, n)</c> linked into one
  /// Hamiltonian cycle (<c>next[perm[i]] = perm[i+1]</c>). A single cycle guarantees the chase visits
  /// every slot exactly once per lap without early repeats, so the access pattern stays fully random
  /// across the whole buffer.
  /// </summary>
  /// <param name="n">The number of elements in the cycle.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>The <c>next</c> table where <c>next[i]</c> is the index chased to after index <c>i</c>.</returns>
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

  /// <summary>
  /// Performs the dependent pointer-chase: each hop reads <c>next[idx]</c> and feeds it back as the
  /// next index, so the loads serialize and can't be prefetched. The returned index is deliberately
  /// consumed by the caller to stop the loop being optimized away.
  /// </summary>
  /// <param name="next">The chase table from <see cref="BuildCycle"/>.</param>
  /// <param name="hops">The number of hops to perform.</param>
  /// <param name="start">The index to start chasing from.</param>
  /// <returns>The index reached after the last hop.</returns>
  private static int Chase(int[] next, long hops, int start) {
    int idx = start;
    for (long h = 0; h < hops; h++) idx = next[idx];
    return idx;
  }
}
