namespace Crystal.Provider.Mmi.HardwareFeatures.IDEControllerDevice;

// Win32_IDEControllerDevice is a WMI association class (CIM_ControlledBy) relating an IDE
// controller (Antecedent) to the logical device connected to it, e.g. a disk drive
// (Dependent). Unlike the simpler Antecedent/Dependent-only associations (DeviceBus,
// DeviceSettings), this one also carries real scalar telemetry about the connection itself.
public record IDEControllerDeviceMetrics(
  ushort? AccessState,        // whether the controller is actively commanding/accessing the device
  string? Antecedent,          // Win32_IDEController REF
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
