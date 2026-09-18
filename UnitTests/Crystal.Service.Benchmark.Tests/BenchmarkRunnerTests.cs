using Crystal.Service.Benchmark;
using Xunit;

namespace Crystal.Service.Benchmark.Tests;

public sealed class BenchmarkRunnerTests {
  [Fact]
  public async Task RunAsync_RunsOnlyGivenSuites_InOrder() {
    var runner = new BenchmarkRunner();
    var suites = new IBenchmark[] {
      FakeBenchmark.Succeeding("a"),
      FakeBenchmark.Succeeding("b"),
    };

    var started = new List<string>();
    var completed = new List<string>();

    await runner.RunAsync(suites,
        onSuiteStarted: b => started.Add(b.Id),
        onSuiteProgress: (_, _) => { },
        onSuiteCompleted: (b, _) => completed.Add(b.Id),
        CancellationToken.None);

    Assert.Equal(["a", "b"], started);
    Assert.Equal(["a", "b"], completed);
  }

  [Fact]
  public async Task RunAsync_ForwardsProgressAndResult() {
    var runner = new BenchmarkRunner();
    var suite = FakeBenchmark.Succeeding("a", score: 42);

    var progressFractions = new List<double>();
    BenchmarkResult? result = null;

    await runner.RunAsync([suite],
        onSuiteStarted: _ => { },
        onSuiteProgress: (_, p) => progressFractions.Add(p.Fraction),
        onSuiteCompleted: (_, r) => result = r,
        CancellationToken.None);

    Assert.Contains(0.5, progressFractions);
    Assert.NotNull(result);
    Assert.True(result!.Succeeded);
    Assert.Equal(42, result.Score);
  }

  [Fact]
  public async Task RunAsync_Cancellation_StopsBatchAndReportsCancelled() {
    var runner = new BenchmarkRunner();
    using var cts = new CancellationTokenSource();

    var blocking = new FakeBenchmark("blocker", BenchmarkCategory.Cpu, async (_, ct) => {
      cts.Cancel();
      await Task.Delay(Timeout.Infinite, ct);
      return BenchmarkResult.Success("blocker", 1, "ops", "", TimeSpan.Zero);
    });
    var second = FakeBenchmark.Succeeding("never");

    var completed = new List<BenchmarkResult>();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
        runner.RunAsync([blocking, second],
            onSuiteStarted: _ => { },
            onSuiteProgress: (_, _) => { },
            onSuiteCompleted: (_, r) => completed.Add(r),
            cts.Token));

    // The blocking suite is reported cancelled; the second suite never runs.
    Assert.Single(completed);
    Assert.False(completed[0].Succeeded);
    Assert.Equal("Cancelled", completed[0].Error);
  }
}
