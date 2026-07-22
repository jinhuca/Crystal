using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.NetworkProtocol;

internal static class WmiNetworkProtocol {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.NetworkProtocol;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Network Protocol Specific Properties
  // ---------------------------------------------------------------------
  public const string ConnectionlessService = nameof(ConnectionlessService);
  public const string GuaranteesDelivery = nameof(GuaranteesDelivery);
  public const string GuaranteesSequencing = nameof(GuaranteesSequencing);
  public const string MaximumAddressSize = nameof(MaximumAddressSize);
  public const string MaximumMessageSize = nameof(MaximumMessageSize);
  public const string MessageOriented = nameof(MessageOriented);
  public const string MinimumAddressSize = nameof(MinimumAddressSize);
  public const string PseudoStreamOriented = nameof(PseudoStreamOriented);
  public const string SupportsBroadcasting = nameof(SupportsBroadcasting);
  public const string SupportsConnectData = nameof(SupportsConnectData);
  public const string SupportsDisconnectData = nameof(SupportsDisconnectData);
  public const string SupportsEncryption = nameof(SupportsEncryption);
  public const string SupportsExpeditedData = nameof(SupportsExpeditedData);
  public const string SupportsFragmentation = nameof(SupportsFragmentation);
  public const string SupportsGracefulClosing = nameof(SupportsGracefulClosing);
  public const string SupportsGuaranteedBandwidth = nameof(SupportsGuaranteedBandwidth);
  public const string SupportsMulticasting = nameof(SupportsMulticasting);
  public const string SupportsQualityofService = nameof(SupportsQualityofService);
}
