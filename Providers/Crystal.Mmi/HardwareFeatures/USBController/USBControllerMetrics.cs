namespace Crystal.Mmi.HardwareFeatures.USBController;

public record USBControllerMetrics(
  ushort? Availability,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Manufacturer,
  uint? MaxNumberControlled,
  string? Name,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  ushort? ProtocolSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  DateTime? TimeOfLastReset) {
  /// <summary>
  /// Human-readable description of <see cref="ProtocolSupported"/>.
  /// </summary>
  public string? ProtocolSupportedName => ProtocolSupported switch {
    1 => "Other",
    2 => "Unknown",
    3 => "EISA",
    4 => "ISA",
    5 => "PCI",
    16 => "Universal Serial Bus",
    17 => "Parallel Protocol",
    37 => "IDE",
    43 => "AGP",
    _ => null
  };
}
