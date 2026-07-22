using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.NetworkClient;

internal static class WmiNetworkClient {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.NetworkClient;

  // ---------------------------------------------------------------------
  // Properties (all shared with CIM_ManagedSystemElement / CIM_LogicalElement)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;
}
