using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.QuickFixEngineering;

internal static class WmiQuickFixEngineering {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.QuickFixEngineering;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Quick Fix Engineering Specific Properties
  // ---------------------------------------------------------------------
  public const string CSName = nameof(CSName);
  public const string FixComments = nameof(FixComments);
  public const string HotFixID = nameof(HotFixID);
  public const string InstalledBy = nameof(InstalledBy);
  public const string InstalledOn = nameof(InstalledOn);
  public const string ServicePackInEffect = nameof(ServicePackInEffect);
}
