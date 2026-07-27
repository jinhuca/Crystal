using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.Share;

internal static class WmiShare {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Share;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Share Specific Properties
  // ---------------------------------------------------------------------
  public const string AccessMask = nameof(AccessMask);
  public const string AllowMaximum = nameof(AllowMaximum);
  public const string MaximumAllowed = nameof(MaximumAllowed);
  public const string Path = nameof(Path);
  public const string Type = nameof(Type);
}
