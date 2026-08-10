using System.IO;
using Crystal.Service.Process;
using ProcessModule.Models;
using Xunit;

namespace ProcessModule.Tests;

// Covers the file-backed ProcessRecorder's CSV output against a real temp file: header, one row per
// sample, empty fields for null (ETW-unavailable) metrics, running peaks, CSV escaping, and the
// optional metrics-unavailable comment.
public class ProcessRecorderCsvTests : IDisposable {
  private readonly string _path = Path.Combine(Path.GetTempPath(), $"crystal-rec-{Guid.NewGuid():N}.csv");

  public void Dispose() {
    if (File.Exists(_path)) File.Delete(_path);
  }

  private static ProcessSample Sample(uint pid, string name, double cpu, double mem,
                                      double? gpu = null, double? disk = null, double? net = null) =>
      new(pid, name, cpu, mem, ProcessCategory.BackgroundProcess,
          GpuPercent: gpu, DiskBytesPerSec: disk, NetBytesPerSec: net);

  private static DateTimeOffset At(int second) =>
      new(2026, 8, 9, 14, 0, second, TimeSpan.Zero);

  [Fact]
  public void Start_writes_the_header() {
    var recorder = new ProcessRecorder();
    Assert.True(recorder.Start(_path).Succeeded);
    recorder.Stop();

    var lines = File.ReadAllLines(_path);
    Assert.Equal("Timestamp,PID,Name,CPU%,PeakCPU%,MemoryMB,PeakMemMB,GPU%,DiskBytesPerSec,NetBytesPerSec",
        lines[0]);
  }

  [Fact]
  public void Each_sample_appends_one_row_with_live_metrics() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path);
    recorder.WriteSample(Sample(200, "beta", cpu: 12.4, mem: 842, gpu: 3.2, disk: 0, net: 15360), At(1));
    recorder.Stop();

    var lines = File.ReadAllLines(_path);
    Assert.Equal(2, lines.Length); // header + 1 row
    Assert.Equal("2026-08-09T14:00:01,200,beta,12.4,12.4,842,842,3.2,0,15360", lines[1]);
  }

  [Fact]
  public void Null_metrics_are_written_as_empty_fields() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path);
    recorder.WriteSample(Sample(200, "beta", cpu: 5, mem: 100), At(1));
    recorder.Stop();

    var row = File.ReadAllLines(_path)[1];
    // GPU/Disk/Net are the trailing three fields — all blank, so the row ends with three commas.
    Assert.EndsWith("5.0,5.0,100,100,,,", row);
  }

  [Fact]
  public void Peaks_are_the_running_high_water_marks_across_rows() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path);
    recorder.WriteSample(Sample(200, "beta", cpu: 10, mem: 500), At(1));
    recorder.WriteSample(Sample(200, "beta", cpu: 40, mem: 300), At(2)); // cpu peak rises, mem dips
    recorder.WriteSample(Sample(200, "beta", cpu: 20, mem: 900), At(3)); // mem peak rises
    recorder.Stop();

    var lines = File.ReadAllLines(_path);
    // Row 3: live cpu 20 (peak 40), live mem 900 (peak 900).
    Assert.Equal("2026-08-09T14:00:03,200,beta,20.0,40.0,900,900,,,", lines[3]);
  }

  [Fact]
  public void A_name_with_a_comma_is_quoted() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path);
    recorder.WriteSample(Sample(200, "Foo, Bar.exe", cpu: 1, mem: 1), At(1));
    recorder.Stop();

    var row = File.ReadAllLines(_path)[1];
    Assert.Contains("\"Foo, Bar.exe\"", row);
  }

  [Fact]
  public void The_metrics_unavailable_reason_is_written_as_a_leading_comment() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path, metricsUnavailable: "not elevated");
    recorder.Stop();

    var lines = File.ReadAllLines(_path);
    Assert.StartsWith("# GPU/Disk/Network unavailable (not elevated)", lines[0]);
    Assert.StartsWith("Timestamp,", lines[1]); // header follows the comment
  }

  [Fact]
  public void Write_after_stop_is_a_noop() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path);
    recorder.Stop();
    recorder.WriteSample(Sample(200, "beta", cpu: 1, mem: 1), At(1));

    Assert.Single(File.ReadAllLines(_path)); // header only
  }

  [Fact]
  public void Start_twice_without_stopping_fails() {
    var recorder = new ProcessRecorder();
    Assert.True(recorder.Start(_path).Succeeded);
    var second = recorder.Start(_path);
    recorder.Stop();

    Assert.False(second.Succeeded);
  }

  [Fact]
  public void Sample_count_reflects_written_rows() {
    var recorder = new ProcessRecorder();
    recorder.Start(_path);
    recorder.WriteSample(Sample(200, "beta", cpu: 1, mem: 1), At(1));
    recorder.WriteSample(Sample(200, "beta", cpu: 2, mem: 2), At(2));

    Assert.Equal(2, recorder.SampleCount);
    recorder.Stop();
  }
}
