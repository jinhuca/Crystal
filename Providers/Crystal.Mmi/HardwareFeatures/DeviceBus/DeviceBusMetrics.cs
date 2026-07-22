namespace Crystal.Mmi.HardwareFeatures.DeviceBus;

// Win32_DeviceBus is a WMI association class (CIM_Dependency) — it has no scalar
// telemetry of its own. It simply relates a physical Win32_Bus instance (Antecedent)
// to the CIM_LogicalDevice instance that sits on that bus (Dependent). Both reference
// properties come back from WMI as embedded object-path strings, e.g.:
//   Antecedent: Win32_Bus.DeviceID="PCIBus"
//   Dependent:  Win32_PnPEntity.DeviceID="PCI\\VEN_..."
public record DeviceBusMetrics(
  string? Antecedent,  // Win32_Bus REF — the physical bus
  string? Dependent     // CIM_LogicalDevice REF — the device attached to that bus
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  // Extracts the bare DeviceID key out of the embedded WMI object-path reference.
  public string? BusDeviceId => ExtractDeviceId(Antecedent);
  public string? DeviceId => ExtractDeviceId(Dependent);

  private static string? ExtractDeviceId(string? path) =>
    string.IsNullOrEmpty(path) ? null : path.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"');
}
