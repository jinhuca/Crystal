using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Telemetry.Hardware;
using System.Collections.Frozen;

namespace Crystal.Service.Storage.Tests;

// Minimal ISensor stand-in for exercising StorageSensorSelector. Only Name/SensorType/Value are
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

// StorageInfoBuilder calls the ToSafeDiskDriveMetricsAsync extension, which under the hood invokes
// GetMultiMetricsForClassAsync(Win32_DiskDrive). So the fake speaks the raw WmiValue property-bag
// contract rather than returning DiskDriveMetrics directly. Keys match Win32_DiskDrive property
// names; the value WmiType must match what the extension's typed getters expect (Size/Index are
// read as ULong/Int respectively).
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

// A canned load source: returns a fixed reading and records how many times Read() ran, so the
// monitor test can assert one poll per interval and cold-until-subscribed behavior.
internal sealed class FakeStorageLoadSource(StorageLoadReading? reading = null) : IStorageLoadSource {
  private readonly StorageLoadReading _reading = reading ?? new StorageLoadReading([]);
  public int ReadCount { get; private set; }
  public StorageLoadReading Read() {
    ReadCount++;
    return _reading;
  }
}

// Fluent builder for a Win32_DiskDrive property bag; only sets the keys the test cares about so an
// unset property reads back as null (the WmiType guard in the extension makes a missing key null).
internal sealed class DiskRow {
  private readonly Dictionary<string, WmiValue> _values = new();

  public DiskRow Str(string key, string value) { _values[key] = new WmiValue(value); return this; }
  public DiskRow Int(string key, int value) { _values[key] = new WmiValue(value); return this; }
  public DiskRow ULong(string key, ulong value) { _values[key] = new WmiValue(value); return this; }

  public FrozenDictionary<string, WmiValue> Build() => _values.ToFrozenDictionary();

  public static FrozenDictionary<string, WmiValue> Drive(
      string? model = null, ulong? sizeBytes = null, int? index = null,
      string? interfaceType = null, string? mediaType = null, string? manufacturer = null,
      int? partitions = null, string? caption = null, string? serial = null, string? firmware = null) {
    var row = new DiskRow();
    if (model is not null) row.Str("Model", model);
    if (caption is not null) row.Str("Caption", caption);
    if (sizeBytes is { } s) row.ULong("Size", s);
    if (index is { } i) row.Int("Index", i);
    if (interfaceType is not null) row.Str("InterfaceType", interfaceType);
    if (mediaType is not null) row.Str("MediaType", mediaType);
    if (manufacturer is not null) row.Str("Manufacturer", manufacturer);
    if (partitions is { } p) row.Int("Partitions", p);
    if (serial is not null) row.Str("SerialNumber", serial);
    if (firmware is not null) row.Str("FirmwareRevision", firmware);
    return row.Build();
  }
}
