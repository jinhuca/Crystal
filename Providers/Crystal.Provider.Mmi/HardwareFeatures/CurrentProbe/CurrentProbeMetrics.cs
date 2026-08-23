namespace Crystal.Provider.Mmi.HardwareFeatures.CurrentProbe;

/// <summary>
/// Represents a current probe sensor, which measures the electrical current (in amps) flowing through a circuit.
/// This class is a record type that encapsulates various properties related to the current probe, including its 
/// accuracy, availability, device ID, and threshold values.
/// Real-world note: like Win32_TemperatureProbe, current implementations of WMI generally
/// do not populate CurrentReading for sensors sourced from SMBIOS — expect nulls even on
/// real hardware unless a vendor-specific provider fills it in.
/// </summary>
/// <param name="Accuracy">The accuracy of the current probe.</param>
/// <param name="Availability">The availability of the current probe.</param>
/// <param name="Caption">The caption of the current probe.</param>
/// <param name="ConfigManagerErrorCode">The configuration manager error code.</param>
/// <param name="ConfigManagerUserConfig">Indicates whether the configuration is user-configured.</param>
/// <param name="CreationClassName">The creation class name.</param>
/// <param name="CurrentReading">The current reading of the probe.</param>
/// <param name="Description">The description of the current probe.</param>
/// <param name="DeviceID">The device ID of the current probe.</param>
/// <param name="ErrorCleared">Indicates whether the error has been cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="InstallDate">The installation date of the current probe.</param>
/// <param name="IsLinear">Indicates whether the probe is linear.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="LowerThresholdCritical">The critical lower threshold.</param>
/// <param name="LowerThresholdFatal">The fatal lower threshold.</param>
/// <param name="LowerThresholdNonCritical">The non-critical lower threshold.</param>
/// <param name="MaxReadable">The maximum readable value.</param>
/// <param name="MinReadable">The minimum readable value.</param>
/// <param name="Name">The name of the current probe.</param>
/// <param name="NominalReading">The nominal reading of the probe.</param>
/// <param name="NormalMax">The normal maximum value.</param>
/// <param name="NormalMin">The normal minimum value.</param>
/// <param name="PNPDeviceID">The PNP device ID.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities.</param>
/// <param name="PowerManagementSupported">Indicates whether power management is supported.</param>
/// <param name="Resolution">The resolution of the current probe.</param>
/// <param name="Status">The status of the current probe.</param>
/// <param name="StatusInfo">The status information of the current probe.</param>
/// <param name="SystemCreationClassName">The system creation class name.</param>
/// <param name="SystemName">The system name.</param>
/// <param name="Tolerance">The tolerance of the current probe.</param>
/// <param name="UpperThresholdCritical">The critical upper threshold.</param>
/// <param name="UpperThresholdFatal">The fatal upper threshold.</param>
/// <param name="UpperThresholdNonCritical">The non-critical upper threshold.</param>
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
