namespace Crystal.Mmi.SoftwareFeatures.NetworkProtocol;

public record NetworkProtocolMetrics(
  string? Caption,
  bool? ConnectionlessService,
  string? Description,
  bool? GuaranteesDelivery,
  bool? GuaranteesSequencing,
  DateTime? InstallDate,
  uint? MaximumAddressSize,
  uint? MaximumMessageSize,
  bool? MessageOriented,
  uint? MinimumAddressSize,
  string? Name,
  bool? PseudoStreamOriented,
  string? Status,
  bool? SupportsBroadcasting,
  bool? SupportsConnectData,
  bool? SupportsDisconnectData,
  bool? SupportsEncryption,
  bool? SupportsExpeditedData,
  bool? SupportsFragmentation,
  bool? SupportsGracefulClosing,
  bool? SupportsGuaranteedBandwidth,
  bool? SupportsMulticasting,
  bool? SupportsQualityofService
);
