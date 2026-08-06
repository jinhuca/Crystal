using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DMAChannel;

internal static class WmiDMAChannel {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.DMAChannel;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // DMA Channel Specific Properties
  // (Note: this class is CIM_DMA-derived, not CIM_LogicalDevice-derived, so it
  // uses CSCreationClassName/CSName instead of SystemCreationClassName/SystemName.)
  // ---------------------------------------------------------------------
  public const string AddressSize = nameof(AddressSize);
  public const string Availability = nameof(Availability);
  public const string BurstMode = nameof(BurstMode);
  public const string ByteMode = nameof(ByteMode);
  public const string ChannelTiming = nameof(ChannelTiming);
  public const string CSCreationClassName = nameof(CSCreationClassName);
  public const string CSName = nameof(CSName);
  public const string DMAChannel = nameof(DMAChannel);
  public const string MaxTransferSize = nameof(MaxTransferSize);
  public const string Port = nameof(Port);
  public const string TransferWidths = nameof(TransferWidths);
  public const string TypeCTiming = nameof(TypeCTiming);
  public const string WordMode = nameof(WordMode);
}
