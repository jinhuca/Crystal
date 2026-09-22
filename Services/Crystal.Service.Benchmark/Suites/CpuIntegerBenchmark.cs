using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Integer-throughput suite: every logical core counts primes in <c>[2, limit)</c> by trial
/// division, repeated over several rounds. Trial division is branch-heavy integer work with no
/// floating point, so it isolates the integer ALUs and scales with core count — the score is the
/// total candidates tested per second across all cores (MOps/s).
/// </summary>
public sealed class CpuIntegerBenchmark : IBenchmark {
  /// <summary>
  /// The upper limit of the range to count primes in. The default is 300,000, which is large enough to
  /// provide a meaningful measurement of integer throughput.
  /// </summary>
  private readonly int _limit;

  /// <summary>
  /// The number of rounds to run the prime counting. The default is 8, which provides a good balance
  /// between measurement accuracy and execution time.
  /// </summary>
  private readonly int _rounds;

  /// <summary>
  /// The number of threads to use for parallel prime counting. If not specified, it defaults to the number
  /// of logical processors available.
  /// </summary>
  private readonly int _threads;

  /// <summary>
  /// Creates a new instance of the <see cref="CpuIntegerBenchmark"/> class with the specified limit, rounds, and threads.
  /// </summary>
  /// <param name="limit">The upper limit of the range to count primes in.</param>
  /// <param name="rounds">The number of rounds to run the prime counting.</param>
  /// <param name="threads">The number of threads to use for parallel prime counting.</param>
  public CpuIntegerBenchmark(int limit = 300_000, int rounds = 8, int? threads = null) {
    _limit = limit;
    _rounds = rounds;
    _threads = threads ?? Environment.ProcessorCount;
  }

  /// <summary>
  /// Gets the unique identifier for this benchmark suite.
  /// </summary>
  public string Id => "cpu.integer";

  /// <summary>
  /// Gets the display name for this benchmark suite.
  /// </summary>
  public string Name => "Integer (prime sieve)";

  /// <summary>
  /// Gets the category of this benchmark suite, which is CPU.
  /// </summary>
  public BenchmarkCategory Category => BenchmarkCategory.Cpu;

  /// <summary>
  /// Gets a one-line description of what this benchmark suite measures.
  /// </summary>
  public string Description => "Multi-threaded trial-division prime counting across all cores.";

  /// <summary>
  /// Gets the unit of the headline score for this benchmark suite, which is MOps/s 
  /// (millions of operations per second).
  /// </summary>
  public string Unit => "MOps/s";

  /// <summary>
  /// Gets a value indicating whether a higher score is better for this benchmark suite.
  /// </summary>
  public bool HigherIsBetter => true;

  /// <summary>
  /// Runs the benchmark asynchronously, reporting progress and supporting cancellation.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>A task representing the asynchronous operation and returning the benchmark result.</returns>
  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
    Task.Run(() => {
      progress.Report(BenchmarkProgress.At(0, "Warming up"));
      CountPrimes(_limit); // JIT + cache warm-up, not timed.

      long totalCandidates = 0;
      var sw = Stopwatch.StartNew();
      for (int round = 0; round < _rounds; round++) {
        ct.ThrowIfCancellationRequested();
        long counted = 0;
        Parallel.For(0, _threads, new ParallelOptions { MaxDegreeOfParallelism = _threads, CancellationToken = ct },
            _ => { CountPrimes(_limit); Interlocked.Add(ref counted, _limit); });
        totalCandidates += counted;
        progress.Report(BenchmarkProgress.At((round + 1) / (double)_rounds, $"Round {round + 1}/{_rounds}"));
      }
      sw.Stop();

      double mops = totalCandidates / sw.Elapsed.TotalSeconds / 1e6;
      string detail = $"{_threads} threads · {totalCandidates / 1e6:N0} M candidates in {sw.Elapsed.TotalSeconds:N2} s";
      return BenchmarkResult.Success(Id, mops, Unit, detail, sw.Elapsed);
    }, ct);

  /// <summary>
  /// Counts the number of prime numbers less than the specified limit using trial division.
  /// </summary>
  /// <param name="limit">The upper limit (exclusive) for counting primes.</param>
  /// <returns>The number of prime numbers less than the specified limit.</returns>
  private static int CountPrimes(int limit) {
    int count = 0;
    for (int n = 2; n < limit; n++) {
      bool prime = true;
      for (int d = 2; (long)d * d <= n; d++) {
        if (n % d == 0) { prime = false; break; }
      }
      if (prime) count++;
    }
    return count;
  }
}
