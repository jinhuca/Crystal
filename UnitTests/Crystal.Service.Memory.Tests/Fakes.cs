using Crystal.Provider.Mmi.MmiEngine;
using System.Collections.Frozen;

namespace Crystal.Service.Memory.Tests;

// MemoryInfoBuilder queries two WMI classes — Win32_PhysicalMemory (the populated sticks) and
// Win32_PhysicalMemoryArray (the board's slot count) — via the ToSafe*MetricsAsync extensions, both
// of which call GetMultiMetricsForClassAsync(className). This fake routes by class name so a test
// can script the two result sets independently.
internal sealed class FakeWmiHardwareProvider(
    IReadOnlyList<FrozenDictionary<string, WmiValue>>? sticks = null,
    IReadOnlyList<FrozenDictionary<string, WmiValue>>? arrays = null)
    : IWmiHardwareProvider {
  private readonly IReadOnlyList<FrozenDictionary<string, WmiValue>> _sticks = sticks ?? [];
  private readonly IReadOnlyList<FrozenDictionary<string, WmiValue>> _arrays = arrays ?? [];

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string wmiClassName, CancellationToken cancellationToken, bool bypassCache = false,
      IReadOnlyList<string>? projection = null)
    => Task.FromResult(wmiClassName == "Win32_PhysicalMemoryArray" ? _arrays : _sticks);

  public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
      string namespaceName, string wmiClassName, CancellationToken cancellationToken)
    => Task.FromResult(wmiClassName == "Win32_PhysicalMemoryArray" ? _arrays : _sticks);

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
internal sealed class FakeMemoryLoadSource(MemoryLoadReading? reading = null) : IMemoryLoadSource {
  private readonly MemoryLoadReading _reading = reading ?? new MemoryLoadReading(0, null, null);
  public int ReadCount { get; private set; }
  public MemoryLoadReading Read() {
    ReadCount++;
    return _reading;
  }
}

internal static class MemoryRows {
  // A populated Win32_PhysicalMemory stick. SMBIOSMemoryType 34 = DDR5, FormFactor 8 = DIMM.
  public static FrozenDictionary<string, WmiValue> Stick(
      string? deviceLocator = null, ulong? capacityBytes = null, int? speed = null,
      int? configuredSpeed = null, int? formFactor = null, int? smbiosType = null,
      string? manufacturer = null, string? partNumber = null, string? serial = null,
      string? bankLabel = null) {
    var v = new Dictionary<string, WmiValue>();
    if (deviceLocator is not null) v["DeviceLocator"] = new WmiValue(deviceLocator);
    if (bankLabel is not null) v["BankLabel"] = new WmiValue(bankLabel);
    if (capacityBytes is { } c) v["Capacity"] = new WmiValue(c);
    if (speed is { } s) v["Speed"] = new WmiValue(s);
    if (configuredSpeed is { } cs) v["ConfiguredClockSpeed"] = new WmiValue(cs);
    if (formFactor is { } f) v["FormFactor"] = new WmiValue(f);
    if (smbiosType is { } t) v["SMBIOSMemoryType"] = new WmiValue(t);
    if (manufacturer is not null) v["Manufacturer"] = new WmiValue(manufacturer);
    if (partNumber is not null) v["PartNumber"] = new WmiValue(partNumber);
    if (serial is not null) v["SerialNumber"] = new WmiValue(serial);
    return v.ToFrozenDictionary();
  }

  // A Win32_PhysicalMemoryArray row; MemoryDevices is the total slot count on the board.
  public static FrozenDictionary<string, WmiValue> Array(int memoryDevices) =>
      new Dictionary<string, WmiValue> { ["MemoryDevices"] = new WmiValue(memoryDevices) }
          .ToFrozenDictionary();
}
