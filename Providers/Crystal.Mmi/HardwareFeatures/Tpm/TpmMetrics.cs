namespace Crystal.Mmi.HardwareFeatures.Tpm;

public record TpmMetrics(
  string? Caption,
  string? Description,
  string? InstanceName,
  bool? IsActivated_InitialValue,
  bool? IsEnabled_InitialValue,
  bool? IsOwned_InitialValue,
  uint? ManufacturerId,
  string? ManufacturerIdTxt,
  string? ManufacturerVersion,
  string? ManufacturerVersionFull20,
  string? ManufacturerVersionInfo,
  string? PhysicalPresenceVersionInfo,
  string? SpecVersion,
  string? Status
);
