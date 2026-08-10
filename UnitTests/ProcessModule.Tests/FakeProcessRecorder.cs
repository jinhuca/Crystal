using ProcessModule.Models;

namespace ProcessModule.Tests;

// In-memory IProcessRecorder: captures Start/WriteSample/Stop calls so the view model's recording
// orchestration can be asserted without touching disk. Set NextStartResult to exercise the
// open-failure path.
internal sealed class FakeProcessRecorder : IProcessRecorder {
  public ProcessActionResult NextStartResult { get; set; } = ProcessActionResult.Ok;

  public bool IsActive { get; private set; }
  public int SampleCount { get; private set; }
  public string? FilePath { get; private set; }

  public int StartCallCount { get; private set; }
  public int StopCallCount { get; private set; }
  public string? MetricsUnavailablePassed { get; private set; }

  // Every sample handed to WriteSample, in order — lets a test assert exactly which PID's readings
  // were recorded and how many.
  public List<(ProcessSample Sample, DateTimeOffset Timestamp)> Written { get; } = [];

  public ProcessActionResult Start(string filePath, string? metricsUnavailable = null) {
    StartCallCount++;
    if (!NextStartResult.Succeeded) return NextStartResult;
    IsActive = true;
    FilePath = filePath;
    SampleCount = 0;
    MetricsUnavailablePassed = metricsUnavailable;
    return NextStartResult;
  }

  public void WriteSample(ProcessSample sample, DateTimeOffset timestamp) {
    if (!IsActive) return;
    Written.Add((sample, timestamp));
    SampleCount++;
  }

  public void Stop() {
    if (!IsActive) return;
    StopCallCount++;
    IsActive = false;
    // FilePath and SampleCount are intentionally left as-is until the next Start, so the view model
    // can read them right after calling Stop() to report where the recording was saved.
  }
}
