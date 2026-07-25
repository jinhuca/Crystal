namespace Crystal.Mmi.HardwareFeatures.CurrentProbe;

// Real-world note: like Win32_TemperatureProbe, current implementations of WMI generally
// do not populate CurrentReading for sensors sourced from SMBIOS — expect nulls even on
// real hardware unless a vendor-specific provider fills it in.
public record CurrentProbeMetrics(
  int? Accuracy,
  ushort? Availability,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  int? CurrentReading,          // tenths of amps; reserved for future use
  string? Description,
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  DateTime? InstallDate,
  bool? IsLinear,
  uint? LastErrorCode,
  int? LowerThresholdCritical,
  int? LowerThresholdFatal,
  int? LowerThresholdNonCritical,
  int? MaxReadable,
  int? MinReadable,
  string? Name,
  int? NominalReading,
  int? NormalMax,
  int? NormalMin,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  uint? Resolution,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  int? Tolerance,
  int? UpperThresholdCritical,
  int? UpperThresholdFatal,
  int? UpperThresholdNonCritical
);
