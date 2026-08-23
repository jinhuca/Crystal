namespace Crystal.Provider.Mmi.HardwareFeatures.AssociatedProcessorMemory;

/// <summary>
/// Represents the association between a processor's cache memory and the processor it serves, including the bus speed between them.
/// Win32_AssociatedProcessorMemory is a WMI association class (CIM_AssociatedProcessorMemory) relating a processor's cache memory 
/// (Antecedent) to the processor it serves (Dependent), plus the bus speed between them.
/// </summary>
/// <param name="Antecedent">a reference to the cache memory (Win32_CacheMemory)</param>
/// <param name="BusSpeed">the bus speed between the cache memory and the processor (MHz)</param>
/// <param name="Dependent">a reference to the processor (Win32_Processor)</param>
public record AssociatedProcessorMemoryMetrics(
  string? Antecedent,     // Win32_CacheMemory REF
  uint? BusSpeed,         // MHz
  string? Dependent       // Win32_Processor REF
) {
  // --- RUNTIME PRESENTATION HELPERS ---
  public string? CacheMemoryDeviceId => ExtractDeviceId(Antecedent);
  public string? ProcessorDeviceId => ExtractDeviceId(Dependent);

  private static string? ExtractDeviceId(string? path) =>
    string.IsNullOrEmpty(path) ? null : path.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"');
}
