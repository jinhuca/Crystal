namespace Crystal.Provider.Mmi.HardwareFeatures.AssociatedProcessorMemory;

// Win32_AssociatedProcessorMemory is a WMI association class (CIM_AssociatedProcessorMemory)
// relating a processor's cache memory (Antecedent) to the processor it serves (Dependent),
// plus the bus speed between them.
public record AssociatedProcessorMemoryMetrics(
  string? Antecedent,   // Win32_CacheMemory REF
  uint? BusSpeed,        // MHz
  string? Dependent      // Win32_Processor REF
) {
  // --- RUNTIME PRESENTATION HELPERS ---
  public string? CacheMemoryDeviceId => ExtractDeviceId(Antecedent);
  public string? ProcessorDeviceId => ExtractDeviceId(Dependent);

  private static string? ExtractDeviceId(string? path) =>
    string.IsNullOrEmpty(path) ? null : path.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"');
}
