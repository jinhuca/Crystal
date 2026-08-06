using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.NetworkAdapter;

internal static class WmiNetworkAdapter {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.NetworkAdapter;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Name = CommonWmiProperties.Name;
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // Network Adapter Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string GUID = nameof(GUID);
  public const string Index = nameof(Index);
  public const string InstallDate = nameof(InstallDate);
  public const string Installed = nameof(Installed);
  public const string InterfaceIndex = nameof(InterfaceIndex);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MACAddress = nameof(MACAddress);
  public const string MaxNumberControlled = nameof(MaxNumberControlled);
  public const string MaxSpeed = nameof(MaxSpeed);
  public const string NetConnectionID = nameof(NetConnectionID);
  public const string NetConnectionStatus = nameof(NetConnectionStatus);
  public const string NetEnabled = nameof(NetEnabled);
  public const string PhysicalAdapter = nameof(PhysicalAdapter);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ProviderName = nameof(ProviderName);
  public const string ProductName = nameof(ProductName);
  public const string ServiceName = nameof(ServiceName);
  public const string Speed = nameof(Speed);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TimeOfLastReset = nameof(TimeOfLastReset);
}