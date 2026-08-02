using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.SystemDriver;

internal static class WmiSystemDriver {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.SystemDriver;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  // Matches the sibling Win32_Service convention in this codebase (WmiService.cs).
  public const string InstallationDate = nameof(InstallationDate);

  // ---------------------------------------------------------------------
  // System Driver Specific Properties
  // ---------------------------------------------------------------------
  public const string AcceptPause = nameof(AcceptPause);
  public const string AcceptStop = nameof(AcceptStop);
  public const string DesktopInteract = nameof(DesktopInteract);
  public const string DisplayName = nameof(DisplayName);
  public const string ErrorControl = nameof(ErrorControl);
  public const string ExitCode = nameof(ExitCode);
  public const string PathName = nameof(PathName);
  public const string ServiceSpecificExitCode = nameof(ServiceSpecificExitCode);
  public const string ServiceType = nameof(ServiceType);
  public const string Started = nameof(Started);
  public const string StartMode = nameof(StartMode);
  public const string StartName = nameof(StartName);
  public const string State = nameof(State);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TagId = nameof(TagId);
}
