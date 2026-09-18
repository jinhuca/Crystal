using System.Diagnostics;

namespace Crystal.Service.Benchmark.Suites;

/// <summary>
/// Sequential storage-throughput suite: writes a large temp file in fixed blocks then reads it back,
/// reporting write and read MB/s. Writes use <see cref="FileOptions.WriteThrough"/> so the OS write
/// cache is bypassed and the number reflects the device, not RAM. The headline score is the write
/// rate (the more device-bound of the two — reads of a just-written file may be partly served from
/// the page cache, which the detail line notes). The temp file is always deleted.
/// </summary>
public sealed class StorageSequentialBenchmark : IBenchmark {
  private readonly long _fileBytes;
  private readonly int _blockBytes;

  public StorageSequentialBenchmark(long fileBytes = 512L * 1024 * 1024, int blockBytes = 4 * 1024 * 1024) {
    _fileBytes = fileBytes;
    _blockBytes = blockBytes;
  }

  public string Id => "storage.sequential";
  public string Name => "Sequential (write / read)";
  public BenchmarkCategory Category => BenchmarkCategory.Storage;
  public string Description => "Large sequential write-through then read of a temp file on the system drive.";
  public string Unit => "MB/s";
  public bool HigherIsBetter => true;

  public Task<BenchmarkResult> RunAsync(IProgress<BenchmarkProgress> progress, CancellationToken ct) =>
      Task.Run(() => {
        string path = Path.Combine(Path.GetTempPath(), $"crystal-bench-{Guid.NewGuid():N}.tmp");
        long blocks = _fileBytes / _blockBytes;
        var block = new byte[_blockBytes];
        new Random(7).NextBytes(block);
        double mbTotal = _fileBytes / (1024.0 * 1024.0);

        try {
          var writeSw = Stopwatch.StartNew();
          using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                     _blockBytes, FileOptions.WriteThrough | FileOptions.SequentialScan)) {
            for (long i = 0; i < blocks; i++) {
              ct.ThrowIfCancellationRequested();
              fs.Write(block, 0, _blockBytes);
              if ((i & 7) == 0) progress.Report(BenchmarkProgress.At(0.5 * (i + 1) / blocks, "Writing"));
            }
            fs.Flush(flushToDisk: true);
          }
          writeSw.Stop();

          var readSw = Stopwatch.StartNew();
          using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None,
                     _blockBytes, FileOptions.SequentialScan)) {
            for (long i = 0; i < blocks; i++) {
              ct.ThrowIfCancellationRequested();
              int read = 0;
              while (read < _blockBytes) {
                int r = fs.Read(block, read, _blockBytes - read);
                if (r == 0) break;
                read += r;
              }
              if ((i & 7) == 0) progress.Report(BenchmarkProgress.At(0.5 + 0.5 * (i + 1) / blocks, "Reading"));
            }
          }
          readSw.Stop();

          double writeMbps = mbTotal / writeSw.Elapsed.TotalSeconds;
          double readMbps = mbTotal / readSw.Elapsed.TotalSeconds;
          string detail = $"write {writeMbps:N0} MB/s · read {readMbps:N0} MB/s ({mbTotal:N0} MB)";
          return BenchmarkResult.Success(Id, writeMbps, Unit, detail, writeSw.Elapsed + readSw.Elapsed);
        } finally {
          TryDelete(path);
        }
      }, ct);

  private static void TryDelete(string path) {
    try { if (File.Exists(path)) File.Delete(path); } catch { /* best effort temp cleanup */ }
  }
}
