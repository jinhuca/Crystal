namespace Crystal.Provider.Mmi.HardwareFeatures.TemperatureProbe;

// Real-world note: current WMI implementations do not populate CurrentReading (or the
// other sint32 sensor fields) since real-time readings can't be extracted from SMBIOS
// tables — most instances will report these as null even on real hardware.
public record TemperatureProbeMetrics(
  int? Accuracy,
  ushort? Availability,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  int? CurrentReading,          // tenths of degrees centigrade; reserved for future use
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
