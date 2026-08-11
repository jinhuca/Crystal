using System.Collections.Frozen;
using Crystal.Provider.Etw;
using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Service.Process.Tests;

// ProcessMonitor reads processes via the ToSafeProcessMetricsAsync extension, which calls
// GetMultiMetricsForClassAsync(Win32_Process). This fake returns a fixed set of Win32_Process
// property bags every poll.
internal sealed class FakeWmiHardwareProvider(IReadOnlyList<FrozenDictionary<string, WmiValue>> instances)
    : IWmiHardwareProvider {
  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName, CancellationToken cancellationToken, bool bypassCache = false,
      IReadOnlyList<string>? projection = null)
    => Task.FromResult(instances);

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string namespaceName, string wmiClassName, CancellationToken cancellationToken)
    => Task.FromResult(instances);

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
      string namespaceName, string wqlQuery, CancellationToken cancellationToken)
    => throw new NotSupportedException();

  public Task<WmiMethodResult> InvokeStaticMethodAsync(
      string namespaceName, string wmiClassName, string methodName,
      IReadOnlyDictionary<string, WmiValue> inParameters, CancellationToken cancellationToken)
    => throw new NotSupportedException();
}

internal static class ProcessRows {
  // A Win32_Process row. WorkingSetSize/Kernel/UserModeTime are ULong per the extension's typed
  // getters; ProcessId/SessionId are Int. An unset key reads back as null.
  public static FrozenDictionary<string, WmiValue> Row(
      int? processId = null, string? name = null, ulong? workingSet = null,
      int? sessionId = null, string? status = null, string? executablePath = null,
      ulong? kernelTime = null, ulong? userTime = null) {
    var v = new Dictionary<string, WmiValue>();
    if (processId is { } pid) v["ProcessId"] = new WmiValue(pid);
    if (name is not null) v["Name"] = new WmiValue(name);
    if (workingSet is { } ws) v["WorkingSetSize"] = new WmiValue(ws);
    if (sessionId is { } sid) v["SessionId"] = new WmiValue(sid);
    if (status is not null) v["Status"] = new WmiValue(status);
    if (executablePath is not null) v["ExecutablePath"] = new WmiValue(executablePath);
    if (kernelTime is { } kt) v["KernelModeTime"] = new WmiValue(kt);
    if (userTime is { } ut) v["UserModeTime"] = new WmiValue(ut);
    return v.ToFrozenDictionary();
  }
}

// A steady-state ETW source: every SnapshotRates() returns the same fixed per-PID rates (unlike the
// real reader's destructive per-interval semantics), so any poll after the broadcaster's first
// emission deterministically sees the same overlay values.
internal sealed class FakeEtwSource(bool isRunning, string? startError,
                                    IReadOnlyDictionary<uint, ProcessEtwMetrics> rates)
    : IProcessEtwSource {
  public bool IsRunning { get; } = isRunning;
  public string? StartError { get; } = startError;
  public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() => rates;
  public void Pause() { }
  public void Resume() { }
  public void Dispose() { }
}
