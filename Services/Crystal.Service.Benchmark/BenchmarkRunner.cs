namespace Crystal.Service.Benchmark;

/// <summary>
/// Runs a selected set of suites <b>sequentially</b> — two suites measuring the same resource at
/// once would each report a depressed number, so the batch is serialized. Per-suite lifecycle is
/// surfaced through the callbacks (started → progress* → completed) so the view model can drive the
/// dashboard rows without knowing how a suite works. A suite that fails its "can run here" check
/// returns a failed <see cref="BenchmarkResult"/> and the batch continues; cancellation stops the
/// batch promptly (the in-flight suite observes the token between work chunks).
/// </summary>
public sealed class BenchmarkRunner {
  /// <summary>
  /// Runs each suite in <paramref name="suites"/> in order. Callbacks fire on the thread the suite
  /// completes on (a background thread) — the caller marshals to the UI. Returns when every suite
  /// has finished; throws <see cref="OperationCanceledException"/> if cancelled mid-batch.
  /// </summary>
  public async Task RunAsync(
      IReadOnlyList<IBenchmark> suites,
      Action<IBenchmark> onSuiteStarted,
      Action<IBenchmark, BenchmarkProgress> onSuiteProgress,
      Action<IBenchmark, BenchmarkResult> onSuiteCompleted,
      CancellationToken ct) {
    ArgumentNullException.ThrowIfNull(suites);

    foreach (var suite in suites) {
      ct.ThrowIfCancellationRequested();
      onSuiteStarted(suite);

      var progress = new SynchronousProgress(p => onSuiteProgress(suite, p));
      var start = System.Diagnostics.Stopwatch.GetTimestamp();
      BenchmarkResult result;
      try {
        result = await suite.RunAsync(progress, ct).ConfigureAwait(false);
      } catch (OperationCanceledException) {
        // Report the partial suite as cancelled, then stop the whole batch.
        onSuiteCompleted(suite, BenchmarkResult.Failure(suite.Id, "Cancelled",
            System.Diagnostics.Stopwatch.GetElapsedTime(start)));
        throw;
      } catch (Exception ex) {
        result = BenchmarkResult.Failure(suite.Id, ex.Message,
            System.Diagnostics.Stopwatch.GetElapsedTime(start));
      }

      onSuiteCompleted(suite, result);
    }
  }

  // Invokes the progress handler synchronously on the suite's thread, unlike System.Threading's
  // Progress<T>, which posts to the captured SynchronizationContext asynchronously. Synchronous
  // dispatch keeps progress ordered with the started/completed callbacks (all fire directly on the
  // background thread; the caller marshals to the UI), and avoids dropping a final tick when a suite
  // reports progress and then completes immediately.
  private sealed class SynchronousProgress(Action<BenchmarkProgress> handler) : IProgress<BenchmarkProgress> {
    public void Report(BenchmarkProgress value) => handler(value);
  }
}
