namespace Crystal.Mmi.HardwareFeatures.SCSIControllerDevice;

// Win32_SCSIControllerDevice is a WMI association class (CIM_ControlledBy) relating a SCSI
// controller (Antecedent) to the logical device connected to it (Dependent), plus real
// scalar telemetry about the connection itself.
public record SCSIControllerDeviceMetrics(
  ushort? AccessState,
  string? Antecedent,          // Win32_SCSIController REF
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
