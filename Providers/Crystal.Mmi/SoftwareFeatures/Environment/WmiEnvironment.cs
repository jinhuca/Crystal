using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.Environment;

internal static class WmiEnvironment {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Environment;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Environment Specific Properties
  // ---------------------------------------------------------------------
  public const string SystemVariable = nameof(SystemVariable);
  public const string UserName = nameof(UserName);
  public const string VariableValue = nameof(VariableValue);
}
