using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Random-access storage suite: after laying down a temp file, it issues many small (4 KiB)
/// write-through reads/writes at random block-aligned offsets and reports IOPS (I/O operations per
/// second). Small random I/O is the access pattern that most separates SSDs from spinning disks.
/// Writes are write-through so they hit the device; the file is always deleted.
/// </summary>
public sealed class StorageRandomBenchmark : IBenchmark {
  /// <summary>
  /// The total size of the temp file the random I/O is spread across, in bytes. The default is
  /// 256 MB — large enough that offsets don't cluster in one region.
  /// </summary>
  private readonly long _fileBytes;

  /// <summary>
  /// The size of each I/O operation, in bytes. The default is 4 KiB, the canonical small-random
  /// block size for IOPS measurement.
  /// </summary>
  private readonly int _blockBytes;

  /// <summary>
  /// The number of random I/O operations to issue in the timed phase. The default is 20,000, split
  /// roughly 50/50 between reads and writes.
  /// </summary>
  private readonly int _operations;

  /// <summary>
  /// Creates a new instance of the <see cref="StorageRandomBenchmark"/> class with the specified
  /// file size, block size, and operation count.
  /// </summary>
  /// <param name="fileBytes">The total size of the temp file, in bytes.</param>
  /// <param name="blockBytes">The size of each I/O operation, in bytes.</param>
  /// <param name="operations">The number of random I/O operations to issue.</param>
  public StorageRandomBenchmark(long fileBytes = 256L * 1024 * 1024, int blockBytes = 4 * 1024, int operations = 20_000) {
    _fileBytes = fileBytes;
    _blockBytes = blockBytes;
    _operations = operations;
  }

  /// <summary>
  /// Gets the unique identifier for this benchmark suite.
  /// </summary>
  public string Id => "storage.random";

  /// <summary>
  /// Gets the display name for this benchmark suite.
  /// </summary>
  public string Name => "Random 4 KiB (IOPS)";

  /// <summary>
  /// Gets the category of this benchmark suite, which is Storage.
  /// </summary>
  public BenchmarkCategory Category => BenchmarkCategory.Storage;

  /// <summary>
  /// Gets a one-line description of what this benchmark suite measures.
  /// </summary>
  public string Description => "Small random-offset write-through I/O on a temp file — 4 KiB IOPS.";

  /// <summary>
  /// Gets the unit of the headline score for this benchmark suite, which is IOPS (I/O operations per second).
  /// </summary>
  public string Unit => "IOPS";

  /// <summary>
  /// Gets a value indicating whether a higher score is better. IOPS is a throughput metric, so higher is better.
  /// </summary>
  public bool HigherIsBetter => true;

  /// <summary>
  /// Runs the benchmark asynchronously, reporting progress and supporting cancellation. It first
  /// lays down a temp file of the target size, then issues alternating write-through reads and
  /// writes at random block-aligned offsets and reports the achieved IOPS. The temp file is always
  /// deleted.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="ct">The cancellation token.</param>
  /// <returns>A task returning the benchmark result.</returns>
  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
      Task.Run(() => {
        string path = Path.Combine(Path.GetTempPath(), $"crystal-bench-{Guid.NewGuid():N}.tmp");
        long blockCount = _fileBytes / _blockBytes;
        var block = new byte[_blockBytes];
        new Random(11).NextBytes(block);
        var rng = new Random(13);

        try {
          progress.Report(BenchmarkProgress.At(0, "Preparing file"));
          using (var init = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, _blockBytes)) {
            init.SetLength(_fileBytes);
            for (long i = 0; i < blockCount; i++) {
              ct.ThrowIfCancellationRequested();
              init.Write(block, 0, _blockBytes);
            }
            init.Flush(flushToDisk: true);
          }

          var sw = Stopwatch.StartNew();
          using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None,
                     _blockBytes, FileOptions.WriteThrough | FileOptions.RandomAccess)) {
            for (int op = 0; op < _operations; op++) {
              ct.ThrowIfCancellationRequested();
              long offset = (long)rng.Next((int)blockCount) * _blockBytes;
              fs.Seek(offset, SeekOrigin.Begin);
              if ((op & 1) == 0) {
                fs.ReadExactly(block, 0, _blockBytes);
              } else {
                fs.Write(block, 0, _blockBytes);
              }
              if ((op & 1023) == 0) progress.Report(BenchmarkProgress.At((op + 1) / (double)_operations, "Random I/O"));
            }
          }
          sw.Stop();

          double iops = _operations / sw.Elapsed.TotalSeconds;
          string detail = $"{_operations:N0} ops (50/50 R/W) in {sw.Elapsed.TotalSeconds:N2} s";
          return BenchmarkResult.Success(Id, iops, Unit, detail, sw.Elapsed);
        } finally {
          TryDelete(path);
        }
      }, ct);

  /// <summary>
  /// Attempts to delete the specified file, ignoring any exceptions. Best-effort cleanup of the
  /// temporary file created during the benchmark.
  /// </summary>
  /// <param name="path">The path of the file to delete.</param>
  private static void TryDelete(string path) {
    try { if (File.Exists(path)) File.Delete(path); } catch { /* best effort temp cleanup */ }
  }
}
