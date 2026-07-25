using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.VoltageProbe;

internal static class WmiVoltageProbe {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.VoltageProbe;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // Voltage Probe Specific Properties
  // ---------------------------------------------------------------------
  public const string Accuracy = nameof(Accuracy);
  public const string Availability = nameof(Availability);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CurrentReading = nameof(CurrentReading);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string IsLinear = nameof(IsLinear);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string LowerThresholdCritical = nameof(LowerThresholdCritical);
  public const string LowerThresholdFatal = nameof(LowerThresholdFatal);
  public const string LowerThresholdNonCritical = nameof(LowerThresholdNonCritical);
  public const string MaxReadable = nameof(MaxReadable);
  public const string MinReadable = nameof(MinReadable);
  public const string NominalReading = nameof(NominalReading);
  public const string NormalMax = nameof(NormalMax);
  public const string NormalMin = nameof(NormalMin);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string Resolution = nameof(Resolution);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string Tolerance = nameof(Tolerance);
  public const string UpperThresholdCritical = nameof(UpperThresholdCritical);
  public const string UpperThresholdFatal = nameof(UpperThresholdFatal);
  public const string UpperThresholdNonCritical = nameof(UpperThresholdNonCritical);
}
