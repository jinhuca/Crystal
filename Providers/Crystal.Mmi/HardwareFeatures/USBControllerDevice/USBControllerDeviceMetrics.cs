namespace Crystal.Mmi.HardwareFeatures.USBControllerDevice;

// Win32_USBControllerDevice is a WMI association class (CIM_ControlledBy) relating a USB
// controller (Antecedent) to the logical device connected to it (Dependent), plus real
// scalar telemetry about the connection itself.
public record USBControllerDeviceMetrics(
  ushort? AccessState,
  string? Antecedent,          // CIM_USBController REF
  string? Dependent,           // CIM_LogicalDevice REF
  uint? NegotiatedDataWidth,   // bits
  ulong? NegotiatedSpeed,      // bits per second
  uint? NumberOfHardResets,
  uint? NumberOfSoftResets
) {
  // --- RUNTIME PRESENTATION HELPERS ---
  public string? ControllerDeviceId => ExtractDeviceId(Antecedent);
  public string? DeviceId => ExtractDeviceId(Dependent);

  private static string? ExtractDeviceId(string? path) =>
    string.IsNullOrEmpty(path) ? null : path.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"');
}
