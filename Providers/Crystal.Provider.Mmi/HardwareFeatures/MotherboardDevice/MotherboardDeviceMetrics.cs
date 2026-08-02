namespace Crystal.Provider.Mmi.HardwareFeatures.MotherboardDevice;

public record MotherboardDeviceMetrics(
  ushort? Availability,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Name,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? PrimaryBusType,       // e.g., "PCI"
  string? RevisionNumber,       // e.g., "00"
  string? SecondaryBusType,     // e.g., "ISA"
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
);
