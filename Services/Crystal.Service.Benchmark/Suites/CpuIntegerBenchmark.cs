using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Integer-throughput suite: every logical core counts primes in <c>[2, limit)</c> by trial
/// division, repeated over several rounds. Trial division is branch-heavy integer work with no
/// floating point, so it isolates the integer ALUs and scales with core count — the score is the
/// total candidates tested per second across all cores (MOps/s).
/// </summary>
public sealed class CpuIntegerBenchmark : IBenchmark {
  private readonly int _limit;
  private readonly int _rounds;
  private readonly int _threads;

  public CpuIntegerBenchmark(int limit = 300_000, int rounds = 8, int? threads = null) {
    _limit = limit;
    _rounds = rounds;
    _threads = threads ?? Environment.ProcessorCount;
  }

  public string Id => "cpu.integer";
  public string Name => "Integer (prime sieve)";
  public BenchmarkCategory Category => BenchmarkCategory.Cpu;
  public string Description => "Multi-threaded trial-division prime counting across all cores.";
  public string Unit => "MOps/s";
  public bool HigherIsBetter => true;

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
