using Crystal.Service.Process;

namespace Crystal.ProcessModule.Models;

/// <summary>
/// Records per-poll resource readings to a CSV file, so the user can investigate the raw data (open in
/// Excel, plot it). Kept behind an interface — like <see cref="IProcessController"/> — so the view model
/// decides <em>when</em> to record and <em>which</em> samples to write, while the file/IO side effects
/// stay here and can be faked in tests.
/// <para>
/// One recording can span several processes: <see cref="Start"/> opens the file and writes the header,
/// each <see cref="WriteSample"/> appends one row tagged by its PID/Name (per-PID peaks are tracked so
/// each process reports its own high-water marks), and the recording ends on <see cref="Stop"/> or when
/// the view model sees the last tracked process exit.
/// </para>
/// </summary>
public interface IProcessRecorder {
  /// <summary>True between a successful <see cref="Start"/> and the next <see cref="Stop"/>.</summary>
  bool IsActive { get; }

  /// <summary>Number of sample rows written since the current recording started (0 when inactive).</summary>
  int SampleCount { get; }

  /// <summary>Full path of the file being written, or null when inactive.</summary>
  string? FilePath { get; }

  /// <summary>
  /// Opens <paramref name="filePath"/> for writing and emits the CSV header. <paramref name="metricsUnavailable"/>
  /// (the ETW "not elevated" reason, or null when live) is recorded as a comment so a reader knows why
  /// the GPU/Disk/Network columns may be blank. Returns a failure result (never throws) if the file
  /// can't be opened; a no-op returning failure if a recording is already active.
  /// </summary>
  ProcessActionResult Start(string filePath, string? metricsUnavailable = null);

  /// <summary>Appends one CSV row for the given sample at <paramref name="timestamp"/>. No-op when
  /// inactive. Null GPU/Disk/Network values are written as empty fields so the columns stay numeric.</summary>
  void WriteSample(ProcessSample sample, DateTimeOffset timestamp);

  /// <summary>Closes the file and returns to the inactive state. No-op when not recording.</summary>
  void Stop();
}
