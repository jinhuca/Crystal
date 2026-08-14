using Crystal.Service.Process;
using System.Globalization;
using System.IO;

namespace Crystal.ProcessModule.Models;

/// <summary>
/// StreamWriter-backed <see cref="IProcessRecorder"/>: writes one CSV row per poll to a file the user
/// picked. The writer is held open for the recording and flushed after every row, so a crash mid-run
/// still leaves a readable file. Every IO failure (open/write) is caught and reported rather than
/// crashing the dashboard — recording is a convenience, never worth taking the app down for.
/// </summary>
public sealed class ProcessRecorder : IProcessRecorder {
  // ISO-8601 to the second: sortable, unambiguous, and parsed natively by Excel and plotting tools.
  private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss";
  private const string Header =
      "Timestamp,PID,Name,CPU%,PeakCPU%,MemoryMB,PeakMemMB,GPU%,DiskBytesPerSec,NetBytesPerSec";

  private StreamWriter? _writer;
  private double _peakCpu;
  private double _peakMem;

  public bool IsActive => _writer is not null;
  public int SampleCount { get; private set; }
  public string? FilePath { get; private set; }

  public ProcessActionResult Start(string filePath, string? metricsUnavailable = null) {
    if (IsActive) return ProcessActionResult.Fail("A recording is already in progress.");
    if (string.IsNullOrWhiteSpace(filePath)) return ProcessActionResult.Fail("Choose a file to record to.");

    try {
      var writer = new StreamWriter(filePath, append: false);
      // A leading comment documents the source; a numeric-tool reader skips '#' lines, and the ETW
      // note explains why GPU/Disk/Network may be blank rather than genuinely zero.
      if (!string.IsNullOrEmpty(metricsUnavailable))
        writer.WriteLine($"# GPU/Disk/Network unavailable ({metricsUnavailable}) — left blank");
      writer.WriteLine(Header);
      writer.Flush();

      _writer = writer;
      FilePath = filePath;
      SampleCount = 0;
      _peakCpu = 0;
      _peakMem = 0;
      return ProcessActionResult.Ok;
    }
    catch (IOException ex) {
      return ProcessActionResult.Fail($"Couldn't start recording: {ex.Message}");
    }
    catch (UnauthorizedAccessException ex) {
      return ProcessActionResult.Fail($"Couldn't start recording: {ex.Message}");
    }
  }

  public void WriteSample(ProcessSample sample, DateTimeOffset timestamp) {
    if (_writer is null) return;

    // The recorder owns the running peaks so the file is self-contained — a reader gets the session
    // high-water marks without needing the live row VM.
    if (sample.CpuPercent > _peakCpu) _peakCpu = sample.CpuPercent;
    if (sample.WorkingSetMb > _peakMem) _peakMem = sample.WorkingSetMb;

    string line = string.Join(',',
        timestamp.ToString(TimestampFormat, CultureInfo.InvariantCulture),
        sample.ProcessId.ToString(CultureInfo.InvariantCulture),
        CsvField(sample.Name),
        Num(sample.CpuPercent, "0.0"),
        Num(_peakCpu, "0.0"),
        Num(sample.WorkingSetMb, "0"),
        Num(_peakMem, "0"),
        NullableNum(sample.GpuPercent, "0.0"),
        NullableNum(sample.DiskBytesPerSec, "0"),
        NullableNum(sample.NetBytesPerSec, "0"));

    try {
      _writer.WriteLine(line);
      _writer.Flush();
      SampleCount++;
    }
    catch (IOException) {
      // Disk full / device removed mid-run: stop rather than throw on every subsequent poll. The
      // rows written so far remain valid.
      Stop();
    }
  }

  public void Stop() {
    if (_writer is null) return;
    try { _writer.Dispose(); } catch (IOException) { /* best effort flush-on-close */ }
    _writer = null;
    FilePath = null;
  }

  private static string Num(double value, string format) =>
      value.ToString(format, CultureInfo.InvariantCulture);

  // A null metric (ETW not live) is written as an empty field, not a placeholder, so the column
  // stays numeric for downstream plotting rather than mixing in a "-" string.
  private static string NullableNum(double? value, string format) =>
      value is { } v ? v.ToString(format, CultureInfo.InvariantCulture) : "";

  // Quote and escape a value only when it contains a comma, quote, or newline (RFC 4180), so a
  // process name like "Foo, Bar.exe" can't shift later columns.
  private static string CsvField(string value) {
    if (value.IndexOfAny([',', '"', '\n', '\r']) < 0) return value;
    return $"\"{value.Replace("\"", "\"\"")}\"";
  }
}
