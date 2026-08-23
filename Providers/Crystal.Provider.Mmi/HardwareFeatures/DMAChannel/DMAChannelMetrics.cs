namespace Crystal.Provider.Mmi.HardwareFeatures.DMAChannel;

/// <summary>
/// Represents the metrics of a DMA channel, including its configuration and status information.
/// </summary>
/// <param name="AddressSize">The size of the address space.</param>
/// <param name="Availability">The availability of the DMA channel.</param>
/// <param name="BurstMode">Indicates whether the DMA channel is in burst mode.</param>
/// <param name="ByteMode">The byte mode of the DMA channel.</param>
/// <param name="Caption">A short description of the DMA channel.</param>
/// <param name="ChannelTiming">The timing information for the DMA channel.</param>
/// <param name="CreationClassName">The name of the class that created the instance.</param>
/// <param name="CSCreationClassName">The name of the class that created the instance.</param>
/// <param name="CSName">The name of the computer system that contains the instance.</param>
/// <param name="Description">A description of the DMA channel.</param>
/// <param name="DMAChannel">The channel number, part of the key.</param>
/// <param name="InstallDate">The date and time when the DMA channel was installed.</param>
/// <param name="MaxTransferSize">The maximum transfer size for the DMA channel.</param>
/// <param name="Name">The name of the DMA channel.</param>
/// <param name="Port">The port number for the DMA channel.</param>
/// <param name="Status">The status of the DMA channel.</param>
/// <param name="TransferWidths">The transfer widths for the DMA channel.</param>
/// <param name="TypeCTiming">The timing information for the DMA channel.</param>
/// <param name="WordMode">The word mode of the DMA channel.</param>
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
