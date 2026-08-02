using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.GroupUser;

internal static class WmiGroupUser {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.GroupUser;

  // ---------------------------------------------------------------------
  // Association Reference Properties
  // ---------------------------------------------------------------------
  public const string GroupComponent = nameof(GroupComponent);
  public const string PartComponent = nameof(PartComponent);
}
