using Crystal.Service.Benchmark;
using Crystal.Service.Benchmark.Suites;
using Xunit;

namespace Crystal.Service.Benchmark.Tests;

/// <summary>
/// Smoke tests: each real suite, sized tiny, runs end to end and returns a sensible result. These
/// exercise the actual workloads (not fakes) but at a size that finishes in well under a second.
/// </summary>
public sealed class BenchmarkSuiteTests {
  private static readonly IProgress<BenchmarkProgress> NoProgress = new Progress<BenchmarkProgress>();

  [Fact]
  public async Task CpuInteger_ReturnsPositiveScore() {
    var result = await new CpuIntegerBenchmark(limit: 5_000, rounds: 1, threads: 2)
        .RunAsync(NoProgress, CancellationToken.None);

    Assert.True(result.Succeeded, result.Error);
    Assert.True(result.Score > 0);
  }

  [Fact]
  public async Task CpuFloatingPoint_ReturnsPositiveScore() {
    var result = await new CpuFloatingPointBenchmark(n: 64, rounds: 1)
        .RunAsync(NoProgress, CancellationToken.None);

    Assert.True(result.Succeeded, result.Error);
    Assert.True(result.Score > 0);
  }

  [Fact]
  public async Task MemoryBandwidth_ReturnsPositiveScore() {
    var result = await new MemoryBandwidthBenchmark(elements: 100_000, rounds: 2)
        .RunAsync(NoProgress, CancellationToken.None);

    Assert.True(result.Succeeded, result.Error);
    Assert.True(result.Score > 0);
  }

  [Fact]
  public async Task MemoryLatency_ReturnsPositiveScore() {
    var result = await new MemoryLatencyBenchmark(elements: 100_000, hops: 500_000)
        .RunAsync(NoProgress, CancellationToken.None);

    Assert.True(result.Succeeded, result.Error);
    Assert.True(result.Score > 0);
  }

  [Fact]
  public async Task StorageSequential_ReturnsPositiveScore() {
    var result = await new StorageSequentialBenchmark(fileBytes: 4 * 1024 * 1024, blockBytes: 1024 * 1024)
        .RunAsync(NoProgress, CancellationToken.None);

    Assert.True(result.Succeeded, result.Error);
    Assert.True(result.Score > 0);
  }

  [Fact]
  public async Task StorageRandom_ReturnsPositiveScore() {
    var result = await new StorageRandomBenchmark(fileBytes: 4 * 1024 * 1024, blockBytes: 4 * 1024, operations: 200)
        .RunAsync(NoProgress, CancellationToken.None);

    Assert.True(result.Succeeded, result.Error);
    Assert.True(result.Score > 0);
  }

  [Fact]
  public async Task GpuCompute_EitherSucceedsOrReportsNoDevice() {
    var result = await new GpuComputeBenchmark(groups: 64, iterations: 64, dispatches: 2)
        .RunAsync(NoProgress, CancellationToken.None);

    // On a headless CI host with no D3D11 device the suite must fail gracefully (not throw); on a
    // machine with a GPU it returns a positive GFLOPS number. Both are acceptable outcomes.
    if (result.Succeeded)
      Assert.True(result.Score > 0);
    else
      Assert.False(string.IsNullOrEmpty(result.Error));
  }
}
