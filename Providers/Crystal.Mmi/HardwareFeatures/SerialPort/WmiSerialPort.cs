using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.SerialPort;

internal static class WmiSerialPort {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.SerialPort;

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
  // Serial Port Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string Binary = nameof(Binary);
  public const string Capabilities = nameof(Capabilities);
  public const string CapabilityDescriptions = nameof(CapabilityDescriptions);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxBaudRate = nameof(MaxBaudRate);
  public const string MaximumInputBufferSize = nameof(MaximumInputBufferSize);
  public const string MaximumOutputBufferSize = nameof(MaximumOutputBufferSize);
  public const string MaxNumberControlled = nameof(MaxNumberControlled);
  public const string OSAutoDiscovered = nameof(OSAutoDiscovered);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ProtocolSupported = nameof(ProtocolSupported);
  public const string ProviderType = nameof(ProviderType);
  public const string SettableBaudRate = nameof(SettableBaudRate);
  public const string SettableDataBits = nameof(SettableDataBits);
  public const string SettableFlowControl = nameof(SettableFlowControl);
  public const string SettableParity = nameof(SettableParity);
  public const string SettableParityCheck = nameof(SettableParityCheck);
  public const string SettableRLSD = nameof(SettableRLSD);
  public const string SettableStopBits = nameof(SettableStopBits);
  public const string StatusInfo = nameof(StatusInfo);
  public const string Supports16BitMode = nameof(Supports16BitMode);
  public const string SupportsDTRDSR = nameof(SupportsDTRDSR);
  public const string SupportsElapsedTimeouts = nameof(SupportsElapsedTimeouts);
  public const string SupportsIntTimeouts = nameof(SupportsIntTimeouts);
  public const string SupportsParityCheck = nameof(SupportsParityCheck);
  public const string SupportsRLSD = nameof(SupportsRLSD);
  public const string SupportsRTSCTS = nameof(SupportsRTSCTS);
  public const string SupportsSpecialCharacters = nameof(SupportsSpecialCharacters);
  public const string SupportsXOnXOff = nameof(SupportsXOnXOff);
  public const string SupportsXOnXOffSet = nameof(SupportsXOnXOffSet);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TimeOfLastReset = nameof(TimeOfLastReset);
}
