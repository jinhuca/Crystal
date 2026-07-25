namespace Crystal.Mmi.HardwareFeatures.DMAChannel;

public record DMAChannelMetrics(
  ushort? AddressSize,
  ushort? Availability,
  bool? BurstMode,
  ushort? ByteMode,
  string? Caption,
  ushort? ChannelTiming,
  string? CreationClassName,
  string? CSCreationClassName,
  string? CSName,
  string? Description,
  uint? DMAChannel,       // channel number, part of the key
  DateTime? InstallDate,
  uint? MaxTransferSize,
  string? Name,
  uint? Port,
  string? Status,
  ushort[]? TransferWidths,
  ushort? TypeCTiming,
  ushort? WordMode
);
