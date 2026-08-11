using Crystal.Provider.Etw;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Network.Tests;

// Minimal ISensor stand-in for exercising NetworkSensorSelector. Only Name/SensorType/Value are
// meaningful; the rest satisfy the interface. Mirrors the other test projects' StubSensor.
internal sealed class StubSensor : ISensor {
  public string Name { get; set; } = string.Empty;
  public SensorType SensorType { get; set; }
  public float? Value { get; set; }
  public float? Min { get; set; }
  public float? Max { get; set; }

  public IControl Control => null!;
  public IHardware Hardware => null!;
  public Identifier Identifier => new("stub", "sensor");
  public int Index => 0;
  public bool IsDefaultHidden => false;
  public IReadOnlyList<IParameter> Parameters => Array.Empty<IParameter>();
  public IEnumerable<SensorValue> Values => Array.Empty<SensorValue>();
  public TimeSpan ValuesTimeWindow { get; set; }

  public void ResetMin() { }
  public void ResetMax() { }
  public void ClearValues() { }
  public void Accept(IVisitor visitor) { }
  public void Traverse(IVisitor visitor) { }
}

// A canned load source: returns a fixed snapshot and records how many times Read() ran, so tests can
// assert the monitor polls once per interval and only while subscribed.
internal sealed class FakeNetworkLoadSource(NetworkSnapshot? snapshot = null) : INetworkLoadSource {
  private readonly NetworkSnapshot _snapshot = snapshot ?? new NetworkSnapshot([]);
  public int ReadCount { get; private set; }
  public NetworkSnapshot Read() {
    ReadCount++;
    return _snapshot;
  }
}

// A steady-state ETW source: always reports running with no per-process activity, enough to build a
// real ProcessNetworkSource for the monitor's forwarded TopTalkers stream (ProcessNetworkSource has
// its own dedicated tests, so this stays minimal).
internal sealed class FakeEtwSource : IProcessEtwSource {
  public bool IsRunning => true;
  public string? StartError => null;
  public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() =>
      new Dictionary<uint, ProcessEtwMetrics>();
  public void Pause() { }
  public void Resume() { }
  public void Dispose() { }
}
