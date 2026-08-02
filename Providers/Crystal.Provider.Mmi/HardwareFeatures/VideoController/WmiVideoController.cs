using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.VideoController;

internal static class WmiVideoController {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.VideoController;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // Video Controller Specific Properties
  // ---------------------------------------------------------------------
  public const string AdapterCompatibility = nameof(AdapterCompatibility);
  public const string AdapterDACType = nameof(AdapterDACType);
  public const string AdapterRAM = nameof(AdapterRAM);
  public const string Availability = nameof(Availability);
  public const string Architecture = nameof(Architecture);
  public const string ColorTableEntries = nameof(ColorTableEntries);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string CurrentNumberOfColors = nameof(CurrentNumberOfColors);
  public const string CurrentNumberOfColumns = nameof(CurrentNumberOfColumns);
  public const string CurrentNumberOfRows = nameof(CurrentNumberOfRows);
  public const string DitherType = nameof(DitherType);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string DriverDate = nameof(DriverDate);
  public const string DriverVersion = nameof(DriverVersion);
  public const string ICMIntent = nameof(ICMIntent);
  public const string ICMMethod = nameof(ICMMethod);
  public const string InfDate = nameof(InfDate);
  public const string InstallationDate = nameof(InstallationDate);
  public const string InfFilename = nameof(InfFilename);
  public const string InfSection = nameof(InfSection);
  public const string InstalledDisplayDrivers = nameof(InstalledDisplayDrivers);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxRefreshRate = nameof(MaxRefreshRate);
  public const string MaxMemorySupported = nameof(MaxMemorySupported);
  public const string MinRefreshRate = nameof(MinRefreshRate);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string CurrentBitsPerPixel = nameof(CurrentBitsPerPixel);
  public const string CurrentHorizontalResolution = nameof(CurrentHorizontalResolution);
  public const string CurrentVerticalResolution = nameof(CurrentVerticalResolution);
  public const string CurrentRefreshRate = nameof(CurrentRefreshRate);
  public const string VideoModeDescription = nameof(VideoModeDescription);
  public const string VideoProcessor = nameof(VideoProcessor);
  public const string VideoArchitecture = nameof(VideoArchitecture);
  public const string VideoMemoryType = nameof(VideoMemoryType);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
}