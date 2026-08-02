using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Group;

internal static class WmiGroup {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Group;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Group (Win32_Account) Specific Properties
  // ---------------------------------------------------------------------
  public const string Domain = nameof(Domain);
  public const string LocalAccount = nameof(LocalAccount);
  public const string SID = nameof(SID);
  public const string SIDType = nameof(SIDType);
}
