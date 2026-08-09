using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemory;
public static class WmiPhysicalMemoryExtensions {
  private const string WmiClassName = "Win32_PhysicalMemory";

  public static async Task<IReadOnlyList<PhysicalMemoryMetrics>> ToSafePhysicalMemoryMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance driver data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) return Array.Empty<PhysicalMemoryMetrics>();

      var results = new List<PhysicalMemoryMetrics>(instancesData.Count);

      // 2. Loop through every detected physical RAM stick module sequential pass
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new PhysicalMemoryMetrics(
            Attributes: (ushort?)GetInt("Attributes"),
            BankLabel: GetStr("BankLabel"),
            Capacity: GetULong("Capacity"),
            Caption: GetStr("Caption"),
            ConfiguredClockSpeed: (uint?)GetInt("ConfiguredClockSpeed"),
            ConfiguredVoltage: (uint?)GetInt("ConfiguredVoltage"),
            CreationClassName: GetStr("CreationClassName"),
            DataWidth: (ushort?)GetInt("DataWidth"),
            Description: GetStr("Description"),
            DeviceLocator: GetStr("DeviceLocator"),
            FormFactor: (ushort?)GetInt("FormFactor"),
            HotSwappable: GetBool("HotSwappable"),
            InstallDate: GetDate("InstallationDate"),
            InterleaveDataDepth: (ushort?)GetInt("InterleaveDataDepth"),
            InterleavePosition: (uint?)GetInt("InterleavePosition"),
            Manufacturer: GetStr("Manufacturer"),
            MaxVoltage: (uint?)GetInt("MaxVoltage"),
            MemoryType: (ushort?)GetInt("MemoryType"),
            MinVoltage: (uint?)GetInt("MinVoltage"),
            Model: GetStr("Model"),
            Name: GetStr("Name"),
            OtherIdentifyingInfo: GetStr("OtherIdentifyingInfo"),
            PartNumber: GetStr("PartNumber"),
            PositionInRow: (uint?)GetInt("PositionInRow"),
            PoweredOn: GetBool("PoweredOn"),
            Removable: GetBool("Removable"),
            Replaceable: GetBool("Replaceable"),
            SerialNumber: GetStr("SerialNumber"),
            SKU: GetStr("SKU"),
            Speed: (ushort?)GetInt("Speed"),
            Status: GetStr("Status"),
            Tag: GetStr("Tag"),
            TotalWidth: (ushort?)GetInt("TotalWidth"),
            TypeDetail: (ushort?)GetInt("TypeDetail"),
            Version: GetStr("Version"),
            SMBIOSMemoryType: (ushort?)GetInt("SMBIOSMemoryType")
        ));
      }

      return results;
    }
    catch {
      return Array.Empty<PhysicalMemoryMetrics>();
    }
  }
}
