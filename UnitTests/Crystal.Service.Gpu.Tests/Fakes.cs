using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Telemetry.Hardware;
using System.Collections.Frozen;

namespace Crystal.Service.Gpu.Tests;

// Minimal ISensor stand-in for exercising GpuSensorSelector. Only Name/SensorType/Value are
// meaningful; the rest satisfy the interface. Mirrors the Sensors test project's StubSensor.
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

internal static class Sensors {
  public static StubSensor Of(SensorType type, string name, float? value) =>
      new() { SensorType = type, Name = name, Value = value };

  public static StubSensor Load(string name, float? value) => Of(SensorType.Load, name, value);
  public static StubSensor Clock(string name, float? value) => Of(SensorType.Clock, name, value);
  public static StubSensor Power(string name, float? value) => Of(SensorType.Power, name, value);
  public static StubSensor Temp(string name, float? value) => Of(SensorType.Temperature, name, value);
  public static StubSensor Fan(string name, float? value) => Of(SensorType.Fan, name, value);
  public static StubSensor Voltage(string name, float? value) => Of(SensorType.Voltage, name, value);
  public static StubSensor SmallData(string name, float? value) => Of(SensorType.SmallData, name, value);
  public static StubSensor Throughput(string name, float? value) => Of(SensorType.Throughput, name, value);
}

// GpuInfoBuilder reads adapters via the ToSafeVideoControllerMetricsAsync extension, which calls
// GetMultiMetricsForClassAsync(Win32_VideoController). This fake returns a fixed set of
// Win32_VideoController property bags.
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

// A canned load source: returns a fixed list of readings and records how many times Read() ran, so
// tests can assert the builder re-reads live load on each poll.
internal sealed class FakeGpuLoadSource(params GpuLoadReading[] readings) : IGpuLoadSource {
  public int ReadCount { get; private set; }
  public IReadOnlyList<GpuLoadReading> Read() {
    ReadCount++;
    return readings;
  }
}

internal static class VideoRows {
  // A Win32_VideoController row. AdapterRAM/resolutions are Int; leaving PNPDeviceID unset keeps
  // GpuInfoBuilder's registry-reading helpers (DriverDate/PhysicalLocation) on their null path, so
  // DriverDate falls back to InfDate deterministically without touching the real registry.
  public static FrozenDictionary<string, WmiValue> Controller(
      string? name = null, int? adapterRamBytes = null, string? driverVersion = null,
      string? videoProcessor = null, int? refreshRate = null,
      int? horizontalRes = null, int? verticalRes = null, DateTime? infDate = null) {
    var v = new Dictionary<string, WmiValue>();
    if (name is not null) v["Name"] = new WmiValue(name);
    if (adapterRamBytes is { } ram) v["AdapterRAM"] = new WmiValue(ram);
    if (driverVersion is not null) v["DriverVersion"] = new WmiValue(driverVersion);
    if (videoProcessor is not null) v["VideoProcessor"] = new WmiValue(videoProcessor);
    if (refreshRate is { } rr) v["CurrentRefreshRate"] = new WmiValue(rr);
    if (horizontalRes is { } hr) v["CurrentHorizontalResolution"] = new WmiValue(hr);
    if (verticalRes is { } vr) v["CurrentVerticalResolution"] = new WmiValue(vr);
    if (infDate is { } d) v["InfDate"] = new WmiValue(d);
    return v.ToFrozenDictionary();
  }
}
