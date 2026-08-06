using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Service;

internal static class WmiService {
  public const string ClassName = WmiClasses.Service;

  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  public const string AcceptPause = nameof(AcceptPause);
  public const string AcceptStop = nameof(AcceptStop);
  public const string CreationClassName = nameof(CreationClassName);
  public const string DesktopInteract = nameof(DesktopInteract);
  public const string ExitCode = nameof(ExitCode);
  public const string InstallationDate = nameof(InstallationDate);
  public const string DisplayName = nameof(DisplayName);
  public const string ErrorControl = nameof(ErrorControl);
  public const string PathName = nameof(PathName);
  public const string ProcessId = nameof(ProcessId);
  public const string ServiceType = nameof(ServiceType);
  public const string ServiceSpecificExitCode = nameof(ServiceSpecificExitCode);
  public const string StartMode = nameof(StartMode);
  public const string Started = nameof(Started);
  public const string StartName = nameof(StartName);
  public const string State = nameof(State);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemName = nameof(SystemName);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string TagId = nameof(TagId);
}