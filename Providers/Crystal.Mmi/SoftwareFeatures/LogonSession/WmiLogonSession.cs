using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.LogonSession;

internal static class WmiLogonSession {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.LogonSession;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Logon Session Specific Properties
  // ---------------------------------------------------------------------
  public const string AuthenticationPackage = nameof(AuthenticationPackage);
  public const string LogonId = nameof(LogonId);
  public const string LogonType = nameof(LogonType);
  public const string StartTime = nameof(StartTime);
}
