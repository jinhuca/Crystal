using Crystal.Smbios;
using Crystal.Smbios.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Smbios.Structures;

/// <summary>
/// High-level façade over the parsed SMBIOS data.
/// Obtain an instance via <see cref="Load"/> (Windows) or
/// <see cref="FromRawTableData"/> (injected bytes for testing / cross-platform).
/// </summary>
public sealed class SmbiosTable {
  // ── Public properties ─────────────────────────────────────────────────────

  public byte MajorVersion { get; }
  public byte MinorVersion { get; }

  /// <summary>All raw structures in table order.</summary>
  public IReadOnlyList<SmbiosRawStructure> RawStructures { get; }

  // ── Decoded structure collections ─────────────────────────────────────────

  /// <summary>BIOS Information structures (Type 0). Normally exactly one.</summary>
  public IReadOnlyList<T000_BiosInformation> BiosInformation { get; }
  /// <summary>System Information structures (Type 1). Normally exactly one.</summary>
  public IReadOnlyList<T001_SystemInformation> SystemInformation { get; }
  /// <summary>Baseboard Information structures (Type 2). One per physical board.</summary>
  public IReadOnlyList<T002_BaseboardInformation> BaseboardInformation { get; }
  /// <summary>Chassis Information structures (Type 3).</summary>
  public IReadOnlyList<T003_ChassisInformation> ChassisInformation { get; }
  /// <summary>Processor Information structures (Type 4). One per socket.</summary>
  public IReadOnlyList<T004_ProcessorInformation> ProcessorInformation { get; }
  /// <summary>Cache Information structures (Type 7). One per cache level per processor.</summary>
  public IReadOnlyList<T007_CacheInformation> CacheInformation { get; }
  /// <summary>Physical Memory Array structures (Type 16). Usually one per system board.</summary>
  public IReadOnlyList<T016_PhysicalMemoryArray> PhysicalMemoryArrays { get; }
  /// <summary>Memory Array Mapped Address structures (Type 19).</summary>
  public IReadOnlyList<T019_MemoryArrayMappedAddress> MemoryArrayMappedAddresses { get; }
  /// <summary>Memory Device Mapped Address structures (Type 20).</summary>
  public IReadOnlyList<T020_MemoryDeviceMappedAddress> MemoryDeviceMappedAddresses { get; }
  /// <summary>Memory Device structures (Type 17). One per DIMM slot.</summary>
  public IReadOnlyList<T017_MemoryDevice> MemoryDevices { get; }
  /// <summary>System Slot structures (Type 9). One per physical expansion slot (PCIe, M.2, etc.).</summary>
  public IReadOnlyList<T009_SystemSlotInformation> SystemSlots { get; }
  /// <summary>Onboard Devices Extended structures (Type 41). One per built-in device (NIC, audio, etc.).</summary>
  public IReadOnlyList<T041_OnboardDeviceExtendedInformation> OnboardDevices { get; }
  /// <summary>OEM Strings structures (Type 11). Contains free-form strings from the vendor.</summary>
  public IReadOnlyList<T011_OemStrings> OemStrings { get; }
  /// <summary>Port Connector structures (Type 8). One per physical port (USB, HDMI, RJ-45, etc.).</summary>
  public IReadOnlyList<T008_PortConnectorInformation> PortConnectors { get; }
  /// <summary>Portable Battery structures (Type 22). One per battery pack.</summary>
  public IReadOnlyList<T022_PortableBatteryInformation> Batteries { get; }
  /// <summary>Built-in Pointing Device structures (Type 21). One per integrated pointing device.</summary>
  public IReadOnlyList<T021_BuiltInPointingDevice> BuiltInPointingDevices { get; }
  /// <summary>BIOS Language structures (Type 13). Describes current and installable BIOS languages.</summary>
  public IReadOnlyList<T013_BiosLanguage> BiosLanguages { get; }
  /// <summary>Memory Error Information (Type 18/33)</summary>
  public IReadOnlyList<IMemoryErrorInformation> MemoryErrorInformation { get; }
  /// <summary>Typed discriminated wrappers for memory error entries.</summary>
  public IReadOnlyList<MemoryErrorEntry> MemoryErrorEntries { get; }

  public IReadOnlyList<T026_VoltageProbeInformation> VoltageProbes { get; }

  /// <summary>Cooling Devices (Type 27)</summary>
  public IReadOnlyList<T027_CoolingDevice> CoolingDevices { get; }
  /// <summary>Temperature Probes (Type 28)</summary>
  public IReadOnlyList<T028_TemperatureProbeInformation> TemperatureProbes { get; }
  /// <summary>System Power Supplies (Type 39)</summary>
  public IReadOnlyList<T039_SystemPowerSupply> PowerSupplies { get; }

  // --- Changes: add property declaration and decode call for Type 15 ---

  // Add this property to the public properties section (near GroupAssociations / PhysicalMemoryArrays)
  public IReadOnlyList<T015_SystemEventLog> SystemEventLogs { get; }


  // ── Convenience OEM helpers ─────────────────────────────────────────────

  /// <summary>
  /// Returns the first OEM string from the first Type 11 record, or null when
  /// no OEM strings are present. Useful for quick access to a common vendor
  /// string without inspecting the full structure list.
  /// </summary>
  public string? FirstOemString => OemStrings.FirstOrDefault()?.Strings.FirstOrDefault();

  /// <summary>
  /// Returns a dictionary mapping the OEM-structure handle to the first
  /// decoded string in that structure (or null when the structure contains
  /// no strings). The dictionary is built lazily each call; callers that
  /// need repeated access should cache the result.
  /// </summary>
  public IReadOnlyDictionary<ushort, string?> OemStringsMap => _oemStringsMap.Value;

  /// <summary>
  /// Returns a dictionary mapping the OEM-structure handle to the full list
  /// of decoded strings from that structure. Useful when callers need the
  /// complete string set rather than only the first entry.
  /// </summary>
  public IReadOnlyDictionary<ushort, IReadOnlyList<string>> OemStringsFullMap => _oemStringsFullMap.Value;

  // ── Convenience accessors ─────────────────────────────────────────────────

  public T000_BiosInformation? Bios => BiosInformation.FirstOrDefault();
  public T001_SystemInformation? System => SystemInformation.FirstOrDefault();
  public T002_BaseboardInformation? Baseboard => BaseboardInformation.FirstOrDefault();
  public T003_ChassisInformation? Chassis => ChassisInformation.FirstOrDefault();

  /// <summary>Populated processor sockets only.</summary>
  public IEnumerable<T004_ProcessorInformation> PopulatedProcessors =>
      ProcessorInformation.Where(p => p.IsPopulated);

  /// <summary>Installed DIMM slots only (modules that are actively populated).</summary>
  public IEnumerable<T017_MemoryDevice> InstalledMemoryDevices =>
      MemoryDevices.Where(m => m.IsPopulated);

  /// <summary>Total installed RAM in MiB across all populated DIMMs.</summary>
  public long TotalInstalledMemoryMiB =>
      InstalledMemoryDevices.Sum(m => m.CapacityBytes / (1024L * 1024L));

  /// <summary>Expansion slots that currently have a card/module installed.</summary>
  public IEnumerable<T009_SystemSlotInformation> PopulatedSlots =>
      SystemSlots.Where(sl => sl.IsInUse);

  /// <summary>Onboard devices that are currently enabled.</summary>
  public IEnumerable<T041_OnboardDeviceExtendedInformation> EnabledOnboardDevices =>
      OnboardDevices.Where(d => d.IsEnabled);

  // ── Relationship lookups (lazy — built once, on first use) ────────────────

  private readonly Lazy<Dictionary<ushort, T007_CacheInformation>> _cachesByHandle;
  private readonly Lazy<Dictionary<ushort, T016_PhysicalMemoryArray>> _memoryArraysByHandle;
  private readonly Lazy<Dictionary<ushort, string?>> _oemStringsMap;
  private readonly Lazy<Dictionary<ushort, IReadOnlyList<string>>> _oemStringsFullMap;

  /// <summary>
  /// Resolves the L1/L2/L3 cache structures for a given processor via its
  /// cache handle fields. Skips any handle equal to 0xFFFF (no cache of
  /// that level) or that doesn't resolve to a structure in this table.
  /// Returned in level order (L1, L2, L3) when present.
  /// </summary>
  public IReadOnlyList<T007_CacheInformation> GetCachesFor(T004_ProcessorInformation processor) {
    var byHandle = _cachesByHandle.Value;
    var result = new List<T007_CacheInformation>(3);

    TryAdd(processor.L1CacheHandle);
    TryAdd(processor.L2CacheHandle);
    TryAdd(processor.L3CacheHandle);

    return result;

    void TryAdd(ushort handle) {
      if (handle != 0xFFFF && byHandle.TryGetValue(handle, out var cache))
        result.Add(cache);
    }
  }

  /// <summary>
  /// Resolves the parent Physical Memory Array for a given Memory Device
  /// (Type 17), or null if the referenced array handle is not present in
  /// this table.
  /// </summary>
  public T016_PhysicalMemoryArray? GetArrayFor(T017_MemoryDevice device) =>
      _memoryArraysByHandle.Value.TryGetValue(device.PhysicalMemoryArrayHandle, out var array)
          ? array
          : null;

  /// <summary>All Memory Device slots (Type 17) that belong to a given array.</summary>
  public IEnumerable<T017_MemoryDevice> GetDevicesIn(T016_PhysicalMemoryArray array) =>
      MemoryDevices.Where(d => d.PhysicalMemoryArrayHandle == array.Handle);


  // ── Factory methods ───────────────────────────────────────────────────────

  /// <summary>
  /// Reads the SMBIOS table from the Windows firmware and decodes it.
  /// </summary>
  /// <exception cref="PlatformNotSupportedException">Not on Windows.</exception>
  public static SmbiosTable Load() {
    var (tableData, major, minor) = WindowsSmbiosReader.ReadTableData();
    return FromRawTableData(tableData, major, minor);
  }

  /// <summary>
  /// Decodes an already-fetched structure-table blob (no entry-point header).
  /// Useful for testing or for supplying data from dmidecode dumps, etc.
  /// </summary>
  public static SmbiosTable FromRawTableData(
      byte[] tableData, byte majorVersion = 0, byte minorVersion = 0) {
    ArgumentNullException.ThrowIfNull(tableData);
    var rawStructures = SmbiosTableParser.Parse(tableData);
    return new SmbiosTable(rawStructures, majorVersion, minorVersion);
  }

  // ── Private constructor ───────────────────────────────────────────────────

  private SmbiosTable(
      IReadOnlyList<SmbiosRawStructure> rawStructures,
      byte major,
      byte minor) {
    MajorVersion = major;
    MinorVersion = minor;
    RawStructures = rawStructures;

    BiosInformation = Decode<T000_BiosInformation>(rawStructures, SmbiosStructureType.BiosInformation, T000_BiosInformation.Decode);
    SystemInformation = Decode<T001_SystemInformation>(rawStructures, SmbiosStructureType.SystemInformation, T001_SystemInformation.Decode);
    BaseboardInformation = Decode<T002_BaseboardInformation>(rawStructures, SmbiosStructureType.BaseboardInformation, T002_BaseboardInformation.Decode);
    ChassisInformation = Decode<T003_ChassisInformation>(rawStructures, SmbiosStructureType.ChassisInformation, T003_ChassisInformation.Decode);
    ProcessorInformation = Decode<T004_ProcessorInformation>(rawStructures, SmbiosStructureType.ProcessorInformation, T004_ProcessorInformation.Decode);
    CacheInformation = Decode<T007_CacheInformation>(rawStructures, SmbiosStructureType.CacheInformation, T007_CacheInformation.Decode);
    PhysicalMemoryArrays = Decode<T016_PhysicalMemoryArray>(rawStructures, SmbiosStructureType.PhysicalMemoryArray, T016_PhysicalMemoryArray.Decode);
    MemoryDevices = Decode<T017_MemoryDevice>(rawStructures, SmbiosStructureType.MemoryDevice, T017_MemoryDevice.Decode);
    MemoryArrayMappedAddresses = Decode<T019_MemoryArrayMappedAddress>(rawStructures, SmbiosStructureType.MemoryArrayMappedAddr, T019_MemoryArrayMappedAddress.Decode);
    MemoryDeviceMappedAddresses = Decode<T020_MemoryDeviceMappedAddress>(rawStructures, SmbiosStructureType.MemoryDeviceMappedAddr, T020_MemoryDeviceMappedAddress.Decode);
    SystemSlots = Decode<T009_SystemSlotInformation>(rawStructures, SmbiosStructureType.SystemSlots, T009_SystemSlotInformation.Decode);
    OnboardDevices = Decode<T041_OnboardDeviceExtendedInformation>(rawStructures, SmbiosStructureType.OnboardDevicesExtended, T041_OnboardDeviceExtendedInformation.Decode);
    PortConnectors = Decode<T008_PortConnectorInformation>(rawStructures, SmbiosStructureType.PortConnector, T008_PortConnectorInformation.Decode);
    Batteries = Decode<T022_PortableBatteryInformation>(rawStructures, SmbiosStructureType.PortableBattery, T022_PortableBatteryInformation.Decode);
    BuiltInPointingDevices = Decode<T021_BuiltInPointingDevice>(rawStructures, SmbiosStructureType.BuiltInPointingDevice, T021_BuiltInPointingDevice.Decode);
    BiosLanguages = Decode<T013_BiosLanguage>(rawStructures, SmbiosStructureType.BiosLanguage, T013_BiosLanguage.Decode);
    OemStrings = Decode<T011_OemStrings>(rawStructures, SmbiosStructureType.OemStrings, T011_OemStrings.Decode);
    // Memory error information may appear as Type 18 (32-bit) or Type 33 (64-bit).
    var mem18 = Decode<IMemoryErrorInformation>(rawStructures, SmbiosStructureType.MemoryErrorInfo32, s => T018_MemoryErrorInformation32.Decode(s));
    var mem33 = Decode<IMemoryErrorInformation>(rawStructures, SmbiosStructureType.MemoryErrorInfo64, s => T033_MemoryErrorInformation64.Decode(s));
    MemoryErrorInformation = mem18.Concat(mem33).ToList();
    MemoryErrorEntries = MemoryErrorInformation.Select(MemoryErrorEntry.From).ToList();
    VoltageProbes = Decode<T026_VoltageProbeInformation>(rawStructures, SmbiosStructureType.VoltageProbe, T026_VoltageProbeInformation.Decode);
    CoolingDevices = Decode<T027_CoolingDevice>(rawStructures, SmbiosStructureType.CoolingDevice, T027_CoolingDevice.Decode);
    TemperatureProbes = Decode<T028_TemperatureProbeInformation>(rawStructures, SmbiosStructureType.TemperatureProbe, T028_TemperatureProbeInformation.Decode);

    PowerSupplies = Decode<T039_SystemPowerSupply>(rawStructures, SmbiosStructureType.SystemPowerSupply, T039_SystemPowerSupply.Decode);
    SystemEventLogs = Decode<T015_SystemEventLog>(rawStructures, SmbiosStructureType.SystemEventLog, T015_SystemEventLog.Decode);

    _cachesByHandle = new Lazy<Dictionary<ushort, T007_CacheInformation>>(
        () => CacheInformation.ToDictionary(c => c.Handle));
    _memoryArraysByHandle = new Lazy<Dictionary<ushort, T016_PhysicalMemoryArray>>(
        () => PhysicalMemoryArrays.ToDictionary(a => a.Handle));
    _oemStringsMap = new Lazy<Dictionary<ushort, string?>>(
        () => OemStrings.ToDictionary(o => o.Handle, o => o.Strings.FirstOrDefault()));
    _oemStringsFullMap = new Lazy<Dictionary<ushort, IReadOnlyList<string>>>(
        () => OemStrings.ToDictionary(o => o.Handle, o => o.Strings));
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static IReadOnlyList<T> Decode<T>(
      IReadOnlyList<SmbiosRawStructure> all,
      SmbiosStructureType type,
      Func<SmbiosRawStructure, T> decoder) {
    var results = new List<T>();
    foreach (var s in all) {
      if (s.Type != type) continue;
      try { results.Add(decoder(s)); }
      catch { /* skip malformed structures */ }
    }
    return results.AsReadOnly();
  }

  public override string ToString() =>
      $"SMBIOS {MajorVersion}.{MinorVersion}, " +
      $"{RawStructures.Count} structures, " +
      $"System={System?.ProductName ?? "?"}, " +
      $"CPU={PopulatedProcessors.FirstOrDefault()?.ProcessorVersion ?? "?"}, " +
      $"RAM={TotalInstalledMemoryMiB} MiB";
}
