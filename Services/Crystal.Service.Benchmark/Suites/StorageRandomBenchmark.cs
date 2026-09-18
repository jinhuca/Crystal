using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Random-access storage suite: after laying down a temp file, it issues many small (4 KiB)
/// write-through reads/writes at random block-aligned offsets and reports IOPS (I/O operations per
/// second). Small random I/O is the access pattern that most separates SSDs from spinning disks.
/// Writes are write-through so they hit the device; the file is always deleted.
/// </summary>
public sealed class StorageRandomBenchmark : IBenchmark {
  private readonly long _fileBytes;
  private readonly int _blockBytes;
  private readonly int _operations;

  public StorageRandomBenchmark(long fileBytes = 256L * 1024 * 1024, int blockBytes = 4 * 1024, int operations = 20_000) {
    _fileBytes = fileBytes;
    _blockBytes = blockBytes;
    _operations = operations;
  }

  public string Id => "storage.random";
  public string Name => "Random 4 KiB (IOPS)";
  public BenchmarkCategory Category => BenchmarkCategory.Storage;
  public string Description => "Small random-offset write-through I/O on a temp file — 4 KiB IOPS.";
  public string Unit => "IOPS";
  public bool HigherIsBetter => true;

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

  private static void TryDelete(string path) {
    try { if (File.Exists(path)) File.Delete(path); } catch { /* best effort temp cleanup */ }
  }
}
