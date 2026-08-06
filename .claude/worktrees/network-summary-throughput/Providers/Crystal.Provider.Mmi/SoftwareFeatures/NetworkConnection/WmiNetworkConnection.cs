using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.NetworkConnection;

internal static class WmiNetworkConnection {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.NetworkConnection;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Network Connection Specific Properties
  // ---------------------------------------------------------------------
  public const string AccessMask = nameof(AccessMask);
  public const string Comment = nameof(Comment);
  public const string ConnectionState = nameof(ConnectionState);
  public const string ConnectionType = nameof(ConnectionType);
  public const string DisplayType = nameof(DisplayType);
  public const string LocalName = nameof(LocalName);
  public const string Persistent = nameof(Persistent);
  public const string ProviderName = nameof(ProviderName);
  public const string RemoteName = nameof(RemoteName);
  public const string RemotePath = nameof(RemotePath);
  public const string ResourceType = nameof(ResourceType);
  public const string UserName = nameof(UserName);
}
