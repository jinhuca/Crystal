using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemory;

internal static class WmiPhysicalMemory {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.PhysicalMemory;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Physical Memory Specific Properties
  // ---------------------------------------------------------------------
  public const string Attributes = nameof(Attributes);
  public const string BankLabel = nameof(BankLabel);
  public const string Capacity = nameof(Capacity);
  public const string ConfiguredClockSpeed = nameof(ConfiguredClockSpeed);
  public const string ConfiguredVoltage = nameof(ConfiguredVoltage);
  public const string DataWidth = nameof(DataWidth);
  public const string DeviceLocator = nameof(DeviceLocator);
  public const string FormFactor = nameof(FormFactor);
  public const string InterleaveDataDepth = nameof(InterleaveDataDepth);
  public const string InterleavePosition = nameof(InterleavePosition);
  public const string MemoryType = nameof(MemoryType);
  public const string Model = nameof(Model);
  public const string Name = nameof(Name);
  public const string PartNumber = nameof(PartNumber);
  public const string PositionInRow = nameof(PositionInRow);
  public const string SerialNumber = nameof(SerialNumber);
  public const string SKU = nameof(SKU);
  public const string SMBIOSMemoryType = nameof(SMBIOSMemoryType);
  public const string Speed = nameof(Speed);
  public const string Tag = nameof(Tag);
  public const string TotalWidth = nameof(TotalWidth);
  public const string TypeDetail = nameof(TypeDetail);
}
