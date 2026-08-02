using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Registry;

internal static class WmiRegistry {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Registry;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // Registry Specific Properties
  // ---------------------------------------------------------------------
  public const string CurrentSize = nameof(CurrentSize);
  public const string MaximumSize = nameof(MaximumSize);
  public const string ProposedSize = nameof(ProposedSize);
}
